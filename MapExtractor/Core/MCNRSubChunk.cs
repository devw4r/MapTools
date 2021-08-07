// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core
{
    /// <summary>
    /// Same as 3.x format, except values order which is like alpha MCVT : all outer values first, then all inner ones.
    /// Normalized values, X, Z, Y. 127 == 1.0, -127 == -1.0.
    /// </summary>
    public class MCNRSubChunk
    {
        public Vector<float>[] Normals = new Vector<float>[9 * 9 + 8 * 8];  // [9 * 9 + 8 * 8] (145);

        public MCNRSubChunk(BinaryReader reader)
        {
            // Interleave vertices (9-8-9-8)
            using (BinaryReader outerVerticesReader = new BinaryReader(new MemoryStream(reader.ReadBytes(243)))) // 81 * 3bytes
            using (BinaryReader innerVerticesReader = new BinaryReader(new MemoryStream(reader.ReadBytes(192)))) // 64 * 3bytes
            {
                int hIndex = 0;
                while (outerVerticesReader.BaseStream.Position != outerVerticesReader.BaseStream.Length)
                {
                    for (int i = 0; i < 9; i++, hIndex++)
                    {
                        var normalZ = outerVerticesReader.ReadSByte();
                        var normalX = outerVerticesReader.ReadSByte();
                        var normalY = outerVerticesReader.ReadSByte();
                        Normals[hIndex] = Vector<float>.Build(-(float)normalX / 127.0f, normalY / 127.0f, -(float)normalZ / 127.0f);
                    }

                    // If we reached the end, skip inner vertices.
                    if (innerVerticesReader.BaseStream.Position != innerVerticesReader.BaseStream.Length)
                    {
                        for (int j = 0; j < 8; j++, hIndex++)
                        {
                            var normalZ = innerVerticesReader.ReadSByte();
                            var normalX = innerVerticesReader.ReadSByte();
                            var normalY = innerVerticesReader.ReadSByte();
                            Normals[hIndex] = Vector<float>.Build(-(float)normalX / 127.0f, normalY / 127.0f, -(float)normalZ / 127.0f);
                        }
                    }
                }
            }

            /*
            // TODO: PAD bytes do not match with the comment below.
             * About pad: 0.5.3.3368 lists this as padding always 0 112 245 18 0 8 0 0 0 84 245 18 0.
            */
            var pad = reader.ReadBytes(13);
        }

        /// <summary>
        /// 145 Floats for the 9 x 9 and 8 x 8 grid of Normal data.
        /// </summary>
        public Vector<float>[,] GetLowResNormalMatrix()
        {
            var normals = new Vector<float>[9, 9];
            for (var x = 0; x < 17; x++)
            {
                if (x % 2 != 0) continue;
                for (var y = 0; y < 9; y++)
                {
                    var count = ((x / 2) * 9) + ((x / 2) * 8) + y;
                    normals[y, x / 2] = Normals[count];
                }
            }
            return normals;
        }
    }
}
