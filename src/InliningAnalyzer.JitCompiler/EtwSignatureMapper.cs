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
                return type.GetConstructors(SEARCH_BINDINGS).Where(m => !m.IsStatic).ToArray();

            if (methodName == "cctor" || methodName == ".cctor")
                return type.GetConstructors(SEARCH_BINDINGS).Where(m => m.IsStatic).ToArray();

            return type.GetMethods(SEARCH_BINDINGS).Where(m => m.Name == methodName).ToArray();
        }

        public static MethodBase SelectOverload(MethodBase[] candidates, string signature)
        {
            if (candidates.Length == 1)
                return candidates[0];

            string[] signatureParameters = EtwSignatureParser.GetParameters(signature);
            
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
            {
                if (type.FullName.Contains("+"))
                    return type.FullName.Substring(type.FullName.IndexOf("+") + 1);

                return type.FullName;
            }

            var genericParameters = type.GetGenericArguments();

            return type.Namespace + "." + type.Name + "[" + string.Join(",", genericParameters.Select(p => GetTypename(p))) + "]";
        }
    }
}
