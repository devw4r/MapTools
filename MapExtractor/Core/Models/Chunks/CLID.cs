// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Structures;
using AlphaCoreExtractor.Core.Models.Cache;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class CLID : BaseChunk
    {
        public uint NrOfVertices;
        public uint NrOfTriIndices;
        public uint NrOfFacetNormals;

        public CLID(BinaryReader br, uint version, bool wmo, MDX model) : base(br)
		{
            model.GeometryHolder = new GeometryHolder();

            br.AssertTag("VRTX");
            NrOfVertices = br.ReadUInt32();
            model.GeometryHolder.BoundingVertices = new Vector3[NrOfVertices];
            for (int i = 0; i < NrOfVertices; i++)
                model.GeometryHolder.BoundingVertices[i] = Vector3.FromReader(br);

            br.AssertTag("TRI ");
            NrOfTriIndices = br.ReadUInt32();
            model.GeometryHolder.BoundingTriangles = new Index3[NrOfTriIndices / 3];
            var triIndex = 0;
            for (int i = 0; i < NrOfTriIndices; i+=3)
            {
                var index0 = br.ReadInt16();
                var index1 = br.ReadInt16();
                var index2 = br.ReadInt16();
                var triIdx = new Index3
                {
                    Index0 = index2,
                    Index1 = wmo ? index0 : index1,
                    Index2 = wmo ? index1 : index0
                };
                model.GeometryHolder.BoundingTriangles[triIndex] = triIdx;
                triIndex++;
            }

            br.AssertTag("NRMS");
            NrOfFacetNormals = br.ReadUInt32();
            model.GeometryHolder.BoundingNormals = new Vector3[NrOfFacetNormals];
            for (int i = 0; i < NrOfFacetNormals; i++)
                model.GeometryHolder.BoundingNormals[i] = Vector3.FromReader(br);
        }
    }
}
