﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class UnorderedMethodProvider : IMethodProvider
    {
        private readonly Assembly _assembly;

        public UnorderedMethodProvider(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<MethodBase> GetMethods()
        {
            Type[] types = _assembly.GetTypes();
            foreach (Type type in types)
            {
                BindingFlags filterFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

                MethodBase[] methods = type.GetMethods(filterFlags);
                MethodBase[] constructors = type.GetConstructors(filterFlags);

                var allMethods = methods.Concat(constructors);

                foreach (MethodBase method in allMethods)
                {
                    if (method == null ||
                        method.IsAbstract ||
                        method.ContainsGenericParameters)
                        continue;

                    yield return method;
                }
            }
        }
    }
}