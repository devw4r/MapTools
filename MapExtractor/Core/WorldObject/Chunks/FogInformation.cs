// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Core.Structures;
using AlphaCoreExtractor.Core.WorldObject.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Chunks
{
    /// <summary>
    /// MFOG
    /// </summary>
    public class FogInformation
    {
        public MFOG_Flags Flags;
        public C3Vector Position;
        public float SmallerRadius;
        public float LargerRadius;
        public Fog[] Fogs = new Fog[2];

        public FogInformation(BinaryReader br)
        {
            Flags = (MFOG_Flags)br.ReadUInt32();
            Position = new C3Vector(br);
            SmallerRadius = br.ReadSingle();
            LargerRadius = br.ReadSingle();

            for (int i = 0; i < 2; i++)
                Fogs[i] = new Fog(br);
        }
    }
}
