// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;

namespace AlphaCoreExtractor.Core.WorldObject
{
    [Flags]
    public enum MFOG_Flags : uint
    {
        None = 0,
        InteriorExteriorBlend = 1
    }

    public enum EFogs : uint
    {
        Fog,
        UnderWaterFog
    }

    public enum LightType
    {
        OMNI_LGT = 0x0,
        SPOT_LGT = 0x1,
        DIRECT_LGT = 0x2,
        AMBIENT_LGT = 0x3,
    }

    public enum LiquidTypes
    {
        LIQUID_Slow_Water = 5,
        LIQUID_Slow_Ocean = 6,
        LIQUID_Slow_Magma = 7,
        LIQUID_Slow_Slime = 8,
        LIQUID_Fast_Water = 9,
        LIQUID_Fast_Ocean = 10,
        LIQUID_Fast_Magma = 11,
        LIQUID_Fast_Slime = 12,
        LIQUID_WMO_Water = 13,
        LIQUID_WMO_Ocean = 14,
        LIQUID_Green_Lava = 15,
        LIQUID_WMO_Water_Interior = 17,
        LIQUID_WMO_Magma = 19,
        LIQUID_WMO_Slime = 20,

        LIQUID_END_BASIC_LIQUIDS = 20,
        LIQUID_FIRST_NONBASIC_LIQUID_TYPE = 21,

        LIQUID_NAXX_SLIME = 21,
        LIQUID_Coilfang_Raid_Water = 41,
        LIQUID_Hyjal_Past_Water = 61,
        LIQUID_Lake_Wintergrasp_Water = 81,
        LIQUID_Basic_Procedural_Water = 100,
        LIQUID_CoA_Black_Magma = 121,
        LIQUID_Chamber_Magma = 141,
        LIQUID_Orange_Slime = 181,
    };

    public enum LiquidBasicTypes
    {
        LiquidBasicTypes_Water = 0,
        LiquidBasicTypes_Ocean = 1,
        LiquidBasicTypes_Magma = 2,
        LiquidBasicTypes_Slime = 3,

        LiquidBasicTypes_MASK = 3,
    };

    public enum WMOLiquidFlags
    {
      LiquidSurface = 0x1000,
      IsNotWaterButOcean = 0x80000,
    }

    [Flags]
    public enum MOGP_Flags : uint
    {
        None = 0,
        HasBSP = 0x1,
        HasLightmap = 0x2,
        HasVertexColors = 0x4,
        IsExterior = 0x8,
        Unknown_0x10 = 0x10,
        Unknown_0x20 = 0x20,
        IsExteriorLit = 0x40,
        Unreachable = 0x80,
        Unknown_0x100 = 0x100,
        HasLights = 0x200,
        HasMPBX = 0x400,
        HasDoodads = 0x800,
        HasLiquids = 0x1000,
        IsInterior = 0x2000,
        Unknown_0x4000 = 0x4000,
        Unknown_0x8000 = 0x8000,
        AlwaysDraw = 0x10000,
        HasTriangleStrips = 0x20000,
        ShowSkybox = 0x40000,
        IsOceanicWater = 0x80000,
        Unknown_0x100000 = 0x100000,
        IsMountAllowed = 0x200000,
        Unknown_0x400000 = 0x400000,
        Unknown_0x800000 = 0x800000,
        HasTwoVertexShadingSets = 0x1000000,
        HasTwoTextureCoordinateSets = 0x2000000,
        IsAntiportal = 0x4000000,
        Unknown_0x8000000 = 0x8000000,
        Unknown_0x10000000 = 0x10000000,
        ExteriorCull = 0x20000000,
        HasThreeTextureCoordinateSets = 0x40000000,
        Unknown_0x80000000 = 0x80000000
    }
}
