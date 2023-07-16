// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Collections.Generic;

using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Readers;
using AlphaCoreExtractor.Core.Structures;
using AlphaCoreExtractor.Core.WorldObject.Structures;
using AlphaCoreExtractor.Core.WorldObject.Chunks.WMOGroups;
using AlphaCoreExtractor.Core.Cache;

namespace AlphaCoreExtractor.Core.WorldObject
{
    public class WMOGroup : IDisposable
    {
        public ObjectGroupHeader Header { get; set; }

        public int TriangleCount;
        public int VertexCount;
        public List<MOPY> TriangleMaterials = new List<MOPY>();
        public List<Vector2> TextureVertices = new List<Vector2>();
        public List<Vector2> LightMapVertices = new List<Vector2>();
        public List<MOBA> Batches = new List<MOBA>();
        public ushort[] LightReferences;
        public List<ushort> DoodadReferences = new List<ushort>();
        public List<MOBN> BSPNodes = new List<MOBN>();
        public List<ushort> MOBR = new List<ushort>();
        public List<CImVector> VertexColors = new List<CImVector>();
        public List<LiquidInformation> LiquidInformation = new List<LiquidInformation>();
        public ushort[] TriangleStripIndices;
        //public MOLM[] LightMaps;
        //public MOLD[] LightMapTexels;
        //public MPBX[] MPBV;
        //public MPBX[] MPBP;
        //public ushort[] MPBI;
        //public Vector3[] MPBG;

        /// <summary>
        /// MOVT - Verticies
        /// </summary>
        public List<Vector3> Vertices = new List<Vector3>();

        /// <summary>
        /// Normals
        /// </summary>
        public List<Vector3> Normals = new List<Vector3>();

        /// <summary>
        /// MOVI - Triangle Indices.
        /// Rendering Indices information for the Group.
        /// </summary>
        public List<Index3> Indices = new List<Index3>();

        /// <summary>
        /// Used to read file tokens and validate chunks.
        /// </summary>
        private readonly DataChunkHeader DataChunkHeader = new DataChunkHeader();

        public WMOGroup(BinaryReaderProgress br)
        {
            Header = new ObjectGroupHeader(br);
            
            if (!ReadMOPY(br))
                return;

            if (!ReadMOVT(br))
                return;

            if (!ReadMONR(br))
                return;

            if (!ReadMOTV(br))
                return;

            if (!ReadMOLV(br))
                return;

            if (!ReadMOIN(br))
                return;

            if (!ReadMOBA(br))
                return;

            // Optional chunks.
            if (Header.Flags.HasFlag(MOGP_Flags.HasLights))
                if (!ReadMOLR(br))
                    return;

            if (Header.Flags.HasFlag(MOGP_Flags.HasDoodads))
                if (!ReadMODR(br))
                    return;

            if (Header.Flags.HasFlag(MOGP_Flags.HasBSP))
            {
                if (!ReadMOBN(br))
                    return;

                if (!ReadMOBR(br))
                    return;
                else
                    LinkBSPNodes();
            }

            if (Header.Flags.HasFlag(MOGP_Flags.HasMPBX))
            {
                if (!ReadMPBV(br))
                    return;

                if (!ReadMPBP(br))
                    return;

                if (!ReadMPBI(br))
                    return;

                if (!ReadMPBG(br))
                    return;
            }

            if (Header.Flags.HasFlag(MOGP_Flags.HasVertexColors))
                if (!ReadMOCV(br))
                    return;

            if(Header.Flags.HasFlag(MOGP_Flags.HasLightmap))
            {
                if (!ReadMOLM(br))
                    return;

                if (!ReadMOLD(br))
                    return;
            }

            if (Header.Flags.HasFlag(MOGP_Flags.HasLiquids))
                if (!ReadMLIQ(br))
                    return;
        }

        private void LinkBSPNodes()
        {
            foreach (var node in BSPNodes)
            {
                if (node.PosChild != -1)
                    node.Positive = BSPNodes[node.PosChild];

                if (node.NegChild != -1)
                    node.Negative = BSPNodes[node.NegChild];

                // Empty
                if (node.FaceStart == 0 && node.NFaces == 0)
                    continue;

                node.TriangleIndices = new List<Index3>(node.NFaces);
                for (int j = 0; j < node.NFaces; j++)
                {
                    var mobrIndex = (int)(node.FaceStart + j);
                    if (mobrIndex >= MOBR.Count)
                        continue;

                    var index = MOBR[mobrIndex];
                    if (index >= TriangleMaterials.Count)
                        continue;

                    // Enabling mopy flags check ends up with a lot of missing triangles, hence all this nasty checks for array lengths.
                    // Still, prety sure there must be a right way to do this.

                    //var mopy = group.TriangleMaterials[index];
                    //if (!mopy.Flags.HasAnyFlag(MOPY_Flags.CollisionMask | MOPY_Flags.Detail))
                    //    continue;
                    //if (mopy.Flags.HasAnyFlag(MOPY.MaterialFlags.NoCollision))
                    //    continue;

                    if (index >= Indices.Count)
                        continue;

                    var triIndex = Indices[index];
                    node.TriangleIndices.Add(triIndex);
                }
            }
        }

        private bool ReadMOLD(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOLD)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOLD]"}");

                if (Globals.Verbose)
                    Logger.Success($"[MOLD]");

                br.ReadBytes(DataChunkHeader.Size);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMOLM(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOLM)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOLM]"}");

                if (Globals.Verbose)
                    Logger.Success($"[MOLM]");

                br.ReadBytes(DataChunkHeader.Size);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMOIN(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOIN)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOIN]"}");

                long endPos = br.BaseStream.Position + DataChunkHeader.Size;
                while (br.BaseStream.Position < endPos)
                    Indices.Add(Index3.FromReader(br));

                if (Globals.Verbose)
                    Logger.Success($"[MOIN]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMOTV(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOTV)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOTV]"}");

                br.ReadBytes(DataChunkHeader.Size);

                if (Globals.Verbose)
                    Logger.Success($"[MOTV]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMPBG(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MPBG)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MPBG]"}");

                br.ReadBytes(DataChunkHeader.Size);

                if (Globals.Verbose)
                    Logger.Success($"[MPBG]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMPBI(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MPBI)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MPBI]"}");

                br.ReadBytes(DataChunkHeader.Size);

                if (Globals.Verbose)
                    Logger.Success($"[MPBI]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMPBP(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MPBP)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MPBP]"}");

                br.ReadBytes(DataChunkHeader.Size);

                if (Globals.Verbose)
                    Logger.Success($"[MPBP]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMPBV(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MPBV)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MPBV]"}");

                br.ReadBytes(DataChunkHeader.Size);

                if (Globals.Verbose)
                    Logger.Success($"[MPBV]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMLIQ(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MLIQ)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MLIQ]"}");

                long endPos = br.BaseStream.Position + DataChunkHeader.Size;
                while (br.BaseStream.Position < endPos)
                    LiquidInformation.Add(new LiquidInformation(br));

                if (Globals.Verbose)
                    Logger.Success($"[MLIQ]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMOCV(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOCV)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOCV]"}");

                long endPos = br.BaseStream.Position + DataChunkHeader.Size;
                while (br.BaseStream.Position < endPos)
                    VertexColors.Add(new CImVector(br));

                if (Globals.Verbose)
                    Logger.Success($"[MOCV]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMOBR(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOBR)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOBR]"}");

                long endPos = br.BaseStream.Position + DataChunkHeader.Size;
                while (br.BaseStream.Position < endPos)
                    MOBR.Add(br.ReadUInt16());

                if (Globals.Verbose)
                    Logger.Success($"[MOBR]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMOBN(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOBN)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOBN]"}");

                long endPos = br.BaseStream.Position + DataChunkHeader.Size;
                while (br.BaseStream.Position < endPos)
                    BSPNodes.Add(new MOBN(br));

                if (Globals.Verbose)
                    Logger.Success($"[MOBN]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMODR(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MODR)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MODR]"}");

                long endPos = br.BaseStream.Position + DataChunkHeader.Size;
                while (br.BaseStream.Position < endPos)
                    DoodadReferences.Add(br.ReadUInt16());

                if (Globals.Verbose)
                    Logger.Success($"[MODR]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// TODO
        /// </summary>
        private bool ReadMOLR(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOLR)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOLR]"}");

                br.ReadBytes(DataChunkHeader.Size);

                if (Globals.Verbose)
                    Logger.Success($"[MOLR]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// TODO
        /// </summary>
        private bool ReadMOBA(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOBA)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOBA]"}");

                br.ReadBytes(DataChunkHeader.Size);

                if (Globals.Verbose)
                    Logger.Success($"[MOBA]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// TODO
        /// </summary>
        private bool ReadMOLV(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOLV)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOLV]"}");

                br.ReadBytes(DataChunkHeader.Size);

                if (Globals.Verbose)
                    Logger.Success($"[MOLV]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMONR(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MONR)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MONR]"}");

                long endPos = br.BaseStream.Position + DataChunkHeader.Size;
                while (br.BaseStream.Position < endPos)
                    Normals.Add(Vector3.FromReader(br));

                if (Globals.Verbose)
                    Logger.Success($"[MONR]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMOVT(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOVT)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOVT]"}");

                long endPos = br.BaseStream.Position + DataChunkHeader.Size;
                while (br.BaseStream.Position < endPos)
                    Vertices.Add(Vector3.FromReader(br));
                VertexCount = Vertices.Count;

                if (Globals.Verbose)
                    Logger.Success($"[MOVT]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMOPY(BinaryReaderProgress br)
        {
            try
            {
                if (br.IsEOF())
                    return false;

                DataChunkHeader.Fill(br);

                if (DataChunkHeader.Token != Tokens.MOPY)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOPY]"}");

                long endPos = br.BaseStream.Position + DataChunkHeader.Size;
                while (br.BaseStream.Position < endPos)
                    TriangleMaterials.Add(new MOPY(br));
                
                TriangleCount = TriangleMaterials.Count;

                if (Globals.Verbose)
                    Logger.Success($"[MOPY]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        public void Dispose()
        {
            Header = null;
            TriangleMaterials?.Clear();
            TriangleMaterials = null;
            TextureVertices?.Clear();
            TextureVertices = null;
            LightMapVertices?.Clear();
            LightMapVertices = null;
            Batches?.Clear();
            Batches = null;
            DoodadReferences?.Clear();
            DoodadReferences = null;
            BSPNodes?.Clear();
            BSPNodes = null;
            VertexColors?.Clear();
            VertexColors = null;
            LiquidInformation?.Clear();
            LiquidInformation = null;
            TriangleStripIndices = null;

            if (Normals != null)
            {
                foreach (Vector3 v3 in Normals)
                    StorageRoom.PushVector3(v3);
                Normals?.Clear();
                Normals = null;
            }

            if (Vertices != null)
            {
                foreach (Vector3 v3 in Vertices)
                    StorageRoom.PushVector3(v3);
                Vertices?.Clear();
                Vertices = null;
            }
        }
    }
}
