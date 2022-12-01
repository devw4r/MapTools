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

namespace AlphaCoreExtractor.Generators
{
    public class DataGenerator
    {
        [DllImport("/recast/AlphaCoreRecast.dll", EntryPoint = "ExtractNav", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool ExtractNav(string filePath, string outputNav);

        private static readonly bool[,] EmptyHolesArray = new bool[4, 4];

        public static void GenerateData(WDT wdt, out int generatedMaps, out int generatedNavs, out int generatedObjs)
        {
            generatedMaps = 0;
            generatedNavs = 0;
            generatedObjs = 0;
            var mapID = wdt.MapID.ToString("000");

            // TODO
            if (wdt.IsWMOBased)
            {
                Logger.Info($"Skipping nav generation for Map {wdt.Name}...");
                return;
            }
           
            try
            {
                uint total_tiles = Convert.ToUInt32(Constants.TileBlockSize * Constants.TileBlockSize);
                int processed_tiles = 0;
                Logger.Notice($"Generating files for Map {wdt.DBCMap.MapName_enUS}");

                for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
                {
                    for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
                    {
                        if (wdt.TileBlocks[tileBlockX, tileBlockY] is ADT adt)
                        {
                            using (adt)
                            {
                                var blockX = tileBlockX.ToString("00");
                                var blockY = tileBlockY.ToString("00");
                                var outputMapFileName = $@"{Paths.OutputMapsPath}{mapID}{blockX}{blockY}.map";
                                var outputObjFileName = $@"{Paths.OutputGeomPath}{mapID}{blockX}{blockY}.obj";

                                if (Configuration.GenerateMaps == GenerateMaps.Enabled)
                                {
                                    GenerateMapFiles(wdt, adt, outputMapFileName);
                                    generatedMaps++;
                                }

                                if(Configuration.GenerateObjs == GenerateObjs.Enabled || Configuration.GenerateNavs == GenerateNavs.Enabled)
                                {
                                    GenerateMeshFiles(wdt, adt, outputObjFileName);
                                    if(Configuration.GenerateNavs == GenerateNavs.Enabled)
                                        generatedNavs++;
                                    if (Configuration.GenerateObjs == GenerateObjs.Enabled)
                                        generatedObjs++;
                                }
                            }
                            GC.Collect();
                        }

                        Logger.Progress("Generating files", ++processed_tiles, total_tiles, 200);
                    }
                }

                if (generatedMaps == 0)
                    Logger.Warning($"No tile data for Map {wdt.DBCMap.MapName_enUS}");
                else
                    Logger.Success($"Generated {generatedMaps} .map files for Map {wdt.DBCMap.MapName_enUS}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        #region NavFiles
        private static int HeightsPerTileSide = Constants.CellSize * Constants.TileSize;
        private static int[,] TileVertLocations = new int[HeightsPerTileSide + 1, HeightsPerTileSide + 1];
        private static bool[,] TileHolesMap = new bool[HeightsPerTileSide + 1, HeightsPerTileSide + 1];
        private static List<Vector3> TileVerts = new List<Vector3>();
        private static void GenerateMeshFiles(WDT wdt, ADT adt, string outputObjFileName)
        {
            try
            {
                if (File.Exists(outputObjFileName))
                {
                    try { File.Delete(outputObjFileName); }
                    catch (Exception ex) { Logger.Error(ex.Message); return; }
                }

                for (var i = 0; i < (HeightsPerTileSide + 1); i++)
                    for (var j = 0; j < (HeightsPerTileSide + 1); j++)
                        TileVertLocations[i, j] = -1;

                for (var x = 0; x < Constants.TileSize; x++)
                {
                    for (var y = 0; y < Constants.TileSize; y++)
                    {
                        var tileChunk = adt.Tiles[x, y];
                        var heights = tileChunk.MCVTSubChunk.GetLowResMapMatrix();
                        var holes = (tileChunk.holes_low_mask > 0) ? tileChunk.HolesMap : EmptyHolesArray;

                        // Add the height map values, inserting them into their correct positions.
                        for (var unitX = 0; unitX <= Constants.CellSize; unitX++)
                        {
                            for (var unitY = 0; unitY <= Constants.CellSize; unitY++)
                            {
                                var tileX = (x * Constants.CellSize) + unitX;
                                var tileY = (y * Constants.CellSize) + unitY;

                                var vertIndex = TileVertLocations[tileX, tileY];
                                if (vertIndex == -1)
                                {
                                    var xPos = Constants.CenterPoint
                                               - (adt.TileX * Constants.TileSizeYrds)
                                               - (tileX * Constants.UnitSize);
                                    var yPos = Constants.CenterPoint
                                               - (adt.TileY * Constants.TileSizeYrds)
                                               - (tileY * Constants.UnitSize);
                                    var zPos = heights[unitX, unitY]; // Absolute height in Alpha.

                                    TileVertLocations[tileX, tileY] = TileVerts.Count;
                                    TileVerts.Add(StorageRoom.PopVector3(xPos, yPos, zPos));
                                }

                                if (unitY == Constants.CellSize || unitX == Constants.CellSize)
                                    continue;

                                TileHolesMap[tileX, tileY] = holes[unitX / 2, unitY / 2];
                            }
                        }
                    }
                }

                var tileIndices = new List<int>();
                for (var tileX = 0; tileX < HeightsPerTileSide; tileX++)
                {
                    for (var tileY = 0; tileY < HeightsPerTileSide; tileY++)
                    {
                        if (TileHolesMap[tileX, tileY])
                            continue;

                        // Top triangle.
                        var vertId0 = TileVertLocations[tileX, tileY];
                        var vertId1 = TileVertLocations[tileX, tileY + 1];
                        var vertId9 = TileVertLocations[tileX + 1, tileY];
                        tileIndices.Add(vertId0);
                        tileIndices.Add(vertId1);
                        tileIndices.Add(vertId9);

                        // Bottom triangle.
                        var vertId10 = TileVertLocations[tileX + 1, tileY + 1];
                        tileIndices.Add(vertId1);
                        tileIndices.Add(vertId10);
                        tileIndices.Add(vertId9);
                    }
                }

                if (adt.WMOs.Count > 0 || adt.MDXs.Count > 0)
                    AppendWMOsAndMDX(ref TileVerts, ref tileIndices, adt);
                if (TileVerts.Count > 0)
                    ExportMesh(outputObjFileName, TileVerts, tileIndices);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                Array.Clear(TileVertLocations, 0, TileVertLocations.Length);
                Array.Clear(TileHolesMap, 0, TileHolesMap.Length);
                foreach (Vector3 vector in TileVerts)
                    StorageRoom.PushVector3(vector);
                TileVerts.Clear();
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
            using (var file = new StreamWriter(fileName))
            {
                // Write verts.
                foreach (var vertex in vertices)
                {
                    vertex.ToRecast();
                    file.WriteLine("v {0} {1} {2}", vertex.X, vertex.Y, vertex.Z);
                }

                // Write faces.
                for (var i = 0; i < indices.Count; i += 3)
                    file.WriteLine("f {0} {1} {2}", indices[i + 2] + 1, indices[i + 1] + 1, indices[i] + 1);
            }

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

        #region MapFiles
        private static void GenerateMapFiles(WDT wdt, ADT adt, string outputMapFileName)
        {
            if (File.Exists(outputMapFileName))
            {
                try { File.Delete(outputMapFileName); }
                catch (Exception ex) { Logger.Error(ex.Message); return; }
            }

            using (FileStream fileStream = new FileStream(outputMapFileName, FileMode.Create))
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

        private static bool[,] liquid_show = new bool[(int)Constants.GridSize, (int)Constants.GridSize];
        private static float[,] liquid_height = new float[(int)Constants.GridSize + 1, (int)Constants.GridSize + 1];
        private static sbyte[,] liquid_flag = new sbyte[(int)Constants.GridSize + 1, (int)Constants.GridSize + 1];
        private static void WriteLiquids(BinaryWriter binaryWriter, ADT adt)
        {
            Array.Clear(liquid_show, 0, liquid_show.Length);
            Array.Clear(liquid_height, 0, liquid_height.Length);
            Array.Clear(liquid_flag, 0, liquid_flag.Length);

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
                                liquid_show[cy, cx] = true;

                                // Overwrite DEEP water flag.
                                if ((liquid.Flags[y, x] & (1 << 7)) != 0)
                                    liquid.Flag = SMChunkFlags.FLAG_LQ_DEEP;

                                liquid_height[cy, cx] = liquid.GetHeight(y, x);
                                liquid_flag[cy, cx] = (sbyte)liquid.Flag;
                            }
                            else
                            {
                                liquid_show[cy, cx] = false;
                            }
                        }
                    }
                }
            }

            for (int y = 0; y < Constants.GridSize; y++)
            {
                for (int x = 0; x < Constants.GridSize; x++)
                {
                    if (liquid_show[y, x])
                    {
                        binaryWriter.Write(liquid_flag[y, x]);
                        binaryWriter.Write(liquid_height[y, x]);
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
