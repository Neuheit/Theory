using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Xml.Linq;

namespace Theory
{
    public static class Extensions
    {
        public static string WithPath(this string str, string path)
            => $"{str}/{path}";

        public static string WithParameter(this string str, string key, string value)
            => str.Contains("?")
                ? str + $"&{key}={value}"
                : str + $"?{key}={value}";

        public static ReadOnlySpan<byte> ToBytes(this ReadOnlySpan<char> str)
            => MemoryMarshal.Cast<char, Byte>(str);

        public static bool Equals(ReadOnlySpan<char> str, ReadOnlySpan<byte> bytes)
            => ToBytes(str)
                .SequenceEqual(bytes);

        public static string SubstringAfter(this string str, string subStr)
        {
            var index = str.IndexOf(subStr, StringComparison.Ordinal);
            return index < 0 ? string.Empty : str.Substring(index + subStr.Length, str.Length - index - subStr.Length);
        }

        public static string SubstringUntil(this string str, string subStr)
        {
            var index = str.IndexOf(subStr, StringComparison.Ordinal);
            return index < 0 ? str : str.Substring(0, index);
        }

        public static string GetToken(this JsonElement element, string token)
        {
            ReadOnlySpan<char> tokenSpan = token.AsSpan();
            JsonElement finalElement = element;
            int indexOf;

            do
            {
                indexOf = tokenSpan.IndexOf('.');
                finalElement = finalElement
                    .GetProperty(indexOf < 0
                        ? tokenSpan
                        : tokenSpan.Slice(0, indexOf));
                tokenSpan = tokenSpan.Slice(indexOf + 1);
            } while (indexOf != -1);

            return finalElement.GetString();
        }

        public static bool TryGetToken(this JsonElement element, string token, out string result)
        {
            try
            {
                result = element.GetToken(token);
                return !string.IsNullOrWhiteSpace(result);
            }
            catch
            {
                result = "";
                return false;
            }
        }

        public static XElement StripNamespaces(this XElement xml)
        {
            //credits: http://stackoverflow.com/a/1147012

            var xmlResult = new XElement(xml);

            foreach (var element in xmlResult.DescendantsAndSelf())
            {
                element.Name = XNamespace.None.GetName(element.Name.LocalName);

                var elementAttributes = element.Attributes()
                    .Where(
                        a => !a.IsNamespaceDeclaration &&
                        a.Name.Namespace != XNamespace.Xml &&
                        a.Name.Namespace != XNamespace.Xmlns)
                    .Select(a => new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value));

                element.ReplaceAttributes(elementAttributes);
            }

            return xmlResult;
        }
    }
}