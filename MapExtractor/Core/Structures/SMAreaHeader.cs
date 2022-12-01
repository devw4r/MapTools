// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.Structures
{
    public class SMAreaHeader
    {
        public uint offsInfo;    // MCIN
        public uint offsTex;     // MTEX
        public uint sizeTex;
        public uint offsDoo;     // MDDF
        public uint sizeDoo;
        public uint offsMob;     // MODF
        public uint sizeMob;
        public byte[] pad = new byte[36];

        public SMAreaHeader(BinaryReader reader)
        {
            offsInfo = reader.ReadUInt32();
            offsTex = reader.ReadUInt32();
            sizeTex = reader.ReadUInt32();
            offsDoo = reader.ReadUInt32();
            sizeDoo = reader.ReadUInt32();
            offsMob = reader.ReadUInt32();
            sizeMob = reader.ReadUInt32();
            pad = reader.ReadBytes(36);
        }
    }
}
