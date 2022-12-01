// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Text;
using System.Reflection;

using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Cache;
using AlphaCoreExtractor.Core.Chunks;
using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Generators
{
    public static class Extensions
    {
        public static void WriteMapVersion(this FileStream fs)
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var version = $"ACMAP_{assembly.Version.Major}.{assembly.Version.Build}0";
            fs.Write(Encoding.ASCII.GetBytes(version), 0, 10);
        }

        public static Vector3 ToRecastCoordinates(this ref Vector3 vector)
        {
            return StorageRoom.PopVector3(vector.Y, vector.Z, vector.X);
        }

        public static void ToRecastCoordinates<T>(this Vector<T> vertex)
        {
            if (vertex.CoordsType == CoordinatesType.Recast)
                return;

            var z = vertex.X;
            vertex.X = vertex.Y;
            vertex.Y = vertex.Z;
            vertex.Z = z;

            vertex.CoordsType = CoordinatesType.Recast;
        }

        public static void ToWoWCoordinates<T>(this Vector<T> vertex)
        {
            if (vertex.CoordsType == CoordinatesType.WoW)
                return;

            var z = vertex.X;
            vertex.X = vertex.Z;
            vertex.Z = vertex.Y;
            vertex.Y = z;

            vertex.CoordsType = CoordinatesType.WoW;
        }

        public static float CalculateZ(this LiquidsHeightmap lHeightMap, double cy, double cx)
        {
            var x = (cy * Constants.TileSizeYrds) / ((double)Configuration.ZResolution - 1);
            var y = (cx * Constants.TileSizeYrds) / ((double)Configuration.ZResolution - 1);
            return lHeightMap.GetZ(x, y);
        }

        private static Vector<double>[] v = new Vector<double>[3] { new Vector<double>(), new Vector<double>(), new Vector<double>() };
        private static Vector<double> p = new Vector<double>();
        private static float GetZ(this LiquidsHeightmap lHeightMap, double x, double z)
        {
            // Find out quadrant
            int xc = (int)(x / Constants.UnitSize);
            int zc = (int)(z / Constants.UnitSize);

            if (xc > 127)
                xc = 127;

            if (zc > 127)
                zc = 127;

            double lx = x - xc * Constants.UnitSize;
            double lz = z - zc * Constants.UnitSize;
            p.X = lx;
            p.Z = lz;

            v[0].X = Constants.UnitSize / 2;
            v[0].Y = lHeightMap.V8[xc, zc];
            v[0].Z = Constants.UnitSize / 2;

            if (lx > lz)
            {
                v[1].X = Constants.UnitSize;
                v[1].Y = lHeightMap.V9[xc + 1, zc];
                v[1].Z = 0;
            }
            else
            {
                v[1].X = 0.0;
                v[1].Y = lHeightMap.V9[xc, zc + 1];
                v[1].Z = Constants.UnitSize;
            }

            if (lz > Constants.UnitSize - lx)
            {
                v[2].X = Constants.UnitSize;
                v[2].Y = lHeightMap.V9[xc + 1, zc + 1];
                v[2].Z = Constants.UnitSize;
            }
            else
            {
                v[2].X = 0;
                v[2].Y = lHeightMap.V9[xc, zc];
                v[2].Z = 0;
            }

            return Convert.ToSingle(-Solve(v, p));
        }

        /// <summary>ñ
        /// Plane equation ax+by+cz+d=0
        /// </summary>
        private static double Solve(Vector<double>[] v, Vector<double> p)
        {
            double a = v[0].Y * (v[1].Z - v[2].Z) + v[1].Y * (v[2].Z - v[0].Z) + v[2].Y * (v[0].Z - v[1].Z);
            double b = v[0].Z * (v[1].X - v[2].X) + v[1].Z * (v[2].X - v[0].X) + v[2].Z * (v[0].X - v[1].X);
            double c = v[0].X * (v[1].Y - v[2].Y) + v[1].X * (v[2].Y - v[0].Y) + v[2].X * (v[0].Y - v[1].Y);
            double d = v[0].X * (v[1].Y * v[2].Z - v[2].Y * v[1].Z) + v[1].X * (v[2].Y * v[0].Z - v[0].Y * v[2].Z) + v[2].X * (v[0].Y * v[1].Z - v[1].Y * v[0].Z);

            return ((a * p.X + c * p.Z - d) / b);
        }
    }
}
