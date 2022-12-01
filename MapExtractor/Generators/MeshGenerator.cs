//// TheAlphaProject
//// Discord: https://discord.gg/RzBMAKU
//// Github:  https://github.com/The-Alpha-Project

//using System;
//using System.IO;
//using System.Linq;
//using System.Diagnostics;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;

//using AlphaCoreExtractor.Log;
//using AlphaCoreExtractor.Helpers;
//using AlphaCoreExtractor.Core.Mesh;
//using AlphaCoreExtractor.Core.Terrain;
//using AlphaCoreExtractor.Helpers.Enums;
//using AlphaCoreExtractor.Core.Structures;

//namespace AlphaCoreExtractor.Generators
//{
//    public class MeshGenerator
//    {


        

//        public static void BuildMesh(WDT map, out int generatedMeshes)
//        {
//            generatedMeshes = 0;
//            var mapID = map.DBCMap.ID.ToString("000");

//            // TODO
//            if (map.IsWMOBased)
//            {
//                Logger.Info($"Skipping nav generation for Map {map.Name}...");
//                return;
//            }

//            for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
//            {
//                for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
//                {
//                    if (map.TileBlocks[tileBlockX, tileBlockY] is ADT adt)
//                    {
//                        using (adt)
//                        {
//                            var blockX = tileBlockX.ToString("00");
//                            var blockY = tileBlockY.ToString("00");
//                            var outputFileName = $@"{Paths.OutputGeomPath}{mapID}{blockX}{blockY}.obj";

//                            if (File.Exists(outputFileName))
//                            {
//                                try { File.Delete(outputFileName); }
//                                catch (Exception ex) { Logger.Error(ex.Message); return; }
//                            }

//                            int heightsPerTileSide = Constants.CellSize * Constants.TileSize;
//                            var tileVertLocations = new int[heightsPerTileSide + 1, heightsPerTileSide + 1];
//                            var tileHolesMap = new bool[heightsPerTileSide + 1, heightsPerTileSide + 1];
//                            var tileVerts = new List<Vector3>();

//                            for (var i = 0; i < (heightsPerTileSide + 1); i++)
//                                for (var j = 0; j < (heightsPerTileSide + 1); j++)
//                                    tileVertLocations[i, j] = -1;

//                            for (var x = 0; x < Constants.TileSize; x++)
//                            {
//                                for (var y = 0; y < Constants.TileSize; y++)
//                                {
//                                    var tileChunk = adt.Tiles[x, y];
//                                    var heights = tileChunk.MCVTSubChunk.GetLowResMapMatrix();
//                                    var holes = (tileChunk.holes_low_mask > 0) ? tileChunk.HolesMap : EmptyHolesArray;

//                                    // Add the height map values, inserting them into their correct positions.
//                                    for (var unitX = 0; unitX <= Constants.CellSize; unitX++)
//                                    {
//                                        for (var unitY = 0; unitY <= Constants.CellSize; unitY++)
//                                        {
//                                            var tileX = (x * Constants.CellSize) + unitX;
//                                            var tileY = (y * Constants.CellSize) + unitY;

//                                            var vertIndex = tileVertLocations[tileX, tileY];
//                                            if (vertIndex == -1)
//                                            {
//                                                var xPos = Constants.CenterPoint
//                                                           - (tileBlockX * Constants.TileSizeYrds)
//                                                           - (tileX * Constants.UnitSize);
//                                                var yPos = Constants.CenterPoint
//                                                           - (tileBlockY * Constants.TileSizeYrds)
//                                                           - (tileY * Constants.UnitSize);
//                                                var zPos = heights[unitX, unitY]; // Absolute height in Alpha.

//                                                tileVertLocations[tileX, tileY] = tileVerts.Count;
//                                                tileVerts.Add(new Vector3(xPos, yPos, zPos));
//                                            }

//                                            if (unitY == Constants.CellSize || unitX == Constants.CellSize)
//                                                continue;

//                                            tileHolesMap[tileX, tileY] = holes[unitX / 2, unitY / 2];
//                                        }
//                                    }
//                                }
//                            }

//                            var tileIndices = new List<int>();
//                            for (var tileX = 0; tileX < heightsPerTileSide; tileX++)
//                            {
//                                for (var tileY = 0; tileY < heightsPerTileSide; tileY++)
//                                {
//                                    if (tileHolesMap[tileX, tileY])
//                                        continue;

//                                    // Top triangle.
//                                    var vertId0 = tileVertLocations[tileX, tileY];
//                                    var vertId1 = tileVertLocations[tileX, tileY + 1];
//                                    var vertId9 = tileVertLocations[tileX + 1, tileY];
//                                    tileIndices.Add(vertId0);
//                                    tileIndices.Add(vertId1);
//                                    tileIndices.Add(vertId9);

//                                    // Bottom triangle.
//                                    var vertId10 = tileVertLocations[tileX + 1, tileY + 1];
//                                    tileIndices.Add(vertId1);
//                                    tileIndices.Add(vertId10);
//                                    tileIndices.Add(vertId9);
//                                }
//                            }

//                            if (adt.WMOs.Count > 0 || adt.MDXs.Count > 0)
//                                AppendWMOsAndMDX(ref tileVerts, ref tileIndices, adt);
//                            if (tileVerts.Count > 0)
//                                ExportMesh(outputFileName, tileVerts.Select(v => v.ToRecastCoordinates()), tileIndices);
//                        }

//                        GC.Collect();
//                    }
//                }
//            }
//        }

        
//    }
//}
