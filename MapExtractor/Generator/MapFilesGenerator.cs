﻿// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Linq;
using System.Text;

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
        public static bool GenerateMapFiles(CMapObj map, out int generatedMaps)
        {
            generatedMaps = 0;
            try
            {
                Logger.Notice($"Generating .map files for Map {map.DBCMap.MapName_enUS}");            
                // HeightMap
                for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
                {
                    for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
                    {
                        var tileBlock = map.TileBlocks[tileBlockX, tileBlockY];
                        if (tileBlock != null)
                        {
                            var mapID = map.DBCMap.ID.ToString("000");
                            var blockX = tileBlockX.ToString("00");
                            var blockY = tileBlockY.ToString("00");
                            var outputFileName = $@"{Paths.OutputMapsPath}{mapID}{blockX}{blockY}.map";

                            if (File.Exists(outputFileName))
                                throw new Exception("Found existent invalid file!");

                            using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
                            {
                                //Map version.
                                fs.Write(Encoding.ASCII.GetBytes(Globals.MapVersion), 0, 10);
                                using (BinaryWriter bw = new BinaryWriter(fs))
                                {
                                    WriteHeightMap(bw, tileBlock);
                                    WriteAreaInformation(bw, tileBlock, map);
                                    WriteLiquids(bw, tileBlock);
                                }
                            }

                            generatedMaps++;
                        }
                    }
                }

                if (generatedMaps == 0)
                    Logger.Warning($"No tile data for Map {map.DBCMap.MapName_enUS}");
                else
                    Logger.Success($"Generated {generatedMaps} .map files for Map {map.DBCMap.MapName_enUS}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private static void WriteLiquids(BinaryWriter bw, CMapArea tileBlock)
        {
            bool[,] liquid_show = new bool[Constants.GridSize, Constants.GridSize];
            float[,] liquid_height = new float[Constants.GridSize + 1, Constants.GridSize + 1];
            sbyte[,] liquid_flag = new sbyte[Constants.GridSize + 1, Constants.GridSize + 1];

            for (int i = 0; i < Constants.TileSize; i++)
            {
                for (int j = 0; j < Constants.TileSize; j++)
                {
                    var cell = tileBlock.Tiles[i, j];

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
                        bw.Write(liquid_flag[y, x]);
                        bw.Write(liquid_height[y, x]);
                    }
                    else
                        bw.Write((sbyte)-1);
                }
            }
        }

        private static void WriteAreaInformation(BinaryWriter bw, CMapArea tileBlock, CMapObj map)
        {
            for (int cy = 0; cy < Constants.TileSize; cy++)
            {
                for (int cx = 0; cx < Constants.TileSize; cx++)
                {
                    var cell = tileBlock.Tiles[cy, cx];
                    var areaNumber = cell.areaNumber;

                    if (map.DBCMap.ID < 2 && areaNumber < 4000000000 && DBCStorage.TryGetAreaByMapIdAndAreaNumber(map.DBCMap.ID, areaNumber, out AreaTable areaTable))
                    {
                        bw.Write((int)areaTable.ID);
                        bw.Write((uint)areaTable.AreaNumber);
                        bw.Write((byte)areaTable.Area_Flags);
                        bw.Write((byte)areaTable.Area_Level);
                        bw.Write((ushort)areaTable.Exploration_Bit);
                        bw.Write((byte)areaTable.FactionGroupMask);
                    }
                    else
                        bw.Write((int)-1);
                }
            }
        }

        private static void WriteHeightMap(BinaryWriter bw, CMapArea tileBlock)
        {
            var cell = tileBlock.TransformHeightData();
            for (int cy = 0; cy < 256; cy++)
                for (int cx = 0; cx < 256; cx++)
                    bw.Write(cell.CalculateZ(cy, cx));
        }
    }
}
