using System;
using System.Collections.Generic;
using System.Text;

namespace Theory.Providers.YouTube.Entities.YouTubeCipher
{
    internal readonly struct YouTubeSliceCipherOp : IYouTubeCipherOp
    {
        private readonly int _index;

        public readonly string Decipher(string input)
            => input.Substring(_index);

        internal YouTubeSliceCipherOp(int index)
        {
            _index = index;
        }
    }
}