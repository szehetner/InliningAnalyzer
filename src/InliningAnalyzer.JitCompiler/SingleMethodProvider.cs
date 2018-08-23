using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class SingleMethodProvider : IMethodProvider
    {
        private readonly Assembly _assembly;
        private readonly string _methodSpecifier;

        public SingleMethodProvider(Assembly assembly, string methodSpecifier)
        {
            _assembly = assembly;
            _methodSpecifier = methodSpecifier;
        }

        public IEnumerable<MethodBase> GetMethods()
        {
            var match = Regex.Match(_methodSpecifier, @"(.+)\|(.+?)\((.*?)\)");
            if (!match.Success)
                throw new ArgumentOutOfRangeException("Invalid methodSpecifier");

            string typeName = match.Groups[1].Value;
            string methodName = match.Groups[2].Value;
            string[] parameters = match.Groups[3].Value.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            
            var type = _assembly.GetType(typeName);
            var candidates = EtwSignatureMapper.GetMethodCandidates(type, methodName);

            if (candidates.Length == 1)
            {
                yield return candidates[0];
            }
            else
            {
                yield return SelectOverload(candidates, parameters);
            }
        }

        private MethodBase SelectOverload(MethodBase[] candidates, string[] parameters)
        {
            foreach (var candidate in candidates)
            {
                var candidateParameters = candidate.GetParameters();
                if (ParameterTypesMatch(candidateParameters, parameters))
                    return candidate;
            }
            throw new InvalidOperationException("No matching overload found for Method " + _methodSpecifier);
        }

        private bool ParameterTypesMatch(ParameterInfo[] candidateParameters, string[] parameters)
        {
            if (candidateParameters.Length != parameters.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (GetTypeName(candidateParameters[i].ParameterType) != parameters[i])
                    return false;
            }
            return true;
        }

        private string GetTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.FullName;

            return type.Namespace + "." + type.Name + "[" + string.Join(";", type.GenericTypeArguments.Select(GetTypeName)) + "]";
        }
    }
}
