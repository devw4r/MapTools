// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Structures
{
    public class Fog
    {
        public float End;
        public float StartScalar;
        public CImVector Color;

        public Fog(BinaryReader br)
        {
            End = br.ReadSingle();
            StartScalar = br.ReadSingle();
            Color = new CImVector(br);
        }
    }
}
