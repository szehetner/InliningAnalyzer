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
        
        public static string GetFullTypename(this ITypeSymbol type, string genericParameterSeparator = ",", bool explicitReferenceTypes = false)
        {
            var arraySymbol = type as IArrayTypeSymbol;
            if (arraySymbol != null)
            {
                return arraySymbol.ElementType.GetFullTypename(genericParameterSeparator, explicitReferenceTypes) + "[]";
            }

            string typename = type.Name;
            var namedType = type as INamedTypeSymbol;
            if (namedType != null && namedType.IsGenericType)
            {
                typename += "`" + namedType.TypeArguments.Length + "[";

                typename += string.Join(genericParameterSeparator, namedType.TypeArguments.Select(t => GetGenericArgumentName(t, explicitReferenceTypes)));

                typename += "]";
            }

            if (type.ContainingType != null)
                return GetFullTypename(type.ContainingType) + "+" + typename;

            return type.GetFullNamespace() + "." + typename;
        }

        private static string GetGenericArgumentName(ITypeSymbol typeArgument, bool explicitReferenceTypes)
        {
            if (!explicitReferenceTypes && typeArgument.IsReferenceType)
                return "System.__Canon";

            if (typeArgument.IsDefinition)
                return typeArgument.Name;

            return GetFullTypename(typeArgument);
        }
    }
}
