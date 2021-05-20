// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core
{
    public class CAaBox
    {
        public C3Vector b;
        public C3Vector t;

        public CAaBox(BinaryReader reader)
        {
            b = new C3Vector(reader);
            t = new C3Vector(reader);
        }
    }
}
