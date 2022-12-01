// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.DBC.Reader;

namespace AlphaCoreExtractor.Core.WorldObject.Structures
{
    public class DoodadSet
    {
        public string SetName;
        public uint FirstInstanceIndex;
        public uint InstanceCount;

        public DoodadSet(BinaryReader br)
        {
            SetName = br.ReadFixedString(20);
            FirstInstanceIndex = br.ReadUInt32();
            InstanceCount = br.ReadUInt32();
            br.ReadBytes(4); // Padding
        }
    }
}
