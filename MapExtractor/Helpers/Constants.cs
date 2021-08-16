// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

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
        public static float CenterPoint = (TileBlockSize / 2f) * TileSize;
    }
}
