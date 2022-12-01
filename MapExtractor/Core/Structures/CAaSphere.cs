// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.Structures
{
    public class CAaSphere
    {
        public C3Vector c;
        public float r;

        public CAaSphere(BinaryReader reader)
        {
            c = new C3Vector(reader);
            r = reader.ReadSingle();
        }
    }
}
