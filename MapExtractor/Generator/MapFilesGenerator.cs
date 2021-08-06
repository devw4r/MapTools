// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Linq;

using AlphaCoreExtractor.DBC;
using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Core;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor.Generator
{
    public class MapFilesGenerator
    {
        public static void GenerateMapFiles(CMapObj map, out int generatedMaps)
        {
            generatedMaps = 0;
            try
            {
                int total_tiles = Constants.TileBlockSize * Constants.TileBlockSize;
                int processed_tiles = 0;
                Logger.Notice($"Generating .map files for Map {map.DBCMap.MapName_enUS}");
                // HeightMap
                for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
                {
                    for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
                    {
                        if (map.TileBlocks[tileBlockX, tileBlockY] is CMapArea tileArea)
                        {
                            var mapID = map.DBCMap.ID.ToString("000");
                            var blockX = tileBlockX.ToString("00");
                            var blockY = tileBlockY.ToString("00");
                            var outputFileName = $@"{Paths.OutputMapsPath}{mapID}{blockX}{blockY}.map";

                            if (File.Exists(outputFileName))
                            {
                                try { File.Delete(outputFileName); }
                                catch (Exception ex) { Logger.Error(ex.Message); return; }
                            }

                            using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create))
                            {
                                //Map version.
                                fileStream.WriteMapVersion();
                                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                                {
                                    WriteHeightMap(binaryWriter, tileArea);
                                    WriteAreaInformation(binaryWriter, tileArea, map);
                                    WriteLiquids(binaryWriter, tileArea);
                                }
                            }

                            generatedMaps++;
                        }

                        Logger.Progress(processed_tiles++, total_tiles);
                    }
                }

                if (generatedMaps == 0)
                    Logger.Warning($"No tile data for Map {map.DBCMap.MapName_enUS}");
                else
                    Logger.Success($"Generated {generatedMaps} .map files for Map {map.DBCMap.MapName_enUS}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        private static void WriteLiquids(BinaryWriter binaryWriter, CMapArea tileArea)
        {
            bool[,] liquid_show = new bool[Constants.GridSize, Constants.GridSize];
            float[,] liquid_height = new float[Constants.GridSize + 1, Constants.GridSize + 1];
            sbyte[,] liquid_flag = new sbyte[Constants.GridSize + 1, Constants.GridSize + 1];

            for (int i = 0; i < Constants.TileSize; i++)
            {
                for (int j = 0; j < Constants.TileSize; j++)
                {
                    var cell = tileArea.Tiles[i, j];

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


                    for (int y = 0; y < Constants.Cell_Size; y++)
                    {
                        int cy = i * Constants.Cell_Size + y;
                        for (int x = 0; x < Constants.Cell_Size; x++)
                        {
                            int cx = j * Constants.Cell_Size + x;
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

        private static void WriteAreaInformation(BinaryWriter binaryWriter, CMapArea tileArea, CMapObj map)
        {
            for (int cy = 0; cy < Constants.TileSize; cy++)
            {
                for (int cx = 0; cx < Constants.TileSize; cx++)
                {
                    var cell = tileArea.Tiles[cy, cx];
                    var areaNumber = cell.areaNumber;

                    if (map.DBCMap.ID < 2 && areaNumber < 4000000000 && DBCStorage.TryGetAreaByMapIdAndAreaNumber(map.DBCMap.ID, areaNumber, out AreaTable areaTable))
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

        private static void WriteHeightMap(BinaryWriter binaryWriter, CMapArea tileArea)
        {
            var cell = tileArea.TransformHeightData();
            for (int cy = 0; cy < 256; cy++)
                for (int cx = 0; cx < 256; cx++)
                    binaryWriter.Write(cell.CalculateZ(cy, cx));
        }
    }
}
