// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Models.Structures;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class GEOS : BaseChunk, IReadOnlyCollection<IGeoset>
    {
        IGeoset[] Geosets;

        public GEOS(BinaryReader br, uint version) : base(br)
        {
            br.BaseStream.Position += Size;
            return;

            if (version == 1500)
            {
                Geosets = new Geoset1500[] { new Geoset1500(br) };
            }
            else
            {
                Geosets = new Geoset1300[br.ReadUInt32()];
                for (int i = 0; i < Geosets.Length; i++)
                    Geosets[i] = new Geoset1300(br);
            }
        }

        public int Count => Geosets.Length;
        public IEnumerator<IGeoset> GetEnumerator() => Geosets.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Geosets.AsEnumerable().GetEnumerator();
    }

    public interface IGeoset
    {
        uint NrOfVertices { get; set; }
        List<CVector3> Vertices { get; set; }
        List<CVector3> Normals { get; set; }
        List<CVector2> TexCoords { get; set; }
        uint NrOfFaceVertices { get; set; }
        List<CVertex> FaceVertices { get; set; }
        List<uint> BoneIndexes { get; set; }
        List<uint> BoneWeights { get; set; }
        uint MaterialId { get; set; }
        Dictionary<byte, List<CVector3>> GroupedVertices { get; set; }
        List<byte[]> GetIndicies();
        CVector3 GetCenter();
    }

    public class Geoset1300 : IGeoset
    {
        public uint TotalSize { get; set; }
        public uint NrOfVertices { get; set; }
        public List<CVector3> Vertices { get; set; } = new List<CVector3>();
        public uint NrOfNormals { get; set; }
        public List<CVector3> Normals { get; set; } = new List<CVector3>();
        public uint NrOfTexCoords { get; set; }
        public List<CVector2> TexCoords { get; set; } = new List<CVector2>();

        //MDLPRIMITIVES
        public uint NrOfFaceTypeGroups { get; set; }
        public List<byte> FaceTypes { get; set; } = new List<byte>();
        public uint NrOfFaceGroups { get; set; }
        public List<uint> FaceGroups { get; set; } = new List<uint>();
        public uint NrOfFaceVertices { get; set; }
        public List<CVertex> FaceVertices { get; set; } = new List<CVertex>();

        public uint NrOfVertexGroupIndices { get; set; }
        public List<byte> VertexGroupIndices { get; set; } = new List<byte>();
        public uint NrOfMatrixGroups { get; set; }
        public List<uint> MatrixGroups { get; set; } = new List<uint>();
        public uint NrOfMatrixIndexes { get; set; }
        public List<uint> MatrixIndexes { get; set; } = new List<uint>();
        public uint NrOfBoneIndexes { get; set; }
        public List<uint> BoneIndexes { get; set; } = new List<uint>();
        public uint NrOfBoneWeights { get; set; }
        public List<uint> BoneWeights { get; set; } = new List<uint>();

        public uint MaterialId { get; set; }
        public CExtent Bounds { get; set; }
        public uint SelectionGroup { get; set; }
        public bool Unselectable { get; set; }

        public uint NrOfExtents { get; set; }
        public List<CExtent> Extents { get; set; } = new List<CExtent>();

        public Dictionary<byte, List<CVector3>> GroupedVertices { get; set; } = new Dictionary<byte, List<CVector3>>();


        public Geoset1300(BinaryReader br)
        {
            TotalSize = br.ReadUInt32();
            long end = TotalSize + br.BaseStream.Position;
            br.BaseStream.Position += TotalSize;
            return;

            //Vertices
            if (br.HasTag("VRTX"))
            {
                NrOfVertices = br.ReadUInt32();
                for (int i = 0; i < NrOfVertices; i++)
                    Vertices.Add(new CVector3(br));
            }

            //Normals
            if (br.HasTag("NRMS"))
            {
                NrOfNormals = br.ReadUInt32();
                for (int i = 0; i < NrOfNormals; i++)
                    Normals.Add(new CVector3(br));
            }

            //TexCoords
            if (br.HasTag("UVAS"))
            {
                NrOfTexCoords = br.ReadUInt32(); //Amount of groups
                for (int i = 0; i < NrOfNormals * NrOfTexCoords; i++)
                    TexCoords.Add(new CVector2(br));
            }

            //Face Group Type
            if (br.HasTag("PTYP"))
            {
                NrOfFaceTypeGroups = br.ReadUInt32();
                FaceTypes.AddRange(br.ReadBytes((int)NrOfFaceTypeGroups));
            }

            //Face Groups
            if (br.HasTag("PCNT"))
            {
                NrOfFaceGroups = br.ReadUInt32();
                for (int i = 0; i < NrOfFaceGroups; i++)
                    FaceGroups.Add(br.ReadUInt32());
            }

            //Indexes
            if (br.HasTag("PVTX"))
            {
                NrOfFaceVertices = br.ReadUInt32();
                for (int i = 0; i < NrOfFaceVertices / 3; i++)
                    FaceVertices.Add(new CVertex(br));
            }

            //Vertex Groups 
            if (br.HasTag("GNDX"))
            {
                NrOfVertexGroupIndices = br.ReadUInt32();
                VertexGroupIndices.AddRange(br.ReadBytes((int)NrOfVertexGroupIndices));
            }

            //Matrix Groups
            if (br.HasTag("MTGC"))
            {
                NrOfMatrixGroups = br.ReadUInt32();
                for (int i = 0; i < NrOfMatrixGroups; i++)
                    MatrixGroups.Add(br.ReadUInt32());
            }

            //Matrix Indexes
            if (br.HasTag("MATS"))
            {
                NrOfMatrixIndexes = br.ReadUInt32();
                for (int i = 0; i < NrOfMatrixIndexes; i++)
                    MatrixIndexes.Add(br.ReadUInt32());
            }

            //Bone Indexes
            if (br.HasTag("BIDX"))
            {
                NrOfBoneIndexes = br.ReadUInt32();
                for (int i = 0; i < NrOfBoneIndexes; i++)
                    BoneIndexes.Add(br.ReadUInt32());
            }

            //Bone Weights
            if (br.HasTag("BWGT"))
            {
                NrOfBoneWeights = br.ReadUInt32();
                for (int i = 0; i < NrOfBoneWeights; i++)
                    BoneWeights.Add(br.ReadUInt32());
            }

            MaterialId = br.ReadUInt32();
            SelectionGroup = br.ReadUInt32();
            Unselectable = br.ReadUInt32() == 1;
            Bounds = new CExtent(br);

            //Extents
            NrOfExtents = br.ReadUInt32();
            for (int i = 0; i < NrOfExtents; i++)
                Extents.Add(new CExtent(br));

            //Grouped Vertices
            for (int i = 0; i < NrOfVertices; i++)
            {
                if (!GroupedVertices.ContainsKey(VertexGroupIndices[i]))
                    GroupedVertices.Add(VertexGroupIndices[i], new List<CVector3>());

                GroupedVertices[VertexGroupIndices[i]].Add(Vertices[i]);
            }
        }

        public List<byte[]> GetIndicies()
        {
            uint[] matrixindexes = MatrixIndexes.ToArray();

            //Parse the bone indices by slicing the matrix groups
            uint[][] slices = new uint[NrOfMatrixGroups][];
            for (int i = 0; i < NrOfMatrixGroups; i++)
            {
                int offset = (i == 0 ? 0 : (int)MatrixGroups[i - 1]);

                slices[i] = new uint[MatrixGroups[i]];
                Array.Copy(matrixindexes, offset, slices[i], 0, slices[i].Length);
            }

            //Construct the final bone arrays
            List<byte[]> boneIndices = new List<byte[]>();
            for (int i = 0; i < NrOfVertices; i++)
            {
                uint[] slice = slices[VertexGroupIndices[i]];
                byte[] indicies = new byte[4];

                //TODO some slices have more than 4 bone indicies what do??
                for (int j = 0; j < Math.Min(slice.Length, 4); j++)
                    indicies[j] = (byte)(slice[j]);

                boneIndices.Add(indicies);
            }

            return boneIndices;
        }

        public CVector3 GetCenter() => new CVector3(Vertices.Average(x => x.X), Vertices.Average(x => x.Y), Vertices.Average(x => x.Z));
    }

    public class Geoset1500 : IGeoset
    {
        #region Interface
        public uint NrOfVertices { get; set; }
        public List<CVector3> Vertices { get; set; } = new List<CVector3>();
        public List<CVector3> Normals { get; set; } = new List<CVector3>();
        public List<CVector2> TexCoords { get; set; } = new List<CVector2>();

        //MDLPRIMITIVES
        public uint NrOfFaceVertices { get; set; }
        public List<CVertex> FaceVertices { get; set; } = new List<CVertex>();

        public List<uint> BoneIndexes { get; set; } = new List<uint>();
        public List<uint> BoneWeights { get; set; } = new List<uint>();

        public uint MaterialId { get; set; }

        public uint NrOfExtents { get; set; }
        public List<CExtent> Extents { get; set; } = new List<CExtent>();
        #endregion

        public Geoset1500(BinaryReader br)
        {
            var primitivecount = br.ReadInt32();

            // MDLPRIVITIVES v2
            MDLGEOSECTION[] primitives = new MDLGEOSECTION[primitivecount];
            for (int i = 0; i < primitivecount; i++)
                primitives[i] = new MDLGEOSECTION(br);

            for (int i = 0; i < primitivecount; i++)
            {
                MDLVERTEX[] vertices = new MDLVERTEX[primitives[i].numVertices];
                for (int x = 0; x < vertices.Length; x++)
                    vertices[x] = new MDLVERTEX(br);

                int primitiveType = br.ReadInt32();  // 0x3 = Triangle, 
                int pad = br.ReadInt32();

                int numPrimitiveIndices = br.ReadInt32(); // matches MDLGEOSECTION
                int maxPrimitiveVertex = br.ReadInt32();
                CVertex[] primitiveVertices = Enumerable.Range(0, numPrimitiveIndices / 3).Select(x => new CVertex(br)).ToArray();

                if (numPrimitiveIndices % 8 != 0)
                {
                    ushort[] padding = Enumerable.Range(0, 8 - (numPrimitiveIndices % 8)).Select(x => br.ReadUInt16()).ToArray();
                    if (!padding.All(x => x == 0))
                        throw new Exception("not padding...");
                }
            }
        }

        internal class MDLGEOSECTION
        {
            public int numVertices;
            public int numPrimitiveTypes;
            public int numPrimitiveIndices;

            public int MaterialId;
            public int SelectionGroup;
            public int GeosetIndex;
            public int Flags;
            public CVector3 CenterBounds;
            public float BoundsRadius;

            public int Unknown1;
            public int Unknown2;

            public MDLGEOSECTION(BinaryReader br)
            {
                MaterialId = br.ReadInt32();
                CenterBounds = new CVector3(br);
                BoundsRadius = br.ReadSingle();
                SelectionGroup = br.ReadInt32();
                GeosetIndex = br.ReadInt32();
                Flags = br.ReadInt32(); // &1: Unselectable, &2: unused, &4 ?, &8: ?, &0x10: Project2D, &0x20: ?  (SHADERSKIN, NOFALLBACK, BATCHES unused MDLTOKENS)

                br.AssertTag("PVTX");
                numVertices = br.ReadInt32(); // count of MDXVertex
                br.AssertTag("PTYP");
                numPrimitiveTypes = br.ReadInt32();
                br.AssertTag("PVTX");
                numPrimitiveIndices = br.ReadInt32();

                Unknown1 = br.ReadInt32();
                Unknown2 = br.ReadInt32();
            }
        }

        internal class MDLVERTEX
        {
            public CVector3 Position;
            public byte[] BoneWeights;
            public byte[] BoneIndices;
            public CVector3 Normal;
            public CVector2[] TexCoords;

            public MDLVERTEX(BinaryReader br)
            {
                Position = new CVector3(br);
                BoneWeights = br.ReadBytes(4);
                BoneIndices = br.ReadBytes(4);
                Normal = new CVector3(br);
                TexCoords = Enumerable.Range(0, 2).Select(x => new CVector2(br)).ToArray();
            }
        }
        #region UNUSED
        public Dictionary<byte, List<CVector3>> GroupedVertices { get; set; } = new Dictionary<byte, List<CVector3>>();

        public List<byte[]> GetIndicies() => throw new NotImplementedException();

        public CVector3 GetCenter() => throw new NotImplementedException();
        #endregion
    }
}
