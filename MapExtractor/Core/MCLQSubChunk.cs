// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Collections.Generic;
using AlphaCoreExtractor.Helpers.Enums;

namespace AlphaCoreExtractor.Core
{
    public class MCLQSubChunk
    {
        public CRange Range;
        public object[,] Verts = new object[9, 9];
        public byte[,] Flags = new byte[8, 8];
        public uint NFlowVS;
        List<SWFlowv> Flowvs = new List<SWFlowv>(); // 2
        public SMChunkFlags Flag = SMChunkFlags.NONE;
        public bool HasHeight;

        public float GetHeight(int y, int x)
        {
            if (Verts[y, x] is SMVert magmaVert)
                return magmaVert.Height;
            else if (Verts[y, x] is SWVert waterVert)
                return waterVert.Height;
            else
                return 0;
        }

        public MCLQSubChunk(BinaryReader reader, SMChunkFlags flag)
        {
            Flag = flag;
            Range = new CRange(reader);

            switch (flag)
            {
                case SMChunkFlags.FLAG_LQ_OCEAN:
                    for (int i = 0; i < 9; i++)
                        for (int j = 0; j < 9; j++)
                            Verts[i, j] = new SOVert(reader); // Ocean Vert
                    break;
                case SMChunkFlags.FLAG_LQ_MAGMA:
                    HasHeight = true;
                    for (int i = 0; i < 9; i++)
                        for (int j = 0; j < 9; j++)
                            Verts[i, j] = new SMVert(reader); // Magma Vert
                    break;
                default:
                    HasHeight = true;
                    for (int i = 0; i < 9; i++)
                        for (int j = 0; j < 9; j++)
                            Verts[i, j] = new SWVert(reader); // Water Vert
                    break;
            }

            // Flags
            // 1<<0 - ocean
            // 1<<1 - lava/slime
            // 1<<2 - water
            // 1<<6 - all water
            // 1<<7 - dark water
            // == 0x0F - not show liquid
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    Flags[i, j] = reader.ReadByte();

            NFlowVS = reader.ReadUInt32();

            for (int i = 0; i < 2; i++)
                Flowvs.Add(new SWFlowv(reader));
        }
    }
}
