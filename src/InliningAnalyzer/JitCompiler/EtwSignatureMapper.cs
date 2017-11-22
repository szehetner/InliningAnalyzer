using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class EtwSignatureMapper
    {
        public const BindingFlags SEARCH_BINDINGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static MethodBase[] GetMethodCandidates(Type type, string methodName)
        {
            if (methodName == "ctor" || methodName == ".ctor")
                return type.GetConstructors(SEARCH_BINDINGS);

            return type.GetMethods(SEARCH_BINDINGS).Where(m => m.Name == methodName).ToArray();
        }

        public static MethodBase SelectOverload(MethodBase[] candidates, string signature)
        {
            if (candidates.Length == 1)
                return candidates[0];

            string[] signatureParameters = GetParameters(signature);
            
            var overloads = candidates.Select(m => (Method: m, Parameters: m.GetParameters())).Where(t => t.Parameters.Length == signatureParameters.Length).ToArray();
            if (overloads.Length == 0)
                return null;

            if (overloads.Length == 1)
                return overloads[0].Method;

            foreach (var overload in overloads)
            {
                if (ParametersMatch(overload.Parameters, signatureParameters))
                    return overload.Method;
            }

            return null;
        }

        private static bool ParametersMatch(ParameterInfo[] parameters, string[] signatureParameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (GetTypename(parameters[i].ParameterType) != signatureParameters[i])
                    return false;
            }
            return true;
        }

        private static string GetTypename(Type type)
        {
            if (!type.IsGenericType)
                return type.FullName;

            var genericParameters = type.GetGenericArguments();

            string typename = type.Namespace + "." + type.Name + "[";
            foreach (var parameter in genericParameters)
            {
                typename += GetTypename(parameter);
            }
            typename += "]";
            return typename;
        }

        private static string[] GetParameters(string rawSignature)
        {
            var start = rawSignature.IndexOf("(");
            var rawParameterPart = rawSignature.Substring(start + 1, rawSignature.Length - start - 2);
            if (rawParameterPart == "")
                return new string[0];

            return ParseParameterList(rawParameterPart);
        }

        private static string[] ParseParameterList(string rawParameterPart)
        {
            var rawParameters = rawParameterPart.Split(',');
            string[] parameters = new string[rawParameters.Length];

            for (int i = 0; i < rawParameters.Length; i++)
                parameters[i] = GetParameter(rawParameters[i]);

            return parameters;
        }

        private static string GetParameter(string rawParameter)
        {
            var arrayPartStart = rawParameter.IndexOf("[");
            string arrayPart = null;
            if (arrayPartStart != -1)
            {
                arrayPart = rawParameter.Substring(arrayPartStart);
                rawParameter = rawParameter.Substring(0, arrayPartStart);
            }
            var typename = MapSpecialTypename(rawParameter);
            if (typename != null)
                return typename + arrayPart;

            rawParameter = rawParameter.Replace("class ", "").Replace("<", "[").Replace(">", "]");
            var items = rawParameter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return items.LastOrDefault() + arrayPart;
        }

        private static string MapSpecialTypename(string rawType)
        {
            switch(rawType)
            {
                case "bool":
                    return "System.Boolean";
                case "int8":
                    return "System.SByte";
                case "int16":
                    return "System.Int16";
                case "int32":
                    return "System.Int32";
                case "int64":
                    return "System.Int64";
                case "unsigned int8":
                    return "System.Byte";
                case "wchar":
                    return "System.SByte";
                case "unsigned int16":
                    return "System.UInt16";
                case "unsigned int32":
                    return "System.UInt32";
                case "unsigned int64":
                    return "System.UInt64";
                case "float32":
                    return "System.Single";
                case "float64":
                    return "System.Double";
                case "int":
                    return "System.IntPtr";
                case "unsigned int":
                    return "System.UIntPtr";
            }
            
            return null;
        }
    }
}
