// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Helpers.Enums;

namespace AlphaCoreExtractor.Core.WorldObject.Chunks.WMOGroups
{
    public class MOPY
    {
        public MOPY_Flags Flags;
        public byte LightmapTex;
        public byte MaterialID;

        public MOPY(BinaryReader br)
        {
            Flags = (MOPY_Flags)br.ReadByte();
            LightmapTex = br.ReadByte();
            MaterialID = br.ReadByte();
            br.ReadByte(); // Padding
        }
    }
}
