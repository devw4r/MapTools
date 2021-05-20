// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Text;
using System.Collections.Generic;

namespace AlphaCoreExtractor.Core
{
    public class SMAreaInfo
    {
        public uint offset;     // absolute offset MHDR
        public uint size;       // offset relative to MHDR start (so value can also be read as : all ADT chunks total size from MHDR to first MCNK) MCNK
        public uint flags;      // FLAG_LOADED = 0x1 is the only flag, set at runtime
        public byte[] pad = new byte[4];

        public SMAreaInfo(BinaryReader reader)
        {
            offset = reader.ReadUInt32();
            size = reader.ReadUInt32();
            flags = reader.ReadUInt32();
            pad = reader.ReadBytes(4);
        }

        public static SMAreaInfo[] BuildFromChunk(byte[] chunk)
        {
            var tiles = new List<SMAreaInfo>();
            using (MemoryStream ms = new MemoryStream(chunk))
            using (BinaryReader reader = new BinaryReader(ms))
                while (reader.BaseStream.Position != chunk.Length)
                    tiles.Add(new SMAreaInfo(reader));
            return tiles.ToArray();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SMAreaInfo");
            sb.AppendLine($"Offset: {offset}");
            sb.AppendLine($"flags: {offset}");
            sb.AppendLine($"pad: {pad.ToString()}");
            return sb.ToString();
        }
    }
}
