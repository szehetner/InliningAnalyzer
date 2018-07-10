using InliningAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsExtension.Model
{
    public class TargetScopeResolver
    {
        public static TargetScope GetTargetScope(SemanticModel model, SyntaxTree tree, int line, int column)
        {
            var symbol = GetSymbolByLocation(model, tree, line, column);
            if (symbol == null)
                return null;

            if (symbol is ITypeSymbol typeSymbol)
                return GetTypeScope(typeSymbol);

            return GetMethodScope(symbol);
        }

        private static TargetScope GetTypeScope(ITypeSymbol symbol)
        {
            return new TargetScope(ScopeType.Class, symbol.GetFullTypename());
        }

        private static TargetScope GetMethodScope(ISymbol symbol)
        {
            var methodSymbol = symbol as IMethodSymbol;
            var propertySymbol = symbol as IPropertySymbol;

            StringBuilder builder = new StringBuilder();
            builder.Append(symbol.ContainingType.GetFullTypename(";", true));
            builder.Append("|");
            if (symbol.Name == ".ctor")
                builder.Append("ctor");
            else if (propertySymbol != null && propertySymbol.IsIndexer)
                builder.Append(propertySymbol.Name.Replace("this[]", "get_Item"));
            else
                builder.Append(symbol.Name);

            var parameters = methodSymbol != null ? methodSymbol.Parameters : propertySymbol.Parameters;

            builder.Append("(");
            builder.Append(string.Join(",", parameters.Select(p => p.Type.GetFullTypename(";", true))));
            builder.Append(")");

            return new TargetScope(ScopeType.Method, builder.ToString());
        }

        public static ISymbol GetSymbolByLocation(SemanticModel model, SyntaxTree tree, int line, int column)
        {
            var textSpan = new TextSpan(column, 0);
            var lineSpan = tree.GetText().Lines[line - 1];

            var nodes = tree.GetRoot().DescendantNodes().Where(n => n.Span.IntersectsWith(lineSpan.Span));
            var methodNode = nodes.FirstOrDefault(n => n.IsKind(SyntaxKind.MethodDeclaration) 
                                                    || n.IsKind(SyntaxKind.ConstructorDeclaration)
                                                    || n.IsKind(SyntaxKind.GetAccessorDeclaration)
                                                    || n.IsKind(SyntaxKind.SetAccessorDeclaration)
                                                    || n.IsKind(SyntaxKind.IndexerDeclaration));
            if (methodNode != null)
                return model.GetDeclaredSymbol(methodNode);

            var classNode = nodes.FirstOrDefault(n => n.IsKind(SyntaxKind.ClassDeclaration)
                                                   || n.IsKind(SyntaxKind.StructDeclaration));

            if (classNode != null)
                return model.GetDeclaredSymbol(classNode);

            return null;
        }
    }
}
