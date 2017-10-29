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
    public class MethodNameResolver
    {
        public static string GetMethodName(SemanticModel model, SyntaxTree tree, int line, int column)
        {
            var symbol = GetSymbolByLocation(model, tree, line, column);
            if (symbol == null)
                return null;
            
            var methodSymbol = GetSymbolByLocation(model, tree, line, column) as IMethodSymbol;
            if (methodSymbol == null)
                return null;
            
            StringBuilder builder = new StringBuilder();
            builder.Append(symbol.ContainingType.GetFullTypename(";", true));
            builder.Append(".");
            if (symbol.Name == ".ctor")
                builder.Append("ctor");
            else
                builder.Append(symbol.Name);

            builder.Append("(");
            builder.Append(string.Join(",", methodSymbol.Parameters.Select(p => p.Type.GetFullTypename(";", true))));
            builder.Append(")");

            return builder.ToString();
        }

        public static ISymbol GetSymbolByLocation(SemanticModel model, SyntaxTree tree, int line, int column)
        {
            var textSpan = new TextSpan(column, 0);
            var lineSpan = tree.GetText().Lines[line - 1];

            var nodes = tree.GetRoot().DescendantNodes().Where(n => n.Span.IntersectsWith(lineSpan.Span));
            var methodNode = nodes.FirstOrDefault(n => n.IsKind(SyntaxKind.MethodDeclaration) 
                                                    || n.IsKind(SyntaxKind.ConstructorDeclaration)
                                                    || n.IsKind(SyntaxKind.GetAccessorDeclaration)
                                                    || n.IsKind(SyntaxKind.SetAccessorDeclaration));
            if (methodNode == null)
                return null;

            return model.GetDeclaredSymbol(methodNode);
        }
    }
}
