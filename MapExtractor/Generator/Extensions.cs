// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Text;
using System.Reflection;

using AlphaCoreExtractor.Core;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Generator
{
    public static class Extensions
    {
        public static void WriteMapVersion(this FileStream fs)
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var version = $"ACMAP_{assembly.Version.Major}.{assembly.Version.Build}0";
            fs.Write(Encoding.ASCII.GetBytes(version), 0, 10);
        }

        public static Cell TransformHeightData(this CMapArea tileBlock)
        {
            Cell cell = new Cell();

            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    cell.V8[x, y] = (float)tileBlock.Tiles[x / 8, y / 8].MCVTSubChunk.V8[x % 8, y % 8];
                    cell.V9[x, y] = (float)tileBlock.Tiles[x / 8, y / 8].MCVTSubChunk.V9[x % 8, y % 8];
                }

                cell.V9[x, 128] = (float)tileBlock.Tiles[x / 8, 15].MCVTSubChunk.V9[x % 8, 8];
                cell.V9[128, x] = (float)tileBlock.Tiles[15, x / 8].MCVTSubChunk.V9[8, x % 8];
            }

            cell.V9[128, 128] = (float)tileBlock.Tiles[15, 15].MCVTSubChunk.V9[8, 8];

            return cell;
        }

        public static float CalculateZ(this Cell cell, double cy, double cx)
        {
            var x = (cy * Constants.TileSizeYrds) / ((double)256 - 1);
            var y = (cx * Constants.TileSizeYrds) / ((double)256 - 1);
            return cell.GetZ(x, y);
        }

        private static float GetZ(this Cell cell, double x, double z)
        {
            Vector<double>[] v = new Vector<double>[3] { new Vector<double>(), new Vector<double>(), new Vector<double>() };
            Vector<double> p = new Vector<double>();

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
            v[0].Y = cell.V8[xc, zc];
            v[0].Z = Constants.UnitSize / 2;

            if (lx > lz)
            {
                v[1].X = Constants.UnitSize;
                v[1].Y = cell.V9[xc + 1, zc];
                v[1].Z = 0;
            }
            else
            {
                v[1].X = 0.0;
                v[1].Y = cell.V9[xc, zc + 1];
                v[1].Z = Constants.UnitSize;
            }

            if (lz > Constants.UnitSize - lx)
            {
                v[2].X = Constants.UnitSize;
                v[2].Y = cell.V9[xc + 1, zc + 1];
                v[2].Z = Constants.UnitSize;
            }
            else
            {
                v[2].X = 0;
                v[2].Y = cell.V9[xc, zc];
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
