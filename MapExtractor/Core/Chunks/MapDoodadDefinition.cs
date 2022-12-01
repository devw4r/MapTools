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
    public class MapDoodadDefinition
    {
        /// <summary>
        /// Filename of the MDX, internal.
        /// </summary>
        public string FilePath;

        /// <summary>
        /// Some referenced MDX models does not exist in the client data.
        /// </summary>
        public bool Exists;

        /// <summary>
        /// Unique ID of the MDX in this ADT
        /// </summary>
        public uint UniqueId;

        /// <summary>
        /// Position of the MDX.
        /// </summary>
        public C3Vector Position;

        /// <summary>
        /// Rotation around the Z axis
        /// </summary>
        public float OrientationA;

        /// <summary>
        /// Rotation around the Y axis
        /// </summary>
        public float OrientationB;

        /// <summary>
        /// Rotation around the X axis
        /// </summary>
        public float OrientationC;

        /// <summary>
        ///  Scale factor of the MDX
        /// </summary>
        public float Scale;

        /// <summary>
        /// Flags for the MDX
        /// </summary>
        public ushort Flags;

        public MapDoodadDefinition(BinaryReader reader, List<string> mdxModelFiles)
        {
            var nameIndex = reader.ReadInt32();
            FilePath = mdxModelFiles[nameIndex].ToLocalModelPath();
            Exists = !string.IsNullOrEmpty(FilePath) && File.Exists(Paths.Transform(FilePath));
            UniqueId = reader.ReadUInt32();
            Position = new C3Vector(reader, true);
            OrientationA = reader.ReadSingle();
            OrientationB = reader.ReadSingle();
            OrientationC = reader.ReadSingle();
            Scale = reader.ReadUInt16() / 1024f;
            Flags = reader.ReadUInt16();
        }

        public static Dictionary<uint, MapDoodadDefinition> BuildFromChunck(byte[] chunk, List<string> mdxModelFiles)
        {
            Dictionary<uint, MapDoodadDefinition> mdxDefs = new Dictionary<uint, MapDoodadDefinition>();
            using (MemoryStream ms = new MemoryStream(chunk))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                while (reader.BaseStream.Position != chunk.Length)
                {
                    var doodadDef = new MapDoodadDefinition(reader, mdxModelFiles);
                    mdxDefs.Add(doodadDef.UniqueId, doodadDef);
                }
            }

            if (Globals.Verbose)
                Logger.Info($"Loaded {mdxDefs.Count} MDX definitions.");

            return mdxDefs;
        }
    }
}
