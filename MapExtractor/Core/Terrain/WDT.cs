// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Collections.Generic;

using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Models;
using AlphaCoreExtractor.Core.Chunks;
using AlphaCoreExtractor.Core.Readers;
using AlphaCoreExtractor.DBC.Structures;
using AlphaCoreExtractor.Core.Structures;
using AlphaCoreExtractor.Core.WorldObject;
using AlphaCoreExtractor.Core.Models.Cache;

namespace AlphaCoreExtractor.Core.Terrain
{
    public class WDT : BinaryReaderProgress
    {
        /// <summary>
        /// Reference to this CMapObj related DBCMap.
        /// </summary>
        public DBCMap DBCMap;

        /// <summary>
        /// MapID.
        /// </summary>
        public uint MapID => DBCMap.ID;

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
        /// Each name points to a .mdx file.
        /// </summary>
        public List<string> MdxModelFiles = new List<string>();

        /// <summary>
        /// Object name sused across all map.
        /// Each name points to a .wmo file.
        /// </summary>
        public List<string> WmoFiles = new List<string>();
       
        /// <summary>
        /// Only one instance is possible. It is usually used by WMO based maps which contain no ADT parts with the exception of RazorfenDowns.
        /// If this chunk exists, the client marks the map as a dungeon and uses absolute positioning for lights.
        /// </summary>
        public MapObjectDefinition MODF;

        /// <summary>
        /// TileBlocks, each map is divided into 64x64 TileBlocks, each block contains 16x16 Tiles.
        /// </summary>
        public ADT[,] TileBlocks = new ADT[Constants.TileBlockSize, Constants.TileBlockSize];

        /// <summary>
        /// How many tiles do not have information.
        /// </summary>
        public uint UsableTiles { get; private set; }

        /// <summary>
        /// Does this terrain file points to a global WMO only. (No Tiles)
        /// </summary>
        public bool IsWMOBased { get; private set; }

        /// <summary>
        /// How many tiles we have information for.
        /// </summary>
        public uint UnUsableTiles => ((uint)Constants.TileBlockSize * (uint)Constants.TileBlockSize) - UsableTiles;

        /// <summary>
        /// Used to read file tokens and validate chunks.
        /// </summary>
        private DataChunkHeader DataChunkHeader = new DataChunkHeader();


        public WDT(DBCMap dbcMap, string filePath) : base(new MemoryStream(File.ReadAllBytes(filePath)))
        {
            DBCMap = dbcMap;
            if (dbcMap.MapName_enUS.Equals(Path.GetFileNameWithoutExtension(filePath)))
                Name = dbcMap.MapName_enUS;
            else
                Name = Path.GetFileNameWithoutExtension(filePath);

            this.OnRead += OnBytesRead;
            LoadData();
            GC.Collect();
        }

        
        private void OnBytesRead(object sender, EventArgs e)
        {
            Logger.Progress("Parsing ADT information", this.BaseStream.Position, this.BaseStream.Length, 1000);
        }

        public void LoadData()
        {
            Logger.Notice($"Processing map: {Name} ContinentID: {DBCMap.ID} IsInstance: {DBCMap.IsInMap != 1}");

            // File version, must be 18.
            if (Globals.Verbose) Logger.Info("Reading file version...");
            if (!ReadMVER())
                return;

            // MapHeader
            if (Globals.Verbose) Logger.Info("Reading map header...");
            if (!ReadMPHD())
                return;

            // Map tile table. Needs to contain 64x64 = 4096 entries of sizeof(SMAreaInfo)
            if (Globals.Verbose) Logger.Info("Reading MAIN section...");
            if (!ReadMAIN())
                return;

            // Filenames Doodads. Zero-terminated strings with complete paths to models.
            if (Globals.Verbose) Logger.Info("Reading Doodads file names...");
            if (!ReadMDNM())
                return;

            // Filenames WMOS. Zero-terminated strings with complete paths to models.
            if (Globals.Verbose) Logger.Info("Reading WMOS file names...");
            if (!ReadMONM())
                return;

            // Only one instance is possible. It is usually used by WMO based maps which contain no ADT parts with the exception of RazorfenDowns.
            // If this chunk exists, the client marks the map as a dungeon and uses absolute positioning for lights.
            if (Globals.Verbose) Logger.Info("Reading MODF section...");
            if (!ReadMODF())
                return;

            // The start of what is now the ADT files.
            if (Globals.Verbose) Logger.Info($"Reading {TileBlocksInformation.Length} TileBlocks information...");
            if (!LoadAvailableADTs())
                return;

            // Read remaining bytes.
            this.ReadToEOF();

            if (Globals.Verbose)
            {
                Console.WriteLine();
                Logger.Notice($"Map information:");
                Logger.Info($"ADT Version: {ADTVersion}");
                Logger.Info(SMOHeader.ToString());
                Logger.Info($"MDX references (.mdx): {MdxModelFiles.Count}");
                Logger.Info($"WMO references (.wmo): {WmoFiles.Count}");
                Logger.Info($"Usable Tiles: {UsableTiles}");
                Logger.Info($"UnUsable Tiles: {UnUsableTiles}");
            }

            Logger.Success("Map information loaded successfully.");
        }

        /// <summary>
        /// The start of what is now the ADT files.
        /// </summary>
        private bool LoadAvailableADTs()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                for (uint x = 0; x < Constants.TileBlockSize; x++)
                {
                    if (x != 48)
                        continue;
                    for (uint y = 0; y < Constants.TileBlockSize; y++)
                    {
                        if (y != 31)
                            continue;

                        var tileBlock = TileBlocksInformation[x, y];
                        // Do we have data for this Tile?
                        if (tileBlock != null & tileBlock.size > 0)
                        {
                            using (ADT adt = new ADT(tileBlock.offset, this, DataChunkHeader, x, y))
                            {
                                if (adt.Errors)
                                {
                                    Logger.Warning($"[WARNING] Unable to load information for tile {x},{y}");
                                    continue;
                                }
                                else
                                {
                                    UsableTiles++;
                                    adt.WriteFiles(this);
                                }
                            }

                            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
                        }
                    }
                }

                // No tiles, uses WMO only.
                if (UsableTiles == 0)
                    IsWMOBased = true;

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMODF()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token == Tokens.MODF)
                    MODF = new MapObjectDefinition(this, WmoFiles);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMVER()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MVER)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MVER]"}");

                ADTVersion = this.ReadUInt32();

                if (Globals.Verbose)
                    Logger.Success($"[MVER]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMAIN()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MAIN)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MAIN]"}");

                for (int x = 0; x < 64; x++)
                    for (int y = 0; y < 64; y++)
                        TileBlocksInformation[x, y] = new SMAreaInfo(this);

                if (Globals.Verbose)
                    Logger.Info($"Loaded {TileBlocksInformation.Length * TileBlocksInformation.Length} MapAreaChunks");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool ReadMPHD()
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MPHD)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MPHD]"}.");

                var byteChunk = this.ReadBytes(DataChunkHeader.Size);
                //SMOHeader = new SMOHeader(byteChunk);

                if (Globals.Verbose)
                    Logger.Success($"[MPHD]");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        public bool ReadMDNM() // DoodDasNames
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MDNM)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MDNM]"}.");

                if (DataChunkHeader.Size > 0)
                {
                    long final_position = this.BaseStream.Position + DataChunkHeader.Size;
                    while (this.BaseStream.Position < final_position)
                        MdxModelFiles.Add(this.ReadCString().ToLower());
                }

                if (Globals.Verbose)
                    Logger.Success($"Loaded {MdxModelFiles.Count} MDX references.");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        public bool ReadMONM() // MapObjNames
        {
            try
            {
                if (this.IsEOF())
                    return false;

                DataChunkHeader.Fill(this);
                if (DataChunkHeader.Token != Tokens.MONM)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MONM]"}.");

                if (DataChunkHeader.Size > 0)
                {
                    long final_position = this.BaseStream.Position + DataChunkHeader.Size;
                    while (this.BaseStream.Position < final_position)
                        WmoFiles.Add(this.ReadCString().ToLower());
                }

                if (Globals.Verbose)
                    Logger.Success($"Loaded {WmoFiles.Count} WMO references.");

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        public bool LoadMDX(MapDoodadDefinition doodadDefinition, out MDX mdx)
        {
            mdx = null;
            var doodadPath = doodadDefinition.FilePath.ToLocalModelPath();

            // Todo, should probably append everything into a buffer and dump it at the end..
            if (Configuration.GenerateMdxPlacement)
            {
                // Write model placement in world to file.
                var posX = (doodadDefinition.Position.x - Constants.CenterPoint) * -1;
                var posY = (doodadDefinition.Position.y - Constants.CenterPoint) * -1;
                using (StreamWriter sw = new StreamWriter($"{DBCMap.MapName_enUS}_ModelsPlacement.txt", true))
                {
                    sw.WriteLine(Path.GetFileName(doodadDefinition.FilePath));
                    sw.WriteLine($".port {posX} {posY} {doodadDefinition.Position.z} {this.DBCMap.ID}");
                }
            }

            // Avoid loading MDX models that we already previously parsed and had no default collision.
            if (!CacheManager.ShouldLoad(doodadPath))
            {
                if (Globals.Verbose)
                    Logger.Warning($"Skip MDX model {Path.GetFileName(doodadPath)}, it as not collision.");
                return false;
            }

            try
            {
                mdx = new MDX(doodadPath, false);
                // If it has no collision, skip.
                if (!mdx.HasCollision)
                    return false;

                mdx.TransformMDX(doodadDefinition);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            return false;
        }

        public bool LoadWMO(MapObjectDefinition objectDefinition, out WMO wmo)
        {
            wmo = null;
            try
            {
                wmo = new WMO(objectDefinition.FilePath.ToLocalPath(), objectDefinition);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            this.OnRead -= OnBytesRead;
            DBCMap = null;
            Name = string.Empty;
            SMOHeader?.Dispose();
            SMOHeader = null;
            TileBlocksInformation = null;
            MdxModelFiles.Clear();
            MdxModelFiles = null;
            WmoFiles.Clear();
            WmoFiles = null;
            MODF = null;
            TileBlocks = null;
            DataChunkHeader = null;

            base.Dispose(disposing);
        }
    }
}
