// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

namespace AlphaCoreExtractor.Helpers
{
    public static class Constants
    {     
        public static int TileBlockSize = 64;
        public static float TileSize = 16;
        public static int Cell_Size = 8;
        public static float GridSize => TileSize * Cell_Size;
        public static float TileSizeYrds = 533.33333F;
        public static float ChunkSize = TileSizeYrds / TileSize;
        public static float UnitSize = ChunkSize / 8.0f;
    }
}
