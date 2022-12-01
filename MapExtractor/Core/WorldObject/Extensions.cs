// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

namespace AlphaCoreExtractor.Core.WorldObject
{
    public static class Extensions
    {
        public static LiquidTypes To_WMO_Liquid(this WMOGroup group, LiquidBasicTypes flag)
        {
            LiquidBasicTypes basic = flag & LiquidBasicTypes.LiquidBasicTypes_MASK;
            switch (basic)
            {
                case LiquidBasicTypes.LiquidBasicTypes_Water:
                    return group.Header.Flags.HasFlag(WMOLiquidFlags.IsNotWaterButOcean) ? LiquidTypes.LIQUID_WMO_Ocean : LiquidTypes.LIQUID_WMO_Water;
                case LiquidBasicTypes.LiquidBasicTypes_Ocean:
                    return LiquidTypes.LIQUID_WMO_Ocean;
                case LiquidBasicTypes.LiquidBasicTypes_Magma:
                    return LiquidTypes.LIQUID_WMO_Magma;
                case LiquidBasicTypes.LiquidBasicTypes_Slime:
                    return LiquidTypes.LIQUID_WMO_Slime;
            }

            return LiquidTypes.LIQUID_WMO_Water;
        }
    }
}
