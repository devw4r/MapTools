// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using AlphaCoreExtractor.DBC;
using System.Collections.Generic;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor.Core
{
    public class CMapObj : BinaryReaderProgress
    {
        /// <summary>
        /// Reference to this CMapObj related DBCMap.
        /// </summary>
        public DBCMap DBCMap;

        /// <summary>
        /// Retrieved from Map.dbc
        /// </summary>
        public string Name;

        /// <summary>
        /// Should always be 18.
        /// </summary>
        public uint ADTVersion;

        /// <summary>
        /// Map header, contains general information of how many and what type of objects present in this map.
        /// </summary>
        public SMOHeader SMOHeader;

        /// <summary>
        /// Contains offsets/size for each TileBlock location.
        /// </summary>
        public SMAreaInfo[,] TileBlocksInformation = new SMAreaInfo[Constants.TileBlockSize, Constants.TileBlockSize];

        /// <summary>
        /// Doodads Names used across all map.
        /// Each name points to a .wdx file.
        /// </summary>
        public List<string> DoodadsNames = new List<string>();

        /// <summary>
        /// Object name sused across all map.
        /// Each name points to a .wmo file.
        /// </summary>
        public List<string> MapObjectsNames = new List<string>();

        /// <summary>
        /// Only one instance is possible. It is usually used by WMO based maps which contain no ADT parts with the exception of RazorfenDowns.
        /// If this chunk exists, the client marks the map as a dungeon and uses absolute positioning for lights.
        /// </summary>
        public SMMapObjDef MODF;

        /// <summary>
        /// TileBlocks, each map is divided into 64x64 TileBlocks, each block contains 16x16 Tiles.
        /// </summary>
        public CMapArea[,] TileBlocks = new CMapArea[Constants.TileBlockSize, Constants.TileBlockSize];

        /// <summary>
        /// How many tiles do not have information.
        /// </summary>
        public uint UsableTiles { get; private set; }

        /// <summary>
        /// How many tiles we have information for.
        /// </summary>
        public uint UnUsableTiles => 256 - UsableTiles;

        /// <summary>
        /// Used to report 'progress' everytime we read bytes.
        /// </summary>
        private BackgroundWorker AsyncLoadWorker;

        public CMapObj(DBCMap dbcMap, string filePath, BackgroundWorker worker) : base(new MemoryStream(File.ReadAllBytes(filePath)))
        {
            AsyncLoadWorker = worker;
            DBCMap = dbcMap;
            if (!dbcMap.MapName_enUS.Equals(Path.GetFileNameWithoutExtension(filePath)))
                throw new Exception("File map name nad DBC map name differs.");
            else
                Name = dbcMap.MapName_enUS;

            OnRead += new EventHandler((o, e) => AsyncLoadWorker?.ReportProgress(0));
        }

        public void LoadData()
        {
            Console.WriteLine($"Loading map: {Name} ContinentID: {DBCMap.ID} IsInstance: {DBCMap.IsInMap != 1}");

            // File version, must be 18.
            Console.WriteLine("Reading file version...");
            if (!ReadMVER())
                return;

            // MapHeader
            Console.WriteLine("Reading map header...");
            if (!ReadMPHD())
                return;

            // Map tile table. Needs to contain 64x64 = 4096 entries of sizeof(SMAreaInfo)
            Console.WriteLine("Reading MAIN section...");
            if (!ReadMAIN())
                return;

            // Filenames Doodads. Zero-terminated strings with complete paths to models.
            Console.WriteLine("Reading Doodads file names...");
            if (!ReadMDNM())
                return;

            // Filenames WMOS. Zero-terminated strings with complete paths to models.
            Console.WriteLine("Reading WMOS file names...");
            if (!ReadMONM())
                return;

            // Only one instance is possible. It is usually used by WMO based maps which contain no ADT parts with the exception of RazorfenDowns.
            // If this chunk exists, the client marks the map as a dungeon and uses absolute positioning for lights.
            Console.WriteLine("Reading MODF section...");
            if (!ReadMODF())
                return;

            // The start of what is now the ADT files.
            Console.WriteLine($"Reading {TileBlocksInformation.Length} TileBlocks information...");
            if (!LoadMapAreaChunks())
                return;

            Console.WriteLine();
            Console.WriteLine($"Map information:");
            Console.WriteLine($"ADT Version: {ADTVersion}");
            Console.Write(SMOHeader.ToString());
            Console.WriteLine($"DoodadsNames (.wdx): {DoodadsNames.Count}");
            Console.WriteLine($"MapObjectsNames (.wmo): {MapObjectsNames.Count}");
            Console.WriteLine($"Usable Tiles: {UsableTiles}");
            Console.WriteLine($"UnUsable Tiles: {UnUsableTiles}");
            PrintTileInformation();
            Console.WriteLine();

            Console.WriteLine("Map loading complete.");
        }

        /// <summary>
        /// The start of what is now the ADT files.
        /// </summary>
        private bool LoadMapAreaChunks()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                for (uint x = 0; x < 64; x++)
                {
                    for (uint y = 0; y < 64; y++)
                    {
                        var tileBlock = TileBlocksInformation[x, y];
                        // Do we have data for this Tile?
                        if (tileBlock != null & tileBlock.size > 0)
                        {
                            // Tile should not be already occupied.
                            if (TileBlocks[x, y] != null)
                                throw new Exception("Invalid tile location.");

                            var mapArea = new CMapArea(tileBlock.offset, this);

                            if (mapArea.Errors)
                            {
                                Console.WriteLine($"[WARNING] Unable to load information for tile {x},{y}");
                                continue;
                            }

                            UsableTiles++;
                            TileBlocks[x, y] = mapArea;
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

        private bool ReadMODF()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                var dataHeader = new DataChunkHeader(this);
                if (dataHeader.Token == Tokens.MODF)
                    MODF = new SMMapObjDef(this);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private bool ReadMVER()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                var dataHeader = new DataChunkHeader(this);
                if (dataHeader.Token != Tokens.MVER)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MVER]"}");

                ADTVersion = this.ReadUInt32();

                if (Globals.Verbose)
                    Console.WriteLine($"[MVER] Success.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private bool ReadMAIN()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                var dataHeader = new DataChunkHeader(this);
                if (dataHeader.Token != Tokens.MAIN)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MAIN]"}");

                for (int x = 0; x < 64; x++)
                    for (int y = 0; y < 64; y++)
                        TileBlocksInformation[x, y] = new SMAreaInfo(this);

                if (Globals.Verbose)
                {
                    Console.WriteLine($"Loaded {TileBlocksInformation.Length * TileBlocksInformation.Length} MapAreaChunks");
                    Console.WriteLine($"[MAIN] Success.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private bool ReadMPHD()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                var dataHeader = new DataChunkHeader(this);
                if (dataHeader.Token != Tokens.MPHD)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MPHD]"}.");

                var byteChunk = this.ReadBytes(dataHeader.Size);
                SMOHeader = new SMOHeader(byteChunk);

                if (Globals.Verbose)
                    Console.WriteLine($"[MPHD] Success.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public bool ReadMDNM() // DoodDasNames
        {
            try
            {
                if (this.IsEOF())
                    return false;

                var dataHeader = new DataChunkHeader(this);
                if (dataHeader.Token != Tokens.MDNM)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MDNM]"}.");

                if (dataHeader.Size > 0)
                {
                    long final_position = this.BaseStream.Position + dataHeader.Size;
                    while (this.BaseStream.Position < final_position)
                        DoodadsNames.Add(this.ReadCString());
                }

                if (Globals.Verbose)
                {
                    Console.WriteLine($"Loaded {DoodadsNames.Count} DoodadNames.");
                    Console.WriteLine($"[MDNM] Success.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public bool ReadMONM() // MapObjNames
        {
            try
            {
                if (this.IsEOF())
                    return false;

                var dataHeader = new DataChunkHeader(this);
                if (dataHeader.Token != Tokens.MONM)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MONM]"}.");

                if (dataHeader.Size > 0)
                {
                    long final_position = this.BaseStream.Position + dataHeader.Size;
                    while (this.BaseStream.Position < final_position)
                        MapObjectsNames.Add(this.ReadCString());
                }

                if (Globals.Verbose)
                {
                    Console.WriteLine($"Loaded {MapObjectsNames.Count} MapObjectsNames.");
                    Console.WriteLine($"[MONM] Success.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public void PrintTileInformation()
        {
            for (int x = 0; x < Constants.TileBlockSize; x++)
            {
                for (int y = 0; y < Constants.TileBlockSize; y++)
                {
                    if (TileBlocks[x, y] != null)
                    {
                        Console.WriteLine($"Tile: {Name}_{x}_{y}");
                        foreach (var areaName in TileBlocks[x, y].GetAreaNames())
                            Console.WriteLine($" {areaName}");
                    }
                }
            }
        }
    }
}
