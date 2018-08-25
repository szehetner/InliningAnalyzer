using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class EtwSignatureParser
    {
        private static string[] _empty = new string[0];

        public static string[] GetParameters(string rawSignature)
        {
            var start = rawSignature.IndexOf("(");
            if (rawSignature.Length - start - 2 == 0)
                return _empty;

            return ParseParameterList(rawSignature, start + 1, rawSignature.Length);
        }

        private static string[] ParseParameterList(string input, int startIndex, int endIndex)
        {
            List<string> parameters = new List<string>();

            int currentIndex = startIndex;
            while (true)
            {
                var parameterToken = GetNextParameter(input, currentIndex, endIndex);
                if (parameterToken.Name == null)
                    break;

                parameters.Add(parameterToken.Name);
                currentIndex = parameterToken.EndIndex + 1;
            }

            return parameters.ToArray();
        }

        private const char PARAM_SEPERATOR = ',';
        private const char END = ')';
        private const char GENERIC_START = '<';
        private const char GENERIC_END = '>';
        private static ParameterToken GetNextParameter(string input, int startIndex, int endIndex)
        {
            if (startIndex >= endIndex)
                return new ParameterToken(null, endIndex);

            int i = startIndex;
            while (i <= endIndex && input[i] != PARAM_SEPERATOR && input[i] != END)
            {
                if (input[i] == GENERIC_START)
                {
                    int genericEndIndex = FindMatchingGenericEnd(input, i, endIndex);
                    string[] parameters = ParseParameterList(input, i + 1, genericEndIndex - 1);

                    string genericParam = ParseTypename(input.Substring(startIndex, i-startIndex)) + "[" + string.Join(",", parameters) + "]";
                    return new ParameterToken(genericParam, genericEndIndex + 1);
                }

                i++;
            }
            if (i == startIndex)
                return new ParameterToken(null, i);

            return new ParameterToken(ParseTypename(input.Substring(startIndex, i - startIndex)), i);
        }

        private static int FindMatchingGenericEnd(string input, int startIndex, int endIndex)
        {
            int stack = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (input[i] == GENERIC_START)
                    stack++;

                if (input[i] == GENERIC_END)
                    stack--;

                if (stack == 0)
                    return i;
            }
            throw new Exception("Invalid Signature (unended generic definition): " + input);
        }

        private static string ParseTypename(string rawParameter)
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
            
            var items = rawParameter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return items.LastOrDefault() + arrayPart;
        }

        private static string MapSpecialTypename(string rawType)
        {
            switch (rawType)
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

    internal struct ParameterToken
    {
        public string Name { get; }
        public int EndIndex { get; }

        public ParameterToken(string name, int endIndex)
        {
            Name = name;
            EndIndex = endIndex;
        }
    }
}
