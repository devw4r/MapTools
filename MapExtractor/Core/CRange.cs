// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core
{
    public class CRange
    {
        public float l;
        public float h;
        public CRange(BinaryReader reader)
        {
            l = reader.ReadSingle();
            h = reader.ReadSingle();
        }
    }
}
