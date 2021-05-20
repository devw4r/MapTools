// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core
{
    public class C4Vector
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public C4Vector(BinaryReader reader)
        {
            z = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            w = reader.ReadSingle();
        }
    }
}
