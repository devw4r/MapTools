// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.Structures
{
    // 1<<0 - ocean
    // 1<<1 - lava/slime
    // 1<<2 - water
    // 1<<6 - all water
    // 1<<7 - dark water
    // == 0x0F - not show liquid
    public class SLTiles
    {
        public byte[,] Tiles = new byte[8, 8];

        public SLTiles(BinaryReader reader)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    Tiles[i, j] = reader.ReadByte();
        }
    }
}
