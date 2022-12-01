// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Structures
{
    public class PortalInformation
    {
        public ushort StartVertex;
        public ushort Count;
        public CAaSphere Plane;

        public PortalInformation(BinaryReader br)
        {
            StartVertex = br.ReadUInt16();
            Count = br.ReadUInt16();
            Plane = new CAaSphere(br);
        }
    }
}
