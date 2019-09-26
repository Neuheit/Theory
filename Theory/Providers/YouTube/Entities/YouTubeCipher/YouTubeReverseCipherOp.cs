using System;
using System.Collections.Generic;
using System.Text;

namespace Theory.Providers.YouTube.Entities.YouTubeCipher
{
    internal readonly struct YouTubeReverseCipherOp : IYouTubeCipherOp
    {
        public readonly string Decipher(string input)
        {
            var stringBuilder = new StringBuilder(input.Length);

            for (var i = input.Length - 1; i >= 0; i--)
                stringBuilder.Append(input[i]);

            return stringBuilder.ToString();
        }

        internal YouTubeReverseCipherOp(int _)
        {
        }
    }
}