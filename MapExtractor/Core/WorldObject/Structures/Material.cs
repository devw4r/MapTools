// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using AlphaCoreExtractor.Core.Readers;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Structures
{
    public class Material
    {
        public uint Version;
        public Material_Flags Flags;
        public BlendMode BlendMode;
        public uint TextureNameStart; // Offset into MOTX?
        public CImVector SidnColor; // Emissive color
        public CImVector FrameSidnColor; // Sidn emissive color; set at runtime; gets sidn-manipulated emissive color
        public uint TextureNameEnd; // Offset into MOTX
        public CImVector DiffColor;
        public uint GroundType; // TerrainTypeRec::m_ID
        public uint UnknownTexture;
        public CImVector UnknownColor;
        public Material_Flags UnknownFlags;

        /// <summary>
        /// MOMT Chunk
        /// </summary>
        /// <param name="br"></param>
        public Material(BinaryReaderProgress br)
        {
            Version = br.ReadUInt32();
            Flags = (Material_Flags)br.ReadUInt32();
            BlendMode = (BlendMode)br.ReadUInt32();
            TextureNameStart = br.ReadUInt32();
            SidnColor = new CImVector(br);
            FrameSidnColor = new CImVector(br);
            TextureNameEnd = br.ReadUInt32();
            DiffColor = new CImVector(br);
            GroundType = br.ReadUInt32();
            br.ReadBytes(8); // inMemPad
        }
    }

    [Flags]
    public enum Material_Flags : uint
    {
        None = 0,
        Unlit = 0x1,
        Unfogged = 0x2,
        Unculled = 0x4,
        ExteriorLit = 0x8,
        SelfIlluminatedDayNight = 0x10,
        Window = 0x20,
        ClampSAddress = 0x40,
        ClampTAddress = 0x80,
        Unknown_0x100 = 0x100
    }

    public enum BlendMode : uint
    {
        Opaque = 0x0,
        AlphaKey = 0x1,
        Alpha = 0x2,
        Add = 0x3,
        Mod = 0x4,
        Mod2x = 0x5,
        ModAdd = 0x6,
        InvSrcAlphaAdd = 0x7,
        InvSrcAlphaOpaque = 0x8,
        SrcAlphaOpaque = 0x9,
        NoAlphaAdd = 0xA,
        ConstantAlpha = 0xB,
        Screen = 0xC,
        BlendAdd = 0xD,
    }
}
