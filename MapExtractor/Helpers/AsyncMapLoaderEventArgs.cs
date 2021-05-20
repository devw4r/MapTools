// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using AlphaCoreExtractor.Core;

namespace AlphaCoreExtractor.Helpers
{
    public class AsyncMapLoaderEventArgs : EventArgs
    {
        public CMapObj Map;
        public AsyncMapLoaderEventArgs(CMapObj map)
        {
            Map = map;
        }
    }
}
