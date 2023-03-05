// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Generators;
using AlphaCoreExtractor.Core.Cache;
using AlphaCoreExtractor.Core.Models;
using AlphaCoreExtractor.Core.Chunks;
using AlphaCoreExtractor.Core.Terrain;
using AlphaCoreExtractor.Core.Readers;
using AlphaCoreExtractor.Core.Structures;
using AlphaCoreExtractor.Core.Models.Cache;
using AlphaCoreExtractor.Core.WorldObject.Chunks;
using AlphaCoreExtractor.Core.WorldObject.Structures;
using System.Diagnostics;

namespace AlphaCoreExtractor.Core.WorldObject
{
    public class WMO : BinaryReaderProgress
    {
        public uint Version;

        /// <summary>
        /// WMO Header. (MOHD)
        /// </summary>
        public SMOHeader Header { get; set; }

        /// <summary>
        /// WMO Textures. (MOTX)
        /// </summary>
        public Dictionary<int, string> Textures = new Dictionary<int, string>();

        /// <summary>
        /// WMO Materials. (MOMT)
        /// </summary>
        public List<Material> Materials = new List<Material>();

        /// <summary>
        /// WMO Group Names. (MOGN)
        /// </summary>
        public Dictionary<int, string> GroupNames = new Dictionary<int, string>();

        /// <summary>
        /// WMO Group Information. (MOGI)
        /// </summary>
        public List<GroupInformation> GroupInformation = new List<GroupInformation>();

        /// <summary>
        /// WMO Portal Vertices. (MOPV)
        /// </summary>
        public List<C3Vector> PortalVertices = new List<C3Vector>();

        /// <summary>
        /// WMO Portal Information. (MOPT)
        /// </summary>
        public List<PortalInformation> PortalInformation = new List<PortalInformation>();

        /// <summary>
        /// WMO Portal Relations. (MOPR)
        /// </summary>
        public List<PortalRelation> PortalRelations = new List<PortalRelation>();

        /// <summary>
        /// WMO Light Information. (MOLT)
        /// </summary>
        public List<LightInformation> LightInformation = new List<LightInformation>();

        /// <summary>
        /// WMO Fog Information. (MFOG)
        /// </summary>
        public List<FogInformation> FogInformation = new List<FogInformation>();

        /// <summary>
        /// WMO Doodads Sets. (MODS)
        /// </summary>
        public List<DoodadSet> DoodadSets = new List<DoodadSet>();

        /// <summary>
        /// WMO DoodadsFileNames. (MODN)
        /// </summary>
        public Dictionary<int, string> DoodadFiles = new Dictionary<int, string>();

        /// <summary>
        /// WMO DoodadsDefinitions. (MODD)
        /// </summary>
        public List<DoodadDefinition> DoodadDefinitions = new List<DoodadDefinition>();

        /// <summary>
        /// WMO Groups. (MOGP)
        /// </summary>
        public List<WMOGroup> Groups = new List<WMOGroup>();

        /// <summary>
        /// List of indices used for rendering this WMO World Space. (Internal)
        /// </summary>
        public List<int> WmoIndices = new List<int>();

        /// <summary>
        /// List of indices used for rendering MDX residing in this WMO World Space. (Internal)
        /// </summary>
        public List<int> WmoMDXIndices = new List<int>();

        /// <summary>
        /// List of indices used for rendering liquids in this WMO. (Internal)
        /// </summary>
        public List<int> WmoLiquidIndices = new List<int>();

        /// <summary>
        /// List of vertices used for rendering this WMO. (Internal)
        /// </summary>
        public List<Vector3> WmoVertices = new List<Vector3>();

        /// <summary>
        /// List of vertices used for rendering MDX residing in this WMO. (Internal)
        /// </summary>
        public List<Vector3> WmoMDXVertices = new List<Vector3>();

        /// <summary>
        /// List of vertices used for rendering Liquids residing in this WMO. (Internal)
        /// </summary>
        public List<Vector3> WmoLiquidVertices = new List<Vector3>();

        /// <summary>
        /// The MDX models that decorate this WMO.
        /// </summary>
        public MDX[] WMOMDXs;

        /// <summary>
        /// Used to read file tokens and validate chunks.
        /// </summary>
        private DataChunkHeader DataChunkHeader = new DataChunkHeader();

        public WMO(string filePath, MapObjectDefinition objectDefinition, ADT adt) : base(new MemoryStream(File.ReadAllBytes(filePath)))
        {
            // Version
            if (!ReadMVER())
                return;

            // Rather than all chunks being top level, they have been wrapped in MOMO.
            if (!ReadMOMO())
                return;

            // Header for the map header. 64 bytes.
            if (!ReadMOHD())
                return;

            // List of textures (BLP Files) used in this map object.
            if (!ReadMOTX())
                return;

            // Materials used in this map object, 64 bytes per texture (BLP file).
            if (!ReadMOMT())
                return;

            // List of group names for the groups in this map object. 
            if (!ReadMOGN())
                return;

            // Group information for WMO groups, 32 bytes per group, nGroups entries.
            if (!ReadMOGI())
                return;

            // Portal vertices, one entry is a float[3], usually 4 * 3 * float per portal.
            if (!ReadMOPV())
                return;

            // Portal information. 20 bytes per portal, nPortals entries.
            if (!ReadMOPT())
                return;

            // Map Object Portal References from groups. Mostly twice the number of portals.
            if (!ReadMOPR())
                return;

            // Lighting information. 48 bytes per light, nLights entries
            if (!ReadMOLT())
                return;

            // This chunk defines doodad sets. There are 32 bytes per doodad set
            if (!ReadMODS())
                return;

            // List of filenames for Mdx models that appear in this WMO.
            if (!ReadMODN())
                return;

            // Information for doodad instances. 40 bytes per doodad instance, nDoodads entries.
            if (!ReadMODD())
                return;

            // Fog information. Made up of blocks of 48 bytes.
            if (!ReadMFOG())
                return;

            // Optional, client moves pointer before checking for groups.
            if (!ReadMCVP())
                return;

            // WMO groups.
            if (!ReadMOGP())
                return;

            // Mesh related, internal.
            if (Configuration.ShouldParseWMOs)
            {
                TransformWMO(objectDefinition, adt);

                GC.Collect();
            }
        }

        /// <summary>
        /// Transform geometry to their in-game final placement.
        /// </summary>
        private void TransformWMO(MapObjectDefinition objectDefinition, ADT adt)
        {
            ClearCollision();

            var doodadSet = objectDefinition.DoodadSetId;
            // Initialize with default doodad set. 'Global'
            var setIndices = new List<int> { 0 };
            if (doodadSet > 0)
                setIndices.Add(doodadSet);

            var doodadSets = new List<DoodadSet>(setIndices.Count);
            foreach (var index in setIndices)
            {
                if (index < DoodadSets.Count)
                    doodadSets.Add(DoodadSets[index]);
                else
                    Logger.Error($"Invalid index {index} into DoodadSet array with id {doodadSet}");
            }

            if (Configuration.ShouldParseMDXs)
            {
                var mdxList = new List<MDX>();
                foreach (var def in doodadSets)
                {
                    var doodadSetOffset = def.FirstInstanceIndex;
                    var doodadSetCount = def.InstanceCount;
                    for (var i = doodadSetOffset; i < (doodadSetOffset + doodadSetCount); i++)
                    {
                        var curDoodadDef = DoodadDefinitions[(int)i];
                        // Check if model exists in extracted mdx models.
                        if (!curDoodadDef.Exists)
                        {
                            Logger.Warning($"MDX {Path.GetFileName(curDoodadDef.FilePath)} does not exists in client data.");
                            continue;
                        }

                        // Avoid loading MDX models that we already previously parsed and had no default collision.
                        if (!CacheManager.ShouldLoad(curDoodadDef.FilePath))
                        {
                            if (Globals.Verbose)
                                Logger.Warning($"Skip MDX model {Path.GetFileName(curDoodadDef.FilePath)}, it as not collision.");
                            continue;
                        }

                        // Create a new mdx obj.
                        var curMDX = new MDX(curDoodadDef.FilePath, true);
                        if (!curMDX.HasCollision)
                        {
                            curMDX?.Dispose();
                            if (Globals.Verbose)
                                Logger.Warning($"Skipping MDX {Path.GetFileName(curDoodadDef.FilePath)} it has no default collision.");
                            continue;
                        }

                        var tempIndices = new List<int>();
                        for (var j = 0; j < curMDX.GeometryHolder.BoundingTriangles.Length; j++)
                        {
                            var tri = curMDX.GeometryHolder.BoundingTriangles[j];

                            tempIndices.Add(tri.Index2);
                            tempIndices.Add(tri.Index1);
                            tempIndices.Add(tri.Index0);
                        }

                        // Rotate this model into its new orientation.
                        curMDX.TransformWMOMDX(tempIndices, curDoodadDef);
                        mdxList.Add(curMDX);
                    }
                }
                WMOMDXs = mdxList.ToArray();
            }

            // Rotate the WMO's to the new orientation
            var position = objectDefinition.Position;
            var posX = (position.x - Constants.CenterPoint) * -1;
            var posY = (position.y - Constants.CenterPoint) * -1;
            var originVec = StorageRoom.PopVector3(posX, posY, position.z);
            var rotateZ = Matrix.CreateRotationZ((objectDefinition.OrientationB + 180) * MathUtil.RadiansPerDegree);
            int offset;
            foreach (var wmoGroup in Groups)
            {
                using (wmoGroup)
                {
                    var usedTriangles = new HashSet<Index3>();
                    var uniqueWmoTriangles = new List<Index3>();

                    foreach (var node in wmoGroup.BSPNodes)
                    {
                        foreach (var triangle in node.TriangleIndices)
                        {
                            if (!usedTriangles.Contains(triangle))
                            {
                                usedTriangles.Add(triangle);
                                uniqueWmoTriangles.Add(triangle);
                            }
                        }
                    }

                    var newIndices = new Dictionary<int, int>();
                    foreach (var triangle in uniqueWmoTriangles)
                    {
                        // Add all vertices, uniquely.
                        if (!newIndices.TryGetValue(triangle.Index0, out int newIndex))
                        {
                            newIndex = WmoVertices.Count;
                            newIndices.Add(triangle.Index0, newIndex);

                            var basePosVec = wmoGroup.Vertices[triangle.Index0];
                            var rotatedPosVec = Vector3.Transform(basePosVec, rotateZ);
                            var finalPosVec = rotatedPosVec + originVec;
                            WmoVertices.Add(finalPosVec);
                        }
                        WmoIndices.Add(newIndex);

                        if (!newIndices.TryGetValue(triangle.Index1, out newIndex))
                        {
                            newIndex = WmoVertices.Count;
                            newIndices.Add(triangle.Index1, newIndex);

                            var basePosVec = wmoGroup.Vertices[triangle.Index1];
                            var rotatedPosVec = Vector3.Transform(basePosVec, rotateZ);
                            var finalPosVec = rotatedPosVec + originVec;
                            WmoVertices.Add(finalPosVec);
                        }
                        WmoIndices.Add(newIndex);

                        if (!newIndices.TryGetValue(triangle.Index2, out newIndex))
                        {
                            newIndex = WmoVertices.Count;
                            newIndices.Add(triangle.Index2, newIndex);

                            var basePosVec = wmoGroup.Vertices[triangle.Index2];
                            var rotatedPosVec = Vector3.Transform(basePosVec, rotateZ);
                            var finalPosVec = rotatedPosVec + originVec;
                            WmoVertices.Add(finalPosVec);
                        }
                        WmoIndices.Add(newIndex);

                        // Liquids
                        if (wmoGroup.LiquidInformation.Count > 0)
                        {
                            var tileSize = Constants.UnitSize;
                            var liquidChunk = wmoGroup.LiquidInformation[0];
                            var baseVector = Vector3.Transform(liquidChunk.BaseCoordinates, rotateZ) + originVec;

                            for (var y = 0; y < liquidChunk.YTileCount; y++) // Height
                            {
                                for (var x = 0; x < liquidChunk.XTileCount; x++) // Width
                                {
                                    var flag = liquidChunk.LiquidTileFlags[y, x];
                                    if (flag == 15) // Do not render.
                                        continue;

                                    var v1 = StorageRoom.PopVector3(baseVector.X + (tileSize * (x + 0)), baseVector.Y + (-1 * tileSize * (y + 0)), baseVector.Z);
                                    var v2 = StorageRoom.PopVector3(baseVector.X + (tileSize * (x + 1)), baseVector.Y + (-1 * tileSize * (y + 0)), baseVector.Z);
                                    var v3 = StorageRoom.PopVector3(baseVector.X + (tileSize * (x + 0)), baseVector.Y + (-1 * tileSize * (y + 1)), baseVector.Z);
                                    var v4 = StorageRoom.PopVector3(baseVector.X + (tileSize * (x + 1)), baseVector.Y + (-1 * tileSize * (y + 1)), baseVector.Z);

                                    //v1 = Vector3.Transform(v1, rotateZ) + originVec;
                                    //v2 = Vector3.Transform(v2, rotateZ) + originVec;
                                    //v3 = Vector3.Transform(v3, rotateZ) + originVec;
                                    //v4 = Vector3.Transform(v4, rotateZ) + originVec;

                                    var port = $".port {v1.X} {v1.Y} {v1.Z} 0";
                                    if (!DataGenerator.Ports.Contains(port))
                                        DataGenerator.Ports.Add(port);
                                    else
                                        continue;
                                
                                    WmoLiquidVertices.Add(v1);
                                    WmoLiquidVertices.Add(v2);
                                    WmoLiquidVertices.Add(v3);
                                    WmoLiquidVertices.Add(v4);

                                    //WmoLiquidIndices.Add(WmoLiquidVertices.Count - 4);
                                    //WmoLiquidIndices.Add(WmoLiquidVertices.Count - 3);
                                    //WmoLiquidIndices.Add(WmoLiquidVertices.Count - 2);
                                    //WmoLiquidIndices.Add(WmoLiquidVertices.Count - 2);
                                    //WmoLiquidIndices.Add(WmoLiquidVertices.Count - 3);
                                    //WmoLiquidIndices.Add(WmoLiquidVertices.Count - 1);

                                    foreach (var v in WmoLiquidVertices)
                                    {
                                        port = $".port {v.X} {v.Y} {v.Z} 0";
                                        var adt_x = (int)(32.0 - (v.X / Constants.TileSizeYrds));
                                        var adt_y = (int)(32.0 - (v.Y / Constants.TileSizeYrds));
                                        var tile_x = (int)(127 * (32.0 - (v.X / Constants.TileSizeYrds) - (adt_x)));
                                        var tile_y = (int)(127 * (32.0 - (v.Y / Constants.TileSizeYrds) - (adt_y)));

                                        if (!DataGenerator.GlobalWMOLiquids.ContainsKey(adt_x))
                                            DataGenerator.GlobalWMOLiquids.Add(adt_x, new Dictionary<int, float[,]>());
                                        if (!DataGenerator.GlobalWMOLiquids[adt_x].ContainsKey(adt_y))
                                            DataGenerator.GlobalWMOLiquids[adt_x].Add(adt_y, new float[128, 128]);

                                        DataGenerator.GlobalWMOLiquids[adt_x][adt_y][tile_y, tile_x] = v.Z;

                                        //Debug.WriteLine($"{tile_x},{tile_y}");
                                        //Debug.WriteLine(port);
                                    }

                                    WmoLiquidVertices.Clear();

                                }
                            }
                        }
                    }

                    // Rotate the MDX's to the new orientation
                    if (WMOMDXs != null)
                    {
                        foreach (var currentMDX in WMOMDXs)
                        {
                            offset = WmoMDXVertices.Count;
                            for (var i = 0; i < currentMDX.Vertices.Count; i++)
                            {
                                var basePosition = currentMDX.Vertices[i];
                                var rotatedPosition = Vector3.Transform(basePosition, rotateZ);
                                var finalPosition = rotatedPosition + originVec;

                                WmoMDXVertices.Add(finalPosition);
                            }

                            foreach (var index in currentMDX.Indices)
                                WmoMDXIndices.Add(index + offset);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Flush WMO calculated Verts and Indexes.
        /// </summary>
        private void ClearCollision()
        {
            foreach (Vector3 vector in WmoVertices)
                StorageRoom.PushVector3(vector);
            WmoVertices.Clear();
            WmoIndices.Clear();
        }

        /// <summary>
        /// MVER, Version.
        /// </summary>
        private bool ReadMVER()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MVER)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MVER]"}");

                Version = this.ReadUInt32();

                if (Globals.Verbose)
                    Logger.Success($"[MVER]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// We don't do anything with MOMO, a MOHD (Header) will follow.
        /// </summary>
        private bool ReadMOMO()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);

                if (DataChunkHeader.Token != Tokens.MOMO)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOMO]"}");

                if (Globals.Verbose)
                    Logger.Success($"[MOMO]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Header
        /// </summary>
        private bool ReadMOHD()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);

                if (DataChunkHeader.Token != Tokens.MOHD)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOHD]"}");

                var byteChunk = this.ReadBytes(DataChunkHeader.Size);
                Header = new SMOHeader(byteChunk);

                if (Globals.Verbose)
                    Logger.Success($"[MOHD]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Textures used.
        /// </summary>
        private bool ReadMOTX()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MOTX)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOTX]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                {
                    if (this.PeekChar() == 0)
                        this.BaseStream.Position++;
                    else
                        Textures.Add((int)(DataChunkHeader.Size - (endPos - this.BaseStream.Position)), this.ReadCString());
                }

                if (Textures.Count != Header.TextureCount)
                    throw new Exception("Invalid Texture count!");

                if (Globals.Verbose)
                    Logger.Success($"[MOTX]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Materials used.
        /// </summary>
        private bool ReadMOMT()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MOMT)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOMT]"}");

                var chunk = this.ReadBytes(DataChunkHeader.Size);
                using (MemoryStream ms = new MemoryStream(chunk))
                using (BinaryReaderProgress br = new BinaryReaderProgress(ms))
                    while (br.BaseStream.Position < chunk.Length)
                        Materials.Add(new Material(br));

                if (Globals.Verbose)
                    Logger.Success($"[MOMT]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// WMO GroupNames
        /// There are not always nGroups entries in this chunk as it contains extra empty strings and descriptive names.
        /// </summary>
        private bool ReadMOGN()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MOGN)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOGN]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                {
                    if (this.PeekChar() == 0)
                        this.BaseStream.Position++;
                    else
                        GroupNames.Add((int)(DataChunkHeader.Size - (endPos - this.BaseStream.Position)), this.ReadCString());
                }

                Header.GroupNamesCount = (uint)GroupNames.Keys.Count;

                if (Globals.Verbose)
                    Logger.Success($"[MOGN]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Groups information.
        /// </summary>
        private bool ReadMOGI()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MOGI)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOGI]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                    GroupInformation.Add(new GroupInformation(this));

                if (GroupInformation.Count != Header.GroupCount)
                    throw new Exception("Invalid WMO group names count!");

                if (Globals.Verbose)
                    Logger.Success($"[MOGI]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Portal verts.
        /// </summary>
        private bool ReadMOPV()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MOPV)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOPV]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                    PortalVertices.Add(new C3Vector(this));

                if (Globals.Verbose)
                    Logger.Success($"[MOPV]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Portals information.
        /// </summary>
        private bool ReadMOPT()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MOPT)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOPT]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                    PortalInformation.Add(new PortalInformation(this));

                if (Globals.Verbose)
                    Logger.Success($"[MOPT]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Portal relations.
        /// </summary>
        private bool ReadMOPR()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MOPR)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOPR]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                    PortalRelations.Add(new PortalRelation(this));

                if (Globals.Verbose)
                    Logger.Success($"[MOPR]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Lights information.
        /// </summary>
        private bool ReadMOLT()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MOLT)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOLT]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                    LightInformation.Add(new LightInformation(this));

                if (Globals.Verbose)
                    Logger.Success($"[MOLT]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// DoodadSets (MDX)
        /// </summary>
        private bool ReadMODS()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MODS)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MODS]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                    DoodadSets.Add(new DoodadSet(this));

                if (Globals.Verbose)
                    Logger.Success($"[MODS]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Doodads Filenames. (MDX)
        /// </summary>
        private bool ReadMODN()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MODN)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MODN]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                {
                    if (this.PeekChar() == 0)
                        this.BaseStream.Position++;
                    else
                        DoodadFiles.Add((int)(DataChunkHeader.Size - (endPos - this.BaseStream.Position)), this.ReadCString().ToLower());
                }

                if (Globals.Verbose)
                    Logger.Success($"[MODN]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// DoodadsDefinitions. (MDX)
        /// </summary>
        /// <returns></returns>
        private bool ReadMODD()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MODD)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MODD]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                    DoodadDefinitions.Add(new DoodadDefinition(this));

                if (Globals.Verbose)
                    Logger.Success($"[MODD]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Fog information.
        /// </summary>
        private bool ReadMFOG()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MFOG)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MFOG]"}");

                long endPos = this.BaseStream.Position + DataChunkHeader.Size;
                while (this.BaseStream.Position < endPos)
                    FogInformation.Add(new FogInformation(this));

                if (Globals.Verbose)
                    Logger.Success($"[MFOG]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// WMO Groups.
        /// </summary>
        private bool ReadMOGP()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                // Parse all available WMOGroups.
                for (int i = 0; i < Header.GroupCount; i++)
                {
                    DataChunkHeader.Fill(this);
                    if (DataChunkHeader.Token != Tokens.MOGP)
                        throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MOGP]"}");

                    Groups.Add(new WMOGroup(this));
                }

                if (Globals.Verbose)
                    Logger.Success($"[MOGP]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }


        /// <summary>
        /// Optional, Convex Volume Planes.
        /// </summary>
        private bool ReadMCVP()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token == Tokens.MCVP)
                    this.ReadBytes(DataChunkHeader.Size);
                else
                    this.BaseStream.Position -= 8;

                if (Globals.Verbose)
                    Logger.Success($"[MCVP]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            Header?.Dispose();
            Textures?.Clear();
            Textures = null;
            Materials?.Clear();
            Materials = null;
            GroupNames?.Clear();
            GroupNames = null;
            GroupInformation?.Clear();
            GroupInformation = null;
            PortalVertices?.Clear();
            PortalVertices = null;
            PortalInformation?.Clear();
            PortalInformation = null;
            PortalRelations?.Clear();
            PortalRelations = null;
            LightInformation?.Clear();
            LightInformation = null;
            FogInformation?.Clear();
            FogInformation = null;
            DoodadSets?.Clear();
            DoodadSets = null;
            DoodadFiles?.Clear();
            DoodadFiles = null;
            DoodadDefinitions?.Clear();
            DoodadDefinitions = null;
            if (Groups != null)
            {
                foreach (var group in Groups)
                    group.Dispose();
                Groups?.Clear();
                Groups = null;
            }
            WmoIndices?.Clear();
            WmoIndices = null;
            WmoMDXIndices?.Clear();
            WmoMDXIndices = null;
            WmoLiquidIndices?.Clear();
            WmoLiquidIndices = null;
            if (WmoVertices != null)
            {
                foreach (Vector3 v3 in WmoVertices)
                    StorageRoom.PushVector3(v3);
                WmoVertices?.Clear();
                WmoVertices = null;
            }
            if (WmoMDXVertices != null)
            {
                foreach (Vector3 v3 in WmoMDXVertices)
                    StorageRoom.PushVector3(v3);
                WmoMDXVertices?.Clear();
                WmoMDXVertices = null;
            }
            if (WmoLiquidVertices != null)
            {
                foreach (Vector3 v3 in WmoLiquidVertices)
                    StorageRoom.PushVector3(v3);
                WmoLiquidVertices?.Clear();
                WmoLiquidVertices = null;
            }
            WMOMDXs = null;
            DataChunkHeader = null;

            base.Dispose(disposing);
        }
    }
}
