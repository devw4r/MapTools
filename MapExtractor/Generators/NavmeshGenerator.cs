//// TheAlphaProject
//// Discord: https://discord.gg/RzBMAKU
//// Github:  https://github.com/The-Alpha-Project

//using System;
//using System.IO;
//using System.Text;

//using AlphaCoreExtractor.Log;
//using AlphaCoreExtractor.Core;
//using AlphaCoreExtractor.Helpers;

//namespace AlphaCoreExtractor.Generators
//{
//    public class NavmeshGenerator
//    {
//        public static void GenerateNavmeshes(CMapObj map, out int generatedMeshes)
//        {
//            generatedMeshes = 0;

//            try
//            {
//                uint total_tiles = Convert.ToUInt32(Constants.TileBlockSize * Constants.TileBlockSize);
//                int processed_tiles = 0;
//                Logger.Notice($"Generating .obj navmesh files for Map {map.DBCMap.MapName_enUS}");

//                for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
//                {
//                    for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
//                    {
//                        if (map.TileBlocks[tileBlockX, tileBlockY] is CMapArea tileArea)
//                        {
//                            var mapID = map.DBCMap.ID.ToString("000");
//                            var blockX = tileBlockX.ToString("00");
//                            var blockY = tileBlockY.ToString("00");
//                            var outputFileName = $@"{Paths.OutputMapsPath}{mapID}{blockX}{blockY}.obj";

//                            if (File.Exists(outputFileName))
//                            {
//                                try { File.Delete(outputFileName); }
//                                catch (Exception ex) { Logger.Error(ex.Message); return; }
//                            }

//                            if (tileBlockX == 34 && tileBlockY == 26)
//                                Console.WriteLine();

//                            float BLOCK_BASE_X = tileBlockX * (Constants.TileSizeYrds);
//                            float BLOCK_BASE_Y = tileBlockY * (Constants.TileSizeYrds);
//                            StringBuilder vertexBuilder = new StringBuilder();
//                            StringBuilder indexBuffer = new StringBuilder();
//                            int vertexCount = 1;
//                            for (int i = 0; i < Constants.TileSize; i++)
//                            {
//                                for (int j = 0; j < Constants.TileSize; j++)
//                                {
//                                    int vertex_index = 0;
//                                    for (int y = 0; y < 9 + 8; y++)
//                                    {
//                                        if ((y + 1) % 2 == 0)  // V8
//                                        {
//                                            for (int x = 0; x < 8; x++)
//                                            {
//                                                float posX = (BLOCK_BASE_X) + (x * Constants.UnitSize) + (j * (Constants.ChunkSize));
//                                                float posY = (BLOCK_BASE_Y) + (y / 2 * Constants.UnitSize) + (i * (Constants.ChunkSize));
//                                                float posZ = tileArea.Tiles[i, j].MCVTSubChunk.Heights[vertex_index];  // Absolute height, not relative to MCNK positions in Alpha.

//                                                vertexBuilder.Append($"v {-posY} {posZ} {-posX}\n");

//                                                indexBuffer.Append($"f {vertexCount} {vertexCount - 9} {vertexCount - 8}{Environment.NewLine}");
//                                                indexBuffer.Append($"f {vertexCount} {vertexCount + 9} {vertexCount + 8}{Environment.NewLine}");
//                                                indexBuffer.Append($"f {vertexCount} {vertexCount + 8} {vertexCount - 9}{Environment.NewLine}");
//                                                indexBuffer.Append($"f {vertexCount} {vertexCount - 8} {vertexCount + 9}{Environment.NewLine}");
//                                                indexBuffer.Append($"f {vertexCount} {vertexCount - 8} {vertexCount + 9}{Environment.NewLine}");

//                                                vertexCount++;
//                                                vertex_index++;
//                                            }
//                                        }
//                                        else  // V9
//                                        {
//                                            for (int x = 0; x < 9; x++)
//                                            {
//                                                float posX = (BLOCK_BASE_X) + (x * Constants.UnitSize) + (j * (Constants.ChunkSize));
//                                                float posY = (BLOCK_BASE_Y) + (y / 2 * Constants.UnitSize) + (i * (Constants.ChunkSize));
//                                                float posZ = tileArea.Tiles[i, j].MCVTSubChunk.Heights[vertex_index];  // Absolute height, not relative to MCNK positions in Alpha.

//                                                vertexBuilder.Append($"v {-posY} {posZ} {-posX}{Environment.NewLine}");

//                                                vertexCount++;
//                                                vertex_index++;
//                                            }
//                                        }
//                                    }
//                                }
//                            }

//                            using (StreamWriter sw = new StreamWriter(outputFileName))
//                            {
//                                sw.Write(vertexBuilder);
//                                sw.Write(indexBuffer);

//                                vertexBuilder.Clear();
//                                indexBuffer.Clear();
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {

//            }
//        }
//    }
//}
