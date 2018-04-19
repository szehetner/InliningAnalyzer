using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsExtension.Model
{
    public static class SignatureResolver
    {
        public static string BuildSignature(IMethodSymbol method)
        {
            return BuildSignature(method, method.ReturnType, method.Parameters);
        }

        public static string BuildSignature(IPropertySymbol property)
        {
            return BuildSignature(property, property.Type, property.Parameters);
        }

        private static string BuildSignature(ISymbol symbol, ITypeSymbol returnType, ImmutableArray<IParameterSymbol> parameters)
        {
            StringBuilder builder = new StringBuilder();
            if (!symbol.IsStatic)
                builder.Append("instance ");

            AppendTypeName(builder, returnType);

            builder.Append("  (");
            for (int i = 0; i < parameters.Length; i++)
            {
                AppendTypeName(builder, parameters[i].Type);

                if (parameters[i].RefKind == RefKind.Out)
                    builder.Append("&");

                if (i != parameters.Length - 1)
                    builder.Append(",");
            }
            builder.Append(")");

            return builder.ToString();
        }

        private static void AppendTypeName(StringBuilder builder, ITypeSymbol type)
        {
            bool isPointer = false;
            if (type.TypeKind == TypeKind.Pointer)
            {
                isPointer = true;
                type = ((IPointerTypeSymbol)type).PointedAtType;
            }

            var specialType = GetSpecialTypeName(type);
            if (specialType != null)
            {
                builder.Append(specialType);
                return;
            }

            if (type.IsValueType)
                builder.Append("value ");

            if (type.TypeKind == TypeKind.Array)
            {
                var arrayType = (IArrayTypeSymbol)type;
                var elementType = (arrayType).ElementType;
                AppendTypeName(builder, elementType);
                for (int i = 0; i < arrayType.Rank; i++)
                    builder.Append("[]");
            }
            else
            {
                builder.Append("class ");
                builder.Append(type.GetFullNamespace());
                builder.Append(".");
                builder.Append(type.Name);
            }
            if (isPointer)
                builder.Append("*");

            if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                builder.Append("`");
                builder.Append(namedType.TypeArguments.Length);
                builder.Append("<");
                for (int i = 0; i < namedType.TypeArguments.Length; i++)
                {
                    var typeArgument = namedType.TypeArguments[i];
                    AppendTypeName(builder, typeArgument);
                    if (i < namedType.TypeArguments.Length - 1)
                        builder.Append(",");
                }
                builder.Append(">");
            }
        }
        
        private static string GetSpecialTypeName(ITypeSymbol type)
        {
            switch(type.SpecialType)
            {
                case SpecialType.System_Void:
                    return "void";
                case SpecialType.System_SByte:
                    return "int8";
                case SpecialType.System_Int16:
                    return "int16";
                case SpecialType.System_Int32:
                    return "int32";
                case SpecialType.System_Int64:
                    return "int64";
                case SpecialType.System_Boolean:
                    return "bool";
                case SpecialType.System_Byte:
                    return "unsigned int8";
                case SpecialType.System_Char:
                    return "wchar";
                case SpecialType.System_UInt16:
                    return "unsigned int16";
                case SpecialType.System_UInt32:
                    return "unsigned int32";
                case SpecialType.System_UInt64:
                    return "unsigned int64";
                case SpecialType.System_Single:
                    return "float32";
                case SpecialType.System_Double:
                    return "float64";
                default:
                    return null;
            }
        }
    }
}
