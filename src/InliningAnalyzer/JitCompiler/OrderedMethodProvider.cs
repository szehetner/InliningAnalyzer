using InliningAnalyzer.Graph.Dependencies;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class OrderedMethodProvider : IMethodProvider
    {
        private readonly Assembly _assembly;
        private readonly MethodCompilationList _methodList;

        public OrderedMethodProvider(Assembly assembly, string methodListFile)
        {
            _assembly = assembly;
            _methodList = DeserializeMethodList(methodListFile);
        }

        private static MethodCompilationList DeserializeMethodList(string filename)
        {
            using (var fileStream = File.OpenRead(filename))
            {
                return Serializer.Deserialize<MethodCompilationList>(fileStream);
            }
        }

        public IEnumerable<MethodBase> GetMethods()
        {
            Dictionary<string, Type> types = _assembly.GetTypes().ToDictionary(t => t.FullName);

            foreach (var methodItem in _methodList.Methods)
            {
                if (!types.TryGetValue(methodItem.FullTypeName, out Type type))
                    continue;

                var candidates = EtwSignatureMapper.GetMethodCandidates(type, methodItem.MethodName);
                if (candidates.Length == 0)
                {
                    Console.WriteLine($"Method {type.FullName}.{methodItem.MethodName} could not be found.");
                    continue;
                }

                var method = SelectOverload(candidates, methodItem.Signature);
                if (method == null)
                {
                    Console.WriteLine($"Method {type.FullName}.{methodItem.MethodName}({methodItem.Signature}) could not be found.");
                    continue;
                }
                yield return method;
            }
        }

        private static MethodBase SelectOverload(MethodBase[] methods, string signature)
        {
            return EtwSignatureMapper.SelectOverload(methods, signature);
        }
    }
}
