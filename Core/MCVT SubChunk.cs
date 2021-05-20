// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core
{
    /// It's composed of the usual 145 floats, but their order is different : alpha vertices are not interleaved... 
    /// Which means there are all outer vertices first (all 81), then all inner vertices (all 64) in MCVT (and not 9-8-9-8 etc.). 
    /// Unlike 3.x format, MCVT have absolute height values (no height relative to MCNK header 0x70).
    /// </summary>
    public class MCVTSubChunk
    {
        float[] Heights = new float[9 * 9 + 8 * 8];

        public MCVTSubChunk(BinaryReader reader)
        {
            for (int i = 0; i < Heights.Length; i++)
                Heights[i] = reader.ReadSingle();
        }
    }
}