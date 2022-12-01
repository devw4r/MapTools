// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.Structures
{
    public class CImVector
    {
        public byte b;
        public byte g;
        public byte r;
        public byte a;

        public CImVector(BinaryReader reader)
        {
            b = reader.ReadByte();
            g = reader.ReadByte();
            r = reader.ReadByte();
            a = reader.ReadByte();
        }

        public override string ToString()
        {
            return $"A{a},R{r},G{g},B{b}";
        }
    }
}
