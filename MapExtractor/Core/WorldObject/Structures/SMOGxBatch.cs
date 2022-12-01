// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.WorldObject.Structures
{
    public class SMOGxBatch
    {
        public ushort VertStart;
        public ushort VertCount;
        public ushort BatchStart;
        public ushort BatchCount;

        public SMOGxBatch(BinaryReader br)
        {
            VertStart = br.ReadUInt16();
            VertCount = br.ReadUInt16();
            BatchStart = br.ReadUInt16();
            BatchCount = br.ReadUInt16();
        }
    }
}
