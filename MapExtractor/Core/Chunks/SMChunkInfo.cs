// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Log;
using System.Collections.Generic;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Core.Chunks
{
    public class SMChunkInfo
    {
        public uint offset;
        public uint size;
        public uint flags;
        public byte[] pad;

        public SMChunkInfo(BinaryReader reader)
        {
            offset = reader.ReadUInt32();
            size = reader.ReadUInt32();
            flags = reader.ReadUInt32();
            pad = reader.ReadBytes(4);
        }
    }
}
