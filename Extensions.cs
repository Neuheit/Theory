using System;
using System.Runtime.InteropServices;

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
    }
}