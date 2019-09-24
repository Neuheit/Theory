using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Theory.Providers.BandCamp
{
    public readonly struct BandCampParser
    {
        public static async ValueTask<string> ScrapeJsonAsync(RestClient restClient, string url)
        {
            var rawHtml = await restClient
                .GetStringAsync(url)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(rawHtml))
                return string.Empty;

            const string startStr = "var TralbumData = {",
                         endStr = "};";

            if (rawHtml.IndexOf(startStr, StringComparison.Ordinal) == -1)
                return string.Empty;

            var tempData = rawHtml.Substring(rawHtml.IndexOf(startStr, StringComparison.Ordinal) + startStr.Length - 1);
            tempData = tempData.Substring(0, tempData.IndexOf(endStr, StringComparison.Ordinal) + 1);

            var jsonReg = new Regex(@"([a-zA-Z0-9_]*:\s)(?!\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var commentReg = new Regex(@"\/\*[\s\S]*?\*\/|([^:]|^)\/\/.*",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            tempData = commentReg.Replace(tempData, "");
            var matches = jsonReg.Matches(tempData);
            foreach (Match match in matches)
            {
                var val = $"\"{match.Value.Replace(": ", "")}\":";
                var regex = new Regex(Regex.Escape(match.Value), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                tempData = regex.Replace(tempData, val, 1);
            }

            tempData = tempData.Replace("\" + \"", "");
            return tempData;
        }
    }
}