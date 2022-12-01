// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Collections.Generic;

using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.Chunks
{
    public class MapObjectDefinition
    {
        public string FilePath;

        public int NameID;
        /// <summary>
        /// Unique ID of the WMO in this ADT
        /// </summary>
        public uint UniqueId;
        /// <summary>
        /// Position of the WMO
        /// </summary>
        public C3Vector Position;
        /// <summary>
        /// Rotation of the Z axis
        /// </summary>
        public float OrientationA;
        /// <summary>
        /// Rotation of the Y axis
        /// </summary>
        public float OrientationB;
        /// <summary>
        ///  Rotation of the X axis
        /// </summary>
        public float OrientationC;

        public CAaBox Extents;
        public ushort Flags;
        public ushort DoodadSetId;
        public ushort NameSet;

        public MapObjectDefinition(BinaryReader reader, List<string> wmoFiles)
        {
            NameID = reader.ReadInt32();
            FilePath = wmoFiles[NameID];
            UniqueId = reader.ReadUInt32();
            Position = new C3Vector(reader, true);
            OrientationA = reader.ReadSingle();
            OrientationB = reader.ReadSingle();
            OrientationC = reader.ReadSingle();
            Extents = new CAaBox(new C3Vector(reader, true), new C3Vector(reader, true));
            Flags = reader.ReadUInt16();
            DoodadSetId = reader.ReadUInt16();
            NameSet = reader.ReadUInt16();

            reader.ReadUInt16(); // Padding
        }

        public static Dictionary<uint, MapObjectDefinition> BuildFromChunk(byte[] chunk, List<string> wmoFiles)
        {
            var wmoDefs = new Dictionary<uint, MapObjectDefinition>();
            using (MemoryStream ms = new MemoryStream(chunk))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                while (reader.BaseStream.Position != chunk.Length)
                {
                    var wmoDef = new MapObjectDefinition(reader, wmoFiles);
                    wmoDefs.Add(wmoDef.UniqueId, wmoDef);
                }
            }

            if (Globals.Verbose)
                Logger.Info($"Loaded {wmoDefs.Count} WMO definitions.");

            return wmoDefs;
        }
    }
}
