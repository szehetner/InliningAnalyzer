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
    public class AssemblyCallGraph
    {
        public string AssemblyName { get; set; }

        public Dictionary<string, JitType> Types { get; set; }

        public List<InliningEvent> EventDetails { get; set; }

        public AssemblyCallGraph()
        {
            Types = new Dictionary<string, JitType>();
        }

        public AssemblyCallGraph(Assembly assembly)
        {
            AssemblyName = assembly.FullName;
            Types = new Dictionary<string, JitType>();
        }

        public JitType GetJitType(string fullName)
        {
            Types.TryGetValue(fullName, out JitType type);
            return type;
        }

        private JitType GetOrAddJitType(string fullName)
        {
            if (Types.TryGetValue(fullName, out JitType type))
                return type;

            type = new JitType(fullName);
            Types.Add(fullName, type);
            return type;
        }

        public Method GetOrAddMethod(string typeName, string methodName, string methodSignature)
        {
            var type = GetOrAddJitType(typeName);
            return type.GetOrAddMethod(methodName, methodSignature);
        }

        public void SerializeToFile(string fileName)
        {
            using (FileStream stream = File.OpenWrite(fileName))
            {
                Serializer.Serialize(stream, this);
            }
        }

        public static AssemblyCallGraph Deserialize(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
            {
                return Serializer.Deserialize<AssemblyCallGraph>(stream);
            }
        }
    }
}
