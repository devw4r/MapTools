// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Chunks
{
    /// <summary>
    /// MODD
    /// </summary>
    public class DoodadDefinition
    {
        public int NameIndex;
        public C3Vector Position;
        public C4Vector Rotation;
        public float Scale;
        public Color4 Color;

        /// <summary>
        /// Model local extractor path.
        /// </summary>
        public string FilePath = string.Empty;

        /// <summary>
        /// Some referenced MDX models doesnt exist on client data.
        /// </summary>
        public bool Exists = false;

        public DoodadDefinition(BinaryReader br)
        {
            NameIndex = br.ReadInt32();
            Position = new C3Vector(br);
            Rotation = new C4Vector(br);
            Scale = br.ReadSingle();
            Color = new Color4(br);

            if (br is WMO wmo)
            {
                if (NameIndex != -1 && wmo != null)
                {
                    if (!wmo.DoodadFiles.TryGetValue(NameIndex, out string _filePath))
                        Logger.Error($"Doodad File Path for index: {NameIndex} missing from the Dictionary!");
                    else
                    {
                        FilePath = _filePath.ToLocalModelPath();
                        Exists = !string.IsNullOrEmpty(FilePath) && File.Exists(Paths.Transform(FilePath));
                    }
                }
            }
        }
    }
}
