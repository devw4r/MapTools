// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.Structures
{
    public class Color4
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;

        public Color4(BinaryReader reader)
        {
            B = reader.ReadByte();
            G = reader.ReadByte();
            R = reader.ReadByte();
            A = reader.ReadByte();
        }
    }
}
