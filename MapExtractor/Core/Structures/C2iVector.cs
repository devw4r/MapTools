// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.Structures
{
    public class C2iVector
    {
        public int x;
        public int y;

        public C2iVector(BinaryReader br)
        {
            x = br.ReadInt32();
            y = br.ReadInt32();
        }
    }
}
