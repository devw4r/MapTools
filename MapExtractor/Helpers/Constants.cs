// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;

namespace AlphaCoreExtractor.Helpers
{
    public static class Constants
    {     
        public static int TileBlockSize = 64;
        public static int TileSize = 16;
        public static int CellSize = 8;
        public static float GridSize => TileSize * CellSize;
        public static float TileSizeYrds = 533.33333F;
        public static float ChunkSize = TileSizeYrds / TileSize;
        public static float UnitSize = ChunkSize / 8.0f;

        /// <summary>
        /// The Center of a map is it's origin
        /// </summary>
        public static float CenterPoint = (TileBlockSize / 2f) * TileSizeYrds;
        public static float Epsilonf = 1e-3f;
        public static float PI = (float)Math.PI;

        /// <summary>
        /// MDX
        /// </summary>
        public const int SizeTag = 4;
        public const int SizeName = 80;
        public const int SizeFileName = 260;
    }
}
