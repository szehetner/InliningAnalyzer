using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsExtension
{
    public static class Extensions
    {
        public static ITagSpan<IClassificationTag> ToTagSpan(this TextSpan span, ITextSnapshot snapshot, IClassificationType classificationType)
        {
            return new TagSpan<IClassificationTag>(
              new SnapshotSpan(snapshot, span.Start, span.Length),
              new ClassificationTag(classificationType)
              );
        }

        public static String GetText(this ITextSnapshot snapshot, TextSpan span)
        {
            return snapshot.GetText(new Span(span.Start, span.Length));
        }
        
        public static string GetFullNamespace(this ITypeSymbol type)
        {
            if (type == null)
                return null;

            string result = "";

            var currentSpace = type.ContainingNamespace;
            while (currentSpace != null && !string.IsNullOrEmpty(currentSpace.Name))
            {
                result = currentSpace.Name + (result != "" ? "." : "") + result;
                currentSpace = currentSpace.ContainingNamespace;
            }
            return result;
        }
        
        public static string GetFullTypename(this ITypeSymbol type)
        {
            string typename = type.Name;
            var namedType = type as INamedTypeSymbol;
            if (namedType != null && namedType.IsGenericType)
            {
                typename += "`" + namedType.TypeArguments.Length + "[";

                typename += string.Join(",", namedType.TypeArguments.Select(GetGenericArgumentName));

                typename += "]";
            }

            if (type.ContainingType != null)
                return GetFullTypename(type.ContainingType) + "+" + typename;

            return type.GetFullNamespace() + "." + typename;
        }

        private static string GetGenericArgumentName(ITypeSymbol typeArgument)
        {
            if (typeArgument.IsReferenceType)
                return "System.__Canon";

            return GetFullTypename(typeArgument);
        }
    }
}
