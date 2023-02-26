// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using AlphaCoreExtractor.DBC;
using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Mesh;
using AlphaCoreExtractor.Core.Cache;
using AlphaCoreExtractor.Core.Chunks;
using AlphaCoreExtractor.Core.Terrain;
using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.DBC.Structures;
using AlphaCoreExtractor.Core.Structures;
using System.Text;
using System.Runtime.InteropServices.ComTypes;

namespace AlphaCoreExtractor.Generators
{
    public class DataGenerator
    {
        [DllImport("/recast/AlphaCoreRecast.dll", EntryPoint = "ExtractNav", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool ExtractNav(string filePath, string outputNav);

        public static uint GeneratedMaps = 0;
        public static uint GeneratedObjs = 0;
        public static uint GeneratedNavs = 0;

        public static void ResetCount()
        {
            GeneratedMaps = 0;
            GeneratedObjs = 0;
            GeneratedNavs = 0;
        }

        #region NavFiles
        private static readonly int HeightsPerTileSide = Constants.CellSize * Constants.TileSize;
        private static void GenerateMeshFiles(WDT wdt, ADT adt)
        {
            int[,] tileVertLocations = new int[HeightsPerTileSide + 1, HeightsPerTileSide + 1];
            bool[,] tileHolesMap = new bool[HeightsPerTileSide + 1, HeightsPerTileSide + 1];
            List<Vector3> tileVerts = new List<Vector3>();

            try
            {
                var outFileName = $@"{Paths.OutputGeomPath}{wdt.MapID:000}{adt.TileX:00}{adt.TileY:00}.obj";

                if (File.Exists(outFileName))
                {
                    try { File.Delete(outFileName); }
                    catch (Exception ex) { Logger.Error(ex.Message); return; }
                }

                for (var i = 0; i < (HeightsPerTileSide + 1); i++)
                    for (var j = 0; j < (HeightsPerTileSide + 1); j++)
                        tileVertLocations[i, j] = -1;

                for (var x = 0; x < Constants.TileSize; x++)
                {
                    for (var y = 0; y < Constants.TileSize; y++)
                    {
                        var tileChunk = adt.Tiles[x, y];
                        var heights = tileChunk.MCVTSubChunk.GetLowResMapMatrix();
                        var holes = (tileChunk.holes_low_mask > 0) ? tileChunk.HolesMap : emptyHolesArray;

                        // Add the height map values, inserting them into their correct positions.
                        for (var unitX = 0; unitX <= Constants.CellSize; unitX++)
                        {
                            for (var unitY = 0; unitY <= Constants.CellSize; unitY++)
                            {
                                var tileX = (x * Constants.CellSize) + unitX;
                                var tileY = (y * Constants.CellSize) + unitY;

                                var vertIndex = tileVertLocations[tileX, tileY];
                                if (vertIndex == -1)
                                {
                                    var xPos = Constants.CenterPoint
                                               - (adt.TileX * Constants.TileSizeYrds)
                                               - (tileX * Constants.UnitSize);
                                    var yPos = Constants.CenterPoint
                                               - (adt.TileY * Constants.TileSizeYrds)
                                               - (tileY * Constants.UnitSize);
                                    var zPos = heights[unitX, unitY]; // Absolute height in Alpha.

                                    tileVertLocations[tileX, tileY] = tileVerts.Count;
                                    tileVerts.Add(StorageRoom.PopVector3(xPos, yPos, zPos));
                                }

                                if (unitY == Constants.CellSize || unitX == Constants.CellSize)
                                    continue;

                                tileHolesMap[tileX, tileY] = holes[unitX / 2, unitY / 2];
                            }
                        }
                    }
                }

                var tileIndices = new List<int>();
                for (var tileX = 0; tileX < HeightsPerTileSide; tileX++)
                {
                    for (var tileY = 0; tileY < HeightsPerTileSide; tileY++)
                    {
                        if (tileHolesMap[tileX, tileY])
                            continue;

                        // Top triangle.
                        var vertId0 = tileVertLocations[tileX, tileY];
                        var vertId1 = tileVertLocations[tileX, tileY + 1];
                        var vertId9 = tileVertLocations[tileX + 1, tileY];
                        tileIndices.Add(vertId0);
                        tileIndices.Add(vertId1);
                        tileIndices.Add(vertId9);

                        // Bottom triangle.
                        var vertId10 = tileVertLocations[tileX + 1, tileY + 1];
                        tileIndices.Add(vertId1);
                        tileIndices.Add(vertId10);
                        tileIndices.Add(vertId9);
                    }
                }

                if (adt.WMOs.Count > 0 || adt.MDXs.Count > 0)
                    AppendWMOsAndMDX(ref tileVerts, ref tileIndices, adt);
                if (tileVerts.Count > 0)
                    ExportMesh(outFileName, tileVerts, tileIndices);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                Array.Clear(tileVertLocations, 0, tileVertLocations.Length);
                Array.Clear(tileHolesMap, 0, tileHolesMap.Length);
                foreach (Vector3 vector in tileVerts)
                    StorageRoom.PushVector3(vector);
                tileVerts.Clear();
            }
        }

        private static void AppendWMOsAndMDX(ref List<Vector3> verts, ref List<int> indices, ADT adt)
        {
            if (verts == null)
                verts = new List<Vector3>();

            if (indices == null)
                indices = new List<int>();

            using (var clipper = new MeshClipper(adt.Bounds))
            {
                List<Vector3> newVertices;
                List<int> newIndices;

                // WMO Vertices
                int offset;
                foreach (var wmo in adt.WMOs)
                {
                    using (wmo)
                    {
                        clipper.ClipMesh(wmo.WmoVertices, wmo.WmoIndices, out newVertices, out newIndices);

                        offset = verts.Count;
                        if (newVertices.Count > 0 && newIndices.Count > 0)
                        {
                            verts.AddRange(newVertices);
                            foreach (var index in newIndices)
                                indices.Add(offset + index);
                        }

                        // MDXs living inside this WMO.
                        if (Configuration.ShouldParseMDXs)
                        {
                            clipper.ClipMesh(wmo.WmoMDXVertices, wmo.WmoMDXIndices, out newVertices, out newIndices);
                            offset = verts.Count;

                            if (newVertices.Count > 0 && newIndices.Count > 0)
                            {
                                verts.AddRange(newVertices);
                                foreach (var index in newIndices)
                                    indices.Add(offset + index);
                            }
                        }

                        // Liquids.
                        //clipper.ClipMesh(wmo.WmoLiquidVertices, wmo.WmoLiquidIndices, out newVertices, out newIndices);
                        //offset = verts.Count;
                        //if (newVertices.Count > 0 && newIndices.Count > 0)
                        //{
                        //    verts.AddRange(newVertices);
                        //    foreach (var index in newIndices)
                        //        indices.Add(offset + index);
                        //}
                    }
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
                }

                // MDX living in the ADT chunk.
                if (Configuration.ShouldParseMDXs)
                {
                    foreach (var mdx in adt.MDXs)
                    {
                        clipper.ClipMesh(mdx.Vertices, mdx.Indices, out newVertices, out newIndices);
                        offset = verts.Count;

                        if (newVertices.Count > 0 && newIndices.Count > 0)
                        {
                            verts.AddRange(newVertices);
                            foreach (var index in newIndices)
                                indices.Add(offset + index);
                        }
                    }
                }
            }

            GC.Collect();
        }

        private static void ExportMesh(string fileName, List<Vector3> vertices, List<int> indices)
        {
            StringBuilder data = new StringBuilder();

            // Verts.
            foreach (var vertex in vertices)
            {
                vertex.ToRecast();
                data.AppendLine($"v {vertex.X} {vertex.Y} {vertex.Z}");
            }

            // Faces.
            for (var i = 0; i < indices.Count; i += 3)
                data.AppendLine($"f {indices[i + 2] + 1} {indices[i + 1] + 1} {indices[i] + 1}");

            using (var file = new StreamWriter(fileName))
                file.Write(data);

            // Create .nav files if requested.
            if (Configuration.GenerateNavs == GenerateNavs.Enabled)
            {
                var navFile = Paths.Transform(fileName.Replace(".obj", ".nav"));
                var name = Path.GetFileName(navFile);
                var destination = Paths.Transform(Paths.Combine(Paths.OutputNavPath, name));

                try
                {
                    if (File.Exists(destination))
                        File.Delete(destination);

                    if (Configuration.IsLinux)
                    {
                        Process extractProc = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = Paths.LinuxExtractorPath,
                                Arguments = $"{fileName} {destination}",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden
                            },
                        };

                        using (extractProc)
                        {
                            extractProc.Start();
                            extractProc.WaitForExit();
                            RecastExtractorErrors result = (RecastExtractorErrors)extractProc.ExitCode;
                            switch (result)
                            {
                                case RecastExtractorErrors.Success:
                                    Logger.Success($"Generated navigation data {Path.GetFileName(destination)} for geometry {Path.GetFileName(fileName)}");
                                    break;
                                case RecastExtractorErrors.MissingArgs:
                                    Logger.Error($"[RECAST] Missing arguments.");
                                    break;
                                case RecastExtractorErrors.LoadingGeomFailed:
                                case RecastExtractorErrors.LoadingGeomFailed2:
                                    Logger.Error($"[RECAST] Unable to process provided geometry.");
                                    break;
                                case RecastExtractorErrors.BuildFailed:
                                    Logger.Error($"[RECAST] Build Failed.");
                                    break;
                                default:
                                    Logger.Error($"[RECAST] Unknown error.");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (ExtractNav(fileName, destination))
                            Logger.Success($"Generated navigation data {Path.GetFileName(destination)} for geometry {Path.GetFileName(fileName)}");
                        else
                            Logger.Error($"[RECAST] Build Failed.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }
            }
        }
        #endregion

        public static void WriteADTFiles(WDT wdt, ADT adt)
        {
            if (Configuration.GenerateMaps == GenerateMaps.Enabled)
            {
                GenerateMapFiles(wdt, adt);
                GeneratedMaps++;
            }

            if (Configuration.GenerateObjs == GenerateObjs.Enabled || Configuration.GenerateNavs == GenerateNavs.Enabled)
            {
                GenerateMeshFiles(wdt, adt);
                if (Configuration.GenerateNavs == GenerateNavs.Enabled)
                    GeneratedNavs++;
                if (Configuration.GenerateObjs == GenerateObjs.Enabled)
                    GeneratedObjs++;
            }
        }

        #region MapFiles
        private static void GenerateMapFiles(WDT wdt, ADT adt)
        {
            var outFileName = $@"{Paths.OutputMapsPath}{wdt.MapID:000}{adt.TileX:00}{adt.TileY:00}.map";

            if (File.Exists(outFileName))
            {
                try { File.Delete(outFileName); }
                catch (Exception ex) { Logger.Error(ex.Message); return; }
            }

            using (FileStream fileStream = new FileStream(outFileName, FileMode.Create))
            {
                //Map version.
                fileStream.WriteMapVersion();
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    WriteHeightMap(binaryWriter, adt);
                    WriteAreaInformation(binaryWriter, adt, wdt);
                    WriteLiquids(binaryWriter, adt);
                }
            }
        }

        private static readonly bool[,] LiquidShow = new bool[(int)Constants.GridSize, (int)Constants.GridSize];
        private static readonly float[,] LiquidHeight = new float[(int)Constants.GridSize + 1, (int)Constants.GridSize + 1];
        private static readonly sbyte[,] LiquidFlag = new sbyte[(int)Constants.GridSize + 1, (int)Constants.GridSize + 1];
        private static readonly bool[,] emptyHolesArray = new bool[4, 4];
        private static void WriteLiquids(BinaryWriter binaryWriter, ADT adt)
        {
            Array.Clear(LiquidShow, 0, LiquidShow.Length);
            Array.Clear(LiquidHeight, 0, LiquidHeight.Length);
            Array.Clear(LiquidFlag, 0, LiquidFlag.Length);

            for (int i = 0; i < Constants.TileSize; i++)
            {
                for (int j = 0; j < Constants.TileSize; j++)
                {
                    var cell = adt.Tiles[i, j];

                    if (cell == null || cell.MCLQSubChunks.Count == 0)
                        continue;

                    MCLQSubChunk liquid;
                    if (cell.MCLQSubChunks.Count > 1)
                    {
                        // Prioritize river over ocean.
                        liquid = cell.MCLQSubChunks.First(mc => mc.Flag != SMChunkFlags.FLAG_LQ_OCEAN);
                    }
                    else
                        liquid = cell.MCLQSubChunks.First();

                    for (int y = 0; y < Constants.CellSize; y++)
                    {
                        int cy = i * Constants.CellSize + y;
                        for (int x = 0; x < Constants.CellSize; x++)
                        {
                            int cx = j * Constants.CellSize + x;
                            // Check if this liquid is rendered by the client.
                            if (liquid.Flags[y, x] != 0x0F)
                            {
                                LiquidShow[cy, cx] = true;

                                // Overwrite DEEP water flag.
                                if ((liquid.Flags[y, x] & (1 << 7)) != 0)
                                    liquid.Flag = SMChunkFlags.FLAG_LQ_DEEP;

                                LiquidHeight[cy, cx] = liquid.GetHeight(y, x);
                                LiquidFlag[cy, cx] = (sbyte)liquid.Flag;
                            }
                            else
                            {
                                LiquidShow[cy, cx] = false;
                            }
                        }
                    }
                }
            }

            for (int y = 0; y < Constants.GridSize; y++)
            {
                for (int x = 0; x < Constants.GridSize; x++)
                {
                    if (LiquidShow[y, x])
                    {
                        binaryWriter.Write(LiquidFlag[y, x]);
                        binaryWriter.Write(LiquidHeight[y, x]);
                    }
                    else
                        binaryWriter.Write((sbyte)-1);
                }
            }
        }

        private static void WriteAreaInformation(BinaryWriter binaryWriter, ADT adt, WDT terrain)
        {
            for (int cy = 0; cy < Constants.TileSize; cy++)
            {
                for (int cx = 0; cx < Constants.TileSize; cx++)
                {
                    var cell = adt.Tiles[cy, cx];
                    var areaNumber = cell.areaNumber;

                    if (terrain.DBCMap.ID < 2 && areaNumber < 4000000000 && DBCStorage.TryGetAreaByMapIdAndAreaNumber(terrain.DBCMap.ID, areaNumber, out AreaTable areaTable))
                    {
                        binaryWriter.Write((int)areaTable.ID);
                        binaryWriter.Write((uint)areaTable.AreaNumber);
                        binaryWriter.Write((byte)areaTable.Area_Flags);
                        binaryWriter.Write((byte)areaTable.Area_Level);
                        binaryWriter.Write((ushort)areaTable.Exploration_Bit);
                        binaryWriter.Write((byte)areaTable.FactionGroupMask);
                    }
                    else
                        binaryWriter.Write((int)-1);
                }
            }
        }

        private static void WriteHeightMap(BinaryWriter binaryWriter, ADT tileArea)
        {
            var liquidHeightfield = tileArea.LiquidsHeightmap;
            for (int cy = 0; cy < Configuration.ZResolution; cy++)
                for (int cx = 0; cx < Configuration.ZResolution; cx++)
                    binaryWriter.Write(liquidHeightfield.CalculateZ(cy, cx));
        }
        #endregion
    }
}
