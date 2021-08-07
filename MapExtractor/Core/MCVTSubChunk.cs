// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;

namespace AlphaCoreExtractor.Core
{
    /// It's composed of the usual 145 floats, but their order is different : alpha vertices are not interleaved... 
    /// Which means there are all outer vertices first (all 81), then all inner vertices (all 64) in MCVT (and not 9-8-9-8). 
    /// Unlike 3.x format, MCVT have absolute height values (no height relative to MCNK header 0x70).
    /// </summary>
    public class MCVTSubChunk
    {
        public float[,] V9 = new float[9, 9];
        public float[,] V8 = new float[8, 8];
        public float[] Heights = new float[8 * 8 + 9 * 9];

        public MCVTSubChunk(BinaryReader reader)
        {
            var v9 = reader.ReadBytes(324);
            var v8 = reader.ReadBytes(256);

            using (MemoryStream v9m = new MemoryStream(v9))
            using (MemoryStream v8m = new MemoryStream(v8))
            using (BinaryReader v9r = new BinaryReader(v9m))
            using (BinaryReader v8r = new BinaryReader(v8m))
            {
                int hIndex = 0;
                while (v9r.BaseStream.Position != v9r.BaseStream.Length)
                {
                    for (int i = 0; i < 9; i++, hIndex++)
                        Heights[hIndex] = v9r.ReadSingle();

                    // If we reached the end, skip inner vertices.
                    if (v8r.BaseStream.Position != v8r.BaseStream.Length)
                        for (int j = 0; j < 8; j++, hIndex++)
                            Heights[hIndex] = v8r.ReadSingle();
                }
            }

            reader.BaseStream.Position -= (Heights.Length * 4);
            using (BinaryReader outerVerticesReader = new BinaryReader(new MemoryStream(reader.ReadBytes(324)))) // 81 * 4bytes
            {
                for (int x = 0; x < 9; x++)
                    for (int y = 0; y < 9; y++)
                        V9[x, y] = outerVerticesReader.ReadSingle();
            }

            using (BinaryReader innerVerticesReader = new BinaryReader(new MemoryStream(reader.ReadBytes(256)))) // 64 * 4bytes
            {
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 8; y++)
                        V8[x, y] = innerVerticesReader.ReadSingle();
            }
        }
    }
}