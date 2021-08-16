// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Log;
using System.Collections.Generic;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Core
{
    public class SMMapObjDef
    {
        public uint nameID;
        public uint uniqueId;
        public C3Vector pos;
        public C3Vector rot;
        public CAaBox extents;
        public ushort flags;
        public ushort doodadSet;
        public ushort nameSet;
        public ushort pad;

        public SMMapObjDef(BinaryReader reader)
        {
            nameID = reader.ReadUInt32();
            uniqueId = reader.ReadUInt32();
            pos = new C3Vector(reader);
            rot = new C3Vector(reader);
            extents = new CAaBox(reader);
            flags = reader.ReadUInt16();
            doodadSet = reader.ReadUInt16();
            nameSet = reader.ReadUInt16();
            pad = reader.ReadUInt16();
        }

        public static Dictionary<uint, SMMapObjDef> BuildFromChunk(byte[] chunk)
        {
            var wmoDefs = new Dictionary<uint, SMMapObjDef>();
            using (MemoryStream ms = new MemoryStream(chunk))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                while (reader.BaseStream.Position != chunk.Length)
                {
                    var wmoDef = new SMMapObjDef(reader);
                    wmoDefs.Add(wmoDef.uniqueId, wmoDef);
                }
            }

            if (Globals.Verbose)
                Logger.Info($"Loaded {wmoDefs.Count} WMO definitions.");

            return wmoDefs;
        }
    }
}
