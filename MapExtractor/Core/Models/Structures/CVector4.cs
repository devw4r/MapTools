// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System;
using System.IO;

namespace AlphaCoreExtractor.Core.Models.Structures
{
    public class CVector4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public CVector4() { }

        public CVector4(BinaryReader br)
        {
            ReadCompressed(br);
        }

        /// <summary>
        /// Alpha stored Quaternions compressed in a int64
        /// <para>Reversed from NTempest::C4QuaternionCompressed</para>
        /// </summary>
        /// <param name="br"></param>
        private void ReadCompressed(BinaryReader br)
        {
            const double multiplier = 0.00000095367432;

            Func<double, float> DoubleToFloat = (d) => Math.Max(Math.Min((float)d, float.MaxValue), float.MinValue); //Prevent infinity

            long value = br.ReadInt64();

            X = DoubleToFloat((value >> 42) * (multiplier / 2.0));
            Y = DoubleToFloat(((value << 22) >> 43) * multiplier);
            Z = DoubleToFloat((value & 0x1FFFFF) * multiplier);

            // calculate W
            double len = X * X + Y * Y + Z * Z;
            if (Math.Abs(len - 1.0) >= multiplier)
            {
                double w = 1.0 - len;
                if (w >= 0)
                    W = DoubleToFloat(Math.Sqrt(w));
            }
        }

        public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}, W: {W}";
    }
}
