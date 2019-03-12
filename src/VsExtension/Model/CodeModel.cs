using InliningAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsExtension.Model
{
    public interface ICodeModel
    {
        MethodCall GetMethodCall(Cache doc, TextSpan textSpan);
    }

    [Export(typeof(ICodeModel))]
    public class CodeModel : ICodeModel
    {
        [Import]
        public IAnalyzerModel AnalyzerModel;

        public CodeModel()
        {
        }

        public MethodCall GetMethodCall(Cache doc, TextSpan textSpan)
        {
            if (AnalyzerModel.CallGraph == null)
                return null;

            var node = GetExpression(doc.SyntaxRoot.FindNode(textSpan));
            if (node is ConstructorDeclarationSyntax || node is MethodDeclarationSyntax || node is PropertyDeclarationSyntax)
                return null;

            var symbol = GetSymbol(doc, node);
            if (symbol == null)
                return null;

            if (symbol.Kind == SymbolKind.Local && node.Parent is ElementAccessExpressionSyntax)
            {
                symbol = GetSymbol(doc, node.Parent);
                if (symbol == null)
                    return null;
            }

            if (symbol.Kind == SymbolKind.Method)
            {
                return ResolveMethod(doc, node, symbol);
            }
            if (symbol.Kind == SymbolKind.Property)
            {
                string methodName = GetPropertyAccessorMethod(node, symbol);
                string typeName = symbol.ContainingType.GetFullTypename();
                
                return GetMethodCall(doc, node, methodName, typeName);
            }
            if (symbol.Kind == SymbolKind.NamedType)
            {
                var objectCreationExpression = node.FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();
                if (objectCreationExpression == null)
                    return null;

                var originalDefinition = symbol.OriginalDefinition;

                symbol = doc.SemanticModel.GetSymbolInfo(objectCreationExpression).Symbol;

                if (originalDefinition != symbol.ContainingType.OriginalDefinition)
                    return null;

                string methodName = ".ctor";
                string typeName = symbol.ContainingType.GetFullTypename();
                string signature = SignatureResolver.BuildSignature((IMethodSymbol)symbol);

                return GetMethodCall(doc, node, methodName, typeName);
            }
            return null;
        }

        private MethodCall ResolveMethod(Cache doc, SyntaxNode node, ISymbol symbol)
        {
            string methodName = symbol.Name;
            string typeName = symbol.ContainingType.GetFullTypename();
            var methodSymbol = (IMethodSymbol)symbol;
            string signature = SignatureResolver.BuildSignature(methodSymbol);

            MethodCall result = GetMethodCall(doc, node, methodName, typeName, signature);
            if (result != null)
                return result;

            if (!symbol.IsOverride || methodSymbol.OverriddenMethod == null)
                return null;

            return ResolveMethod(doc, node, methodSymbol.OverriddenMethod);
        }

        private MethodCall GetMethodCall(Cache doc, SyntaxNode node, string methodName, string typeName, string signature = null)
        {
            var (namespaceName, containingType, containingMethod, containingSignature) = GetContainingMethodName(doc, node);

            if (containingType == null || containingMethod == null)
                return null;

            var jitType = AnalyzerModel.CallGraph.GetJitType(namespaceName + "." + containingType);
            if (jitType == null)
                return null;

            var jitMethod = jitType.GetMethod(containingMethod, containingSignature);
            if (jitMethod == null)
                return null;

            return jitMethod.GetMethodCall(typeName, methodName, signature);
        }

        private string GetPropertyAccessorMethod(SyntaxNode node, ISymbol symbol)
        {
            var propertySymbol = (IPropertySymbol)symbol;
            bool isGetAccess = true;
            if (!propertySymbol.ReturnsByRef)
            {
                var assignment = node.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
                if (assignment != null)
                {
                    if (assignment.Left.DescendantNodesAndSelf().Contains(node))
                        isGetAccess = false; // if it appears on the left side of an assignment, it is a setter, otherwise getter
                }
            }

            return (isGetAccess ? propertySymbol.GetMethod : propertySymbol.SetMethod)?.MetadataName;
        }

        private (string namespaceName, string type, string method, string signature) GetContainingMethodName(Cache doc, SyntaxNode node)
        {
            var (member, memberName, signature) = GetMemberContainer(doc, node);
            if (member == null)
                return (null, null, null, null);

            var type = GetContainer<TypeDeclarationSyntax>(member);
            string typeName = type.Identifier.Text;

            var namespaceNode = GetContainer<NamespaceDeclarationSyntax>(type);
            string namespaceName = namespaceNode?.Name.GetText().ToString().Trim();

            var outerClassNode = GetContainer<TypeDeclarationSyntax>(type.Parent);
            if (outerClassNode != null)
                typeName = outerClassNode.Identifier.Text + "+" + typeName;

            return (namespaceName, typeName, memberName, signature);
        }

        private (SyntaxNode member, string memberName, string signature) GetMemberContainer(Cache doc, SyntaxNode node)
        {
            var method = GetContainer<MethodDeclarationSyntax>(node);
            if (method != null)
                return (method, GetSymbol(doc, method)?.Name, GetSignature(doc, method));

            var constructor = GetContainer<ConstructorDeclarationSyntax>(node);
            if (constructor != null)
                return (constructor, ".ctor", GetSignature(doc, constructor));

            var property = GetContainer<PropertyDeclarationSyntax>(node);
            if (property != null)
            {
                var accessor = GetContainer<AccessorDeclarationSyntax>(node);
                if (accessor != null)
                {
                    var propertySymbol = GetSymbol(doc, accessor);
                    return (property, propertySymbol.Name, GetSignature(doc, accessor));
                }
                var arrowExpression = GetContainer<ArrowExpressionClauseSyntax>(node);
                if (arrowExpression != null)
                {
                    var propertySymbol = GetSymbol(doc, property) as IPropertySymbol;
                    return (property, propertySymbol.GetMethod.Name, GetSignature(doc, property));
                }
            }

            var indexer = GetContainer<IndexerDeclarationSyntax>(node);
            if (indexer != null)
            {
                var indexerSymbol = GetSymbol(doc, indexer) as IPropertySymbol;
                return (indexer, indexerSymbol.Name.Replace("this[]", "get_Item"), GetSignature(doc, indexer));
            }

            var operatorContainer = GetContainer<OperatorDeclarationSyntax>(node);
            if (operatorContainer != null)
            {
                var operatorSymbol = GetSymbol(doc, operatorContainer);
                return (operatorContainer, operatorSymbol.Name, SignatureResolver.BuildSignature((IMethodSymbol)operatorSymbol));
            }
            return (null, null, null);
        }

        private ISymbol GetSymbol(Cache doc, SyntaxNode node)
        {
            var symbol = doc.SemanticModel.GetSymbolInfo(node).Symbol;
            if (symbol != null)
                return symbol;

            return doc.SemanticModel.GetDeclaredSymbol(node);
        }

        private string GetSignature(Cache doc, SyntaxNode node)
        {
            var symbol = GetSymbol(doc, node);

            if (symbol == null)
                return null;

            if (symbol.Kind == SymbolKind.Method)
            {
                return SignatureResolver.BuildSignature((IMethodSymbol)symbol);
            }
            if (symbol.Kind == SymbolKind.Property)
            {
                return SignatureResolver.BuildSignature((IPropertySymbol)symbol);
            }
            return null;
        }

        private T GetContainer<T>(SyntaxNode node) where T : SyntaxNode
        {
            if (node == null)
                return null;

            var declaration = node as T;
            if (declaration != null)
            {
                return declaration;
            }

            return GetContainer<T>(node.Parent);
        }
        
        private SyntaxNode GetExpression(SyntaxNode node)
        {
            if (node.Kind() == SyntaxKind.Argument)
            {
                return ((ArgumentSyntax)node).Expression;
            }
            else if (node.Kind() == SyntaxKind.AttributeArgument)
            {
                return ((AttributeArgumentSyntax)node).Expression;
            }
            return node;
        }
    }
}
