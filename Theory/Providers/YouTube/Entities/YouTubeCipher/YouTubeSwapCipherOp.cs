using System;
using System.Collections.Generic;
using System.Text;

namespace Theory.Providers.YouTube.Entities.YouTubeCipher
{
    internal readonly struct YouTubeSwapCipherOp : IYouTubeCipherOp
    {
        private readonly int _index;

        public readonly string Decipher(string input)
        {
            var stringBuilder = new StringBuilder(input)
            {
                [0] = input[_index],
                [_index] = input[0]
            };

            return stringBuilder.ToString();
        }

        internal YouTubeSwapCipherOp(int index)
        {
            _index = index;
        }
    }
}