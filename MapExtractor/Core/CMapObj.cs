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
        public DBCMap DBCMap;
        public string Name;
        public uint ADTVersion;
        public SMOHeader SMOHeader;
        public SMAreaInfo[] SMAreaChunks = new SMAreaInfo[4096]; //64x64 sizeof(SMAreaInfo)
        public List<string> DoodadsNames = new List<string>();
        public List<string> MapObjectsNames = new List<string>();
        public SMMapObjDef MODF;
        public List<CMapArea> MapAreaChunks = new List<CMapArea>();
        public CMapArea[,] Tiles = new CMapArea[64, 64];

        public List<uint> GetUniqueAreaIDs()
        {
            var hashSet = new HashSet<uint>();
            foreach (var mapChunk in MapAreaChunks)
                foreach (var chunk in mapChunk.MapChunks)
                    hashSet.Add(chunk.area);

            return hashSet.ToList();
        }

        private BackgroundWorker Worker;
        public CMapObj(DBCMap dbcMap, string filePath, BackgroundWorker worker) : base(new MemoryStream(File.ReadAllBytes(filePath)))
        {
            DBCMap = dbcMap;
            Name = Path.GetFileNameWithoutExtension(filePath);
            Console.WriteLine($"Loading map: {Name}");
            Worker = worker;
            OnRead += OnBinaryRead;
        }

        private void OnBinaryRead(object sender, EventArgs e)
        {
            Worker?.ReportProgress(0);
        }

        public void LoadData()
        {
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
            Console.WriteLine($"Reading {SMAreaChunks.Length} Area chunks...");
            if (!LoadMapAreaChunks())
                return;

            Console.WriteLine();
            Console.WriteLine($"Map information:");
            Console.WriteLine($"ADT Version: {ADTVersion}");
            Console.Write(SMOHeader.ToString());
            Console.WriteLine($"DoodadsNames: {DoodadsNames.Count}");
            Console.WriteLine($"MapObjectsNames: {MapObjectsNames.Count}");
            Console.WriteLine($"SMAreaChunks: {SMAreaChunks.Length}");
            Console.WriteLine($"MapAreaChunks: {MapAreaChunks.Count}");

            Console.WriteLine();
            Console.WriteLine("Found data for the following Areas:");
            foreach (var area in GetUniqueAreaIDs())
            {
                if (DBCStorage.TryGetAreaByAreaNumber(area, out AreaTable table))
                    Console.WriteLine($" AreaNumber: {table.AreaNumber}\tAreaName: {table.AreaName_enUS}");
                else
                    Console.WriteLine($" No information found for Area id: {area}");
            }


            Console.WriteLine("Map loading complete.");
            OnRead -= OnBinaryRead;
            Worker = null;
        }

        //The start of what is now the ADT files.
        private bool LoadMapAreaChunks()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                uint adtNumber = 0;
                foreach (var areaChunk in SMAreaChunks)
                {
                    // Has no valid adt information, move on.
                    if (areaChunk.offset == 0)
                        continue;

                    var mapArea = new CMapArea(areaChunk.offset, this, adtNumber);

                    if (mapArea.Errors)
                        return false;

                    if (Tiles[mapArea.ADT_BlockX, mapArea.ADT_BlockY] != null)
                        throw new Exception("Invalid tile location.");

                    Tiles[mapArea.ADT_BlockX, mapArea.ADT_BlockY] = mapArea;
                    MapAreaChunks.Add(mapArea);
                    adtNumber++;
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

                var byteChunk = this.ReadBytes(dataHeader.Size);
                SMAreaChunks = SMAreaInfo.BuildFromChunk(byteChunk);

                if (Globals.Verbose)
                {
                    Console.WriteLine($"Loaded {SMAreaChunks.Length} MapAreaChunks");
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
    }
}
