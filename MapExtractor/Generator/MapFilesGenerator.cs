// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Text;
using AlphaCoreExtractor.Core;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Generator
{
    public class MapFilesGenerator
    {
        public static bool GenerateMapFiles(CMapObj map)
        {
            return true;

            //Todo, need to proprely pack heights.
            try
            {
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

                            using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
                            {
                                fs.Write(Encoding.ASCII.GetBytes(Globals.MapVersion), 0, 8);
                                using (BinaryWriter br = new BinaryWriter(fs))
                                {
                                    for (int tileX = 0; tileX < Constants.TileSize; tileX++)
                                        for (int tileY = 0; tileY < Constants.TileSize; tileY++)
                                            foreach (float height in tileBlock.Tiles[tileX, tileY].MCVTSubChunk.Heights)
                                                br.Write(height);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }
    }
}
