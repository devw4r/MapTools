// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;

using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Core;
using AlphaCoreExtractor.Helpers;
using System.Collections.Generic;

namespace AlphaCoreExtractor.Generators
{
    public class TerrainMeshGenerator
    {
        private static readonly bool[,] EmptyHolesArray = new bool[4, 4];

        public static void BuildTerrainMesh(CMapObj map)
        {
            for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
            {
                for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
                {
                    if (map.TileBlocks[tileBlockX, tileBlockY] is CMapArea tileArea)
                    {
                        var mapID = map.DBCMap.ID.ToString("000");
                        var blockX = tileBlockX.ToString("00");
                        var blockY = tileBlockY.ToString("00");
                        var outputFileName = $@"{Paths.OutputMapsPath}{mapID}{blockX}{blockY}.obj";

                        if (File.Exists(outputFileName))
                        {
                            try { File.Delete(outputFileName); }
                            catch (Exception ex) { Logger.Error(ex.Message); return; }
                        }

                        int heightsPerTileSide = Constants.CellSize * Constants.TileSize;
                        var tileVertLocations = new int[heightsPerTileSide + 1, heightsPerTileSide + 1];
                        var tileHolesMap = new bool[heightsPerTileSide + 1, heightsPerTileSide + 1];
                        var tileVerts = new List<Vector<float>>();

                        for (var i = 0; i < (heightsPerTileSide + 1); i++)
                            for (var j = 0; j < (heightsPerTileSide + 1); j++)
                                tileVertLocations[i, j] = -1;

                        for (var x = 0; x < Constants.TileSize; x++)
                        {
                            for (var y = 0; y < Constants.TileSize; y++)
                            {
                                var tileChunk = tileArea.Tiles[x, y];
                                var heights = tileChunk.MCVTSubChunk.GetLowResMapMatrix();
                                var holes = (tileChunk.holes_low_mask > 0) ? tileChunk.HolesMap : EmptyHolesArray;

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
                                                       - (tileBlockX * Constants.TileSizeYrds)
                                                       - (tileX * Constants.UnitSize);
                                            var yPos = Constants.CenterPoint
                                                       - (tileBlockY * Constants.TileSizeYrds)
                                                       - (tileY * Constants.UnitSize);
                                            var zPos = heights[unitX, unitY]; // Absolute height in Alpha.

                                            tileVertLocations[tileX, tileY] = tileVerts.Count;
                                            tileVerts.Add(Vector<float>.Build(xPos, yPos, zPos));
                                        }

                                        if (unitY == Constants.CellSize || unitX == Constants.CellSize)
                                            continue;

                                        tileHolesMap[tileX, tileY] = holes[unitX / 2, unitY / 2];
                                    }
                                }
                            }
                        }

                        var tileIndices = new List<int>();
                        for (var tileX = 0; tileX < heightsPerTileSide; tileX++)
                        {
                            for (var tileY = 0; tileY < heightsPerTileSide; tileY++)
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

                        ExportMesh(outputFileName, tileVerts.ToArray(), tileIndices.ToArray());
                    }
                }
            }
        }

        private static void ExportMesh(string fileName, Vector<float>[] vertices, int[] indices)
        {
            using (var file = new StreamWriter(fileName))
            {
                foreach (var vertex in vertices)
                {
                    vertex.ToRecastCoordinates();
                    file.WriteLine("v {0} {1} {2}", vertex.X, vertex.Y, vertex.Z);
                }

                // Write faces.
                for (var i = 0; i < indices.Length; i += 3)
                    file.WriteLine("f {0} {1} {2}", indices[i + 2] + 1, indices[i + 1] + 1, indices[i] + 1);
            }
        }
    }
}
