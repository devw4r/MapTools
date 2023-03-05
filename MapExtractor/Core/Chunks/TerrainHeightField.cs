// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;

namespace AlphaCoreExtractor.Core.Chunks
{
    public class TerrainHeightMap : IDisposable
    {
        public float[,] V9 = new float[16 * 8 + 1, 16 * 8 + 1];
        public float[,] V8 = new float[16 * 8, 16 * 8];

        public void Dispose()
        {
            V9 = null;
            V8 = null;
        }
    }
}
