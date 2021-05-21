// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Collections.Generic;

namespace AlphaCoreExtractor.Core
{
    public class MCLQSubChunk
    {
        public CRange Range;
        public List<SLVert> SLVerts = new List<SLVert>(); // 81
        public SLTiles SLTiles;
        public uint NFlowVS;
        List<SWFlowv> Flowvs = new List<SWFlowv>(); // 2

        public MCLQSubChunk(BinaryReader reader)
        {
            Range = new CRange(reader);

            for (int i = 0; i < 81; i++)
                SLVerts.Add(new SLVert(reader));

            SLTiles = new SLTiles(reader);
            NFlowVS = reader.ReadUInt32();

            for (int i = 0; i < 2; i++)
                Flowvs.Add(new SWFlowv(reader));
        }
    }
}
