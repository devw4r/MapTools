// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;

namespace AlphaCoreExtractor.Core.Chunks
{
    /// It's composed of the usual 145 floats, but their order is different : alpha vertices are not interleaved... 
    /// Which means there are all outer vertices first (all 81), then all inner vertices (all 64) in MCVT (and not 9-8-9-8). 
    /// Unlike 3.x format, MCVT have absolute height values (no height relative to MCNK header 0x70).
    /// </summary>
    public class MCVTSubChunk : IDisposable
    {
        public float[,] V9 = new float[9, 9];
        public float[,] V8 = new float[8, 8];

        // Interleaved 9-8-9-8
        public float[] Heights = new float[8 * 8 + 9 * 9];

        private float[,] lowResHeightsMatrix;
        private float[,] highResHeightsMatrix;

        public MCVTSubChunk(BinaryReader reader)
        {
            // Interleaved heights.
            var bytes_v9 = reader.ReadBytes(324);
            var bytes_v8 = reader.ReadBytes(256);
            using (MemoryStream msV9 = new MemoryStream(bytes_v9))
            using (MemoryStream msV8 = new MemoryStream(bytes_v8))
            using (BinaryReader v9Reader = new BinaryReader(msV9))
            using (BinaryReader v8Reader = new BinaryReader(msV8))
            {
                int hIndex = 0;
                while (v9Reader.BaseStream.Position != v9Reader.BaseStream.Length)
                {
                    for (int i = 0; i < 9; i++, hIndex++)
                        Heights[hIndex] = v9Reader.ReadSingle();

                    // If we reached the end, skip inner vertices.
                    if (v8Reader.BaseStream.Position != v8Reader.BaseStream.Length)
                        for (int j = 0; j < 8; j++, hIndex++)
                            Heights[hIndex] = v8Reader.ReadSingle();
                }
            }

            // Segmented.
            reader.BaseStream.Position -= (Heights.Length * 4);
            using (BinaryReader outerVerticesReader = new BinaryReader(new MemoryStream(reader.ReadBytes(324)))) // 81 floats * 4bytes
                for (int x = 0; x < 9; x++)
                    for (int y = 0; y < 9; y++)
                        V9[x, y] = outerVerticesReader.ReadSingle();

            using (BinaryReader innerVerticesReader = new BinaryReader(new MemoryStream(reader.ReadBytes(256)))) // 64 floats * 4bytes
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 8; y++)
                        V8[x, y] = innerVerticesReader.ReadSingle();
        }

        /// <summary>
        /// 145 Floats for the 9 x 9 and 8 x 8 grid of height data.
        /// </summary>
        public float[] GetLowResMapArray()
        {
            var heights = new float[81];

            for (var r = 0; r < 17; r++)
            {
                if (r % 2 != 0) continue;
                for (var c = 0; c < 9; c++)
                {
                    var count = ((r / 2) * 9) + ((r / 2) * 8) + c;
                    heights[c + ((r / 2) * 8)] = heights[count];
                }
            }
            return heights;
        }

        /// <summary>
        /// 145 Floats for the 9 x 9 and 8 x 8 grid of height data.
        /// </summary>
        public float[,] GetLowResMapMatrix()
        {
            if (lowResHeightsMatrix != null)
                return lowResHeightsMatrix;

            // *  1    2    3    4    5    6    7    8    9       Row 0
            // *    10   11   12   13   14   15   16   17         Row 1
            // *  18   19   20   21   22   23   24   25   26      Row 2
            // *    27   28   29   30   31   32   33   34         Row 3
            // *  35   36   37   38   39   40   41   42   43      Row 4
            // *    44   45   46   47   48   49   50   51         Row 5
            // *  52   53   54   55   56   57   58   59   60      Row 6
            // *    61   62   63   64   65   66   67   68         Row 7
            // *  69   70   71   72   73   74   75   76   77      Row 8
            // *    78   79   80   81   82   83   84   85         Row 9
            // *  86   87   88   89   90   91   92   93   94      Row 10
            // *    95   96   97   98   99   100  101  102        Row 11
            // * 103  104  105  106  107  108  109  110  111      Row 12
            // *   112  113  114  115  116  117  118  119         Row 13
            // * 120  121  122  123  124  125  126  127  128      Row 14
            // *   129  130  131  132  133  134  135  136         Row 15
            // * 137  138  139  140  141  142  143  144  145      Row 16

            // We only want even rows (starting at 0)
            lowResHeightsMatrix = new float[9, 9];

            var index = 0;
            for (var x = 0; x < 9; x++)
            {
                for (var y = 0; y < 9; y++)
                    lowResHeightsMatrix[x, y] = Heights[index++];
                index += 8;
            }

            return lowResHeightsMatrix;
        }

        public float[,] GetHighResMapMatrix()
        {
            if (highResHeightsMatrix != null)
                return highResHeightsMatrix;

            // *  1    2    3    4    5    6    7    8    9       Row 0
            // *    10   11   12   13   14   15   16   17         Row 1
            // *  18   19   20   21   22   23   24   25   26      Row 2
            // *    27   28   29   30   31   32   33   34         Row 3
            // *  35   36   37   38   39   40   41   42   43      Row 4
            // *    44   45   46   47   48   49   50   51         Row 5
            // *  52   53   54   55   56   57   58   59   60      Row 6
            // *    61   62   63   64   65   66   67   68         Row 7
            // *  69   70   71   72   73   74   75   76   77      Row 8
            // *    78   79   80   81   82   83   84   85         Row 9
            // *  86   87   88   89   90   91   92   93   94      Row 10
            // *    95   96   97   98   99   100  101  102        Row 11
            // * 103  104  105  106  107  108  109  110  111      Row 12
            // *   112  113  114  115  116  117  118  119         Row 13
            // * 120  121  122  123  124  125  126  127  128      Row 14
            // *   129  130  131  132  133  134  135  136         Row 15
            // * 137  138  139  140  141  142  143  144  145      Row 16

            // We only want odd rows (starting at 1).
            highResHeightsMatrix = new float[8, 8];

            var index = 9;
            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 8; y++)
                    highResHeightsMatrix[x, y] = Heights[index++];
                index += 9;
            }

            return highResHeightsMatrix;
        }

        /// <summary>
        /// Flush V8 and V9 verts.
        /// </summary>
        public void Flush()
        {
            V9 = null;
            V8 = null;
        }

        public void Dispose()
        {
            Heights = null;
            lowResHeightsMatrix = null;
            highResHeightsMatrix = null;
        }
    }  
}