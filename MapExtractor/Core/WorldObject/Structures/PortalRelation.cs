// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.WorldObject.Structures
{
    public class PortalRelation
    {
        public ushort PortalIndex;
        public ushort GroupIndex;
        public short Side;

        public PortalRelation(BinaryReader br)
        {
            PortalIndex = br.ReadUInt16();
            GroupIndex = br.ReadUInt16();
            Side = br.ReadInt16();
            br.ReadUInt16(); // Padding
        }
    }
}
