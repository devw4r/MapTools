// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core
{
    public class SLTiles
    {
        public byte[,] Tiles = new byte[8, 8];

        public SLTiles(BinaryReader reader)
        {
            for (int i = 0; i < Tiles.Length; i++)
                for (int j = 0; j < Tiles.Length; j++)
                    Tiles[i, j] = reader.ReadByte();
        }
    }
}
