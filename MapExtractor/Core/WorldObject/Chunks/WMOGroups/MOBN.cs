// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Collections.Generic;

using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Chunks.WMOGroups
{
    public class MOBN
    {
        public MOBN_Flags Flags;
        public short NegChild;
        public short PosChild;
        public ushort NFaces;
        public uint FaceStart;
        public float PlaneDist;

        public MOBN Positive;
        public MOBN Negative;
        public List<Index3> TriangleIndices = new List<Index3>();

        public MOBN(BinaryReader br)
        {
            Flags = (MOBN_Flags)br.ReadInt16();
            NegChild = br.ReadInt16();
            PosChild = br.ReadInt16();
            NFaces = br.ReadUInt16();
            FaceStart = br.ReadUInt32();
            PlaneDist = br.ReadSingle();
        }

        public void GetIndices(List<int> indices)
        {
            foreach (var triangle in TriangleIndices)
            {
                indices.Add(triangle.Index0);
                indices.Add(triangle.Index1);
                indices.Add(triangle.Index2);
            }

            if (Positive != null)
                Positive.GetIndices(indices);
            Negative?.GetIndices(indices);
        }
    }
}
