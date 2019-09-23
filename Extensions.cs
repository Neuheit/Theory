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
    }
}