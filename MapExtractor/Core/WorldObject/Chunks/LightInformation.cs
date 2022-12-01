// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Chunks
{
    /// <summary>
    /// MOLT
    /// </summary>
    public class LightInformation
    {
        public LightType Type;
        public byte UseAtten;
        public byte[] Unknown_0x2;
        public CImVector Color;
        public C3Vector Position;
        public float Intensity;
        public float AttenStart;
        public float AttenEnd;

        public LightInformation(BinaryReader br)
        {
            Type = (LightType)br.ReadByte();
            UseAtten = br.ReadByte();
            Unknown_0x2 = br.ReadBytes(2);
            Color = new CImVector(br);
            Position = new C3Vector(br);
            Intensity = br.ReadSingle();
            AttenStart = br.ReadSingle();
            AttenEnd = br.ReadSingle();
        }
    }
}
