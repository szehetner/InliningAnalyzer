using System;
using System.Collections.Generic;
using System.Reflection;

namespace InliningAnalyzer
{
    internal class TypeMethodProvider : UnorderedMethodProvider
    {
        private readonly string _typeName;

        public TypeMethodProvider(Assembly assembly, string typeName) 
            : base(assembly)
        {
            _typeName = typeName;
        }

        protected override IEnumerable<Type> GetTypes()
        {
            yield return _assembly.GetType(_typeName);
        }
    }
}