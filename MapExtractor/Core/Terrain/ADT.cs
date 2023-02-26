// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Collections.Generic;

using AlphaCoreExtractor.DBC;
using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Generators;
using AlphaCoreExtractor.Core.Chunks;
using AlphaCoreExtractor.Core.Models;
using AlphaCoreExtractor.Core.Readers;
using AlphaCoreExtractor.DBC.Structures;
using AlphaCoreExtractor.Core.Structures;
using AlphaCoreExtractor.Core.WorldObject;

namespace AlphaCoreExtractor.Core.Terrain
{
    public class ADT : IDisposable
    {
        /// <summary>
		/// The X offset of the map in the 64 x 64 grid.
		/// </summary>
		public uint TileX
        {
            get;
            private set;
        }

        /// <summary>
        /// The Y offset of the map in the 64 x 64 grid.
        /// </summary>
        public uint TileY
        {
            get;
            private set;
        }

        /// <summary>
        /// This TileBlock map bounds.
        /// </summary>
        public Rect Bounds
        {
            get
            {
                var topLeftX = Constants.CenterPoint - ((TileX) * Constants.TileSizeYrds);
                var topLeftY = Constants.CenterPoint - ((TileY) * Constants.TileSizeYrds);
                var botRightX = topLeftX - Constants.TileSizeYrds;
                var botRightY = topLeftY - Constants.TileSizeYrds;
                return new Rect(new Point(topLeftX, topLeftY), new Point(botRightX, botRightY));
            }
        }

        /// <summary>
        /// General TileBlock information.
        /// </summary>
        public SMAreaHeader AreaHeader;

        /// <summary>
        /// List of textures used for texturing the terrain in this TileBlock.
        /// </summary>
        public MTEXChunk MTEXChunk;

        /// <summary>
        /// MDX refs for this TileBlock.
        /// </summary>
        public Dictionary<uint, MapDoodadDefinition> DoodadDefinitions = new Dictionary<uint, MapDoodadDefinition>();

        /// <summary>
        /// WMO refs for this TileBlock.
        /// </summary>
        public Dictionary<uint, MapObjectDefinition> ObjectDefinitions = new Dictionary<uint, MapObjectDefinition>();

        /// <summary>
        /// Offsets/Sizes for each Tile.
        /// </summary>
        public SMChunkInfo[,] TilesInformation = new SMChunkInfo[(int)Constants.TileSize, (int)Constants.TileSize];

        /// <summary>
        /// The actual Tiles.
        /// </summary>
        public SMChunk[,] Tiles = new SMChunk[(int)Constants.TileSize, (int)Constants.TileSize];

        /// <summary>
        /// Liquids transformed heightmap given 127 resolution.
        /// </summary>
        public LiquidsHeightmap LiquidsHeightmap;

        /// <summary>
        /// WMO's used in this ADT.
        /// </summary>
        public List<WMO> WMOs = new List<WMO>();

        /// <summary>
        /// MDX's used in this ADT.
        /// </summary>
        public List<MDX> MDXs = new List<MDX>();

        /// <summary>
        /// Failed to parse something?
        /// </summary>
        public bool Errors = true;

        /// <summary>
        /// Used to read file tokens and validate chunks.
        /// </summary>
        private DataChunkHeader DataChunkHeader;

        public ADT(uint offset, WDT terrainReader, DataChunkHeader dataChunkHeader, uint tileX, uint tileY)
        {
            TileX = tileX;
            TileY = tileY;

            DataChunkHeader = dataChunkHeader;

            // MHDR offset
            terrainReader.SetPosition(offset);

            // AreaHeader
            if (!BuildAreaHeader(terrainReader))
                return;

            // MCIN, 256 Entries, so a 16*16 Chunkmap.
            if (!BuildMCIN(terrainReader))
                return;

            // MTEX, List of textures used for texturing the terrain in this map tile.
            if (!BuildMTEX(terrainReader))
                return;

            // MDDF, Placement information for doodads (MDX models)
            // Additional to this, the models to render are referenced in each MCRF chunk.
            if (!BuildMDDF(terrainReader))
                return;

            // MODF, Placement information for WMOs.
            // Additional to this, the WMOs to render are referenced in each MCRF chunk.
            if (!BuildMODF(terrainReader))
                return;

            // The MCNK chunks have a large block of data that starts with a header, and then has sub-chunks of its own.
            // Each map chunk has 9x9 vertices, and in between them 8x8 additional vertices, several texture layers, normal vectors,
            // a shadow map, etc.
            // MCNK, The header is 128 bytes like later versions, but information inside is placed slightly differently.
            // Offsets are relative to the end of MCNK header.
            if (!BuildMCNK(terrainReader))
                return;

            // Liquids transformed heightmap given 127 resolution.
            LiquidsHeightmap = BuildLiquidsHeightmap();

            // Load related WMOs.
            if (Configuration.ShouldParseWMOs)
                foreach (MapObjectDefinition objectDefinition in ObjectDefinitions.Values)
                    if (terrainReader.LoadWMO(objectDefinition, out WMO wmo))
                        WMOs.Add(wmo);

            // Load related MDXs.
            if (Configuration.ShouldParseMDXs)
                foreach (MapDoodadDefinition modelDefinition in DoodadDefinitions.Values)
                    if (modelDefinition.Exists)
                        if (terrainReader.LoadMDX(modelDefinition, out MDX mdx))
                            MDXs.Add(mdx);

            Errors = false;
        }

        private bool BuildMCNK(WDT terrainReader)
        {
            try
            {
                for (int x = 0; x < Constants.TileSize; x++)
                {
                    for (int y = 0; y < Constants.TileSize; y++)
                    {
                        terrainReader.SetPosition(TilesInformation[x, y].offset);

                        DataChunkHeader.Fill(terrainReader);
                        if (DataChunkHeader.Token != Tokens.MCNK)
                            throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MCNK]"}");

                        Tiles[x, y] = new SMChunk(terrainReader, this);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildMODF(WDT terrainterrainReader)
        {
            try
            {
                DataChunkHeader.Fill(terrainterrainReader);
                if (DataChunkHeader.Token != Tokens.MODF)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MODF]"}");

                //MODF (Placement information for WMOs. Additional to this, the WMOs to render are referenced in each MCRF chunk)
                var dataChunk = terrainterrainReader.ReadBytes(DataChunkHeader.Size);
                if (Configuration.ShouldParseWMOs)
                    ObjectDefinitions = MapObjectDefinition.BuildFromChunk(dataChunk, terrainterrainReader.WmoFiles);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildMDDF(WDT terrainReader)
        {
            try
            {
                DataChunkHeader.Fill(terrainReader);
                if (DataChunkHeader.Token != Tokens.MDDF)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MDDF]"}");

                var dataChunk = terrainReader.ReadBytes(DataChunkHeader.Size);
                if (Configuration.ShouldParseMDXs)
                    DoodadDefinitions = MapDoodadDefinition.BuildFromChunck(dataChunk, terrainReader.MdxModelFiles);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildMTEX(WDT terrainReader)
        {
            try
            {
                DataChunkHeader.Fill(terrainReader);
                if (DataChunkHeader.Token != Tokens.MTEX)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MTEX]"}");

                var dataChunk = terrainReader.ReadBytes(DataChunkHeader.Size);
                //MTEXChunk = MTEXChunk.BuildFromChunk(dataChunk);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildMCIN(WDT terrainReader)
        {
            try
            {
                DataChunkHeader.Fill(terrainReader);
                if (DataChunkHeader.Token != Tokens.MCIN)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MCIN]"}");

                // All tiles should be used, meaming we should have valid offset and size for each tile.
                for (int x = 0; x < Constants.TileSize; x++)
                    for (int y = 0; y < Constants.TileSize; y++)
                        TilesInformation[x, y] = new SMChunkInfo(terrainReader);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildAreaHeader(WDT terrainReader)
        {
            try
            {
                DataChunkHeader.Fill(terrainReader);
                if (DataChunkHeader.Token != Tokens.MHDRChunk)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MHDRChunk]"}");

                AreaHeader = new SMAreaHeader(terrainReader);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private LiquidsHeightmap BuildLiquidsHeightmap()
        {
            LiquidsHeightmap _heightMap = new LiquidsHeightmap();

            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    _heightMap.V8[x, y] = (float)this.Tiles[x / 8, y / 8].MCVTSubChunk.V8[x % 8, y % 8];
                    _heightMap.V9[x, y] = (float)this.Tiles[x / 8, y / 8].MCVTSubChunk.V9[x % 8, y % 8];
                }

                _heightMap.V9[x, 128] = (float)this.Tiles[x / 8, 15].MCVTSubChunk.V9[x % 8, 8];
                _heightMap.V9[128, x] = (float)this.Tiles[15, x / 8].MCVTSubChunk.V9[8, x % 8];
            }

            _heightMap.V9[128, 128] = (float)this.Tiles[15, 15].MCVTSubChunk.V9[8, 8];

            // Flush all MCVT chunks on all tiles.
            for (int x = 0; x < Constants.TileSize; x++)
                for (int y = 0; y < Constants.TileSize; y++)
                    Tiles[x, y]?.MCVTSubChunk?.Flush();

            return _heightMap;
        }

        #region #Helpers
        public IEnumerable<string> GetAreaNames(uint mapID)
        {
            HashSet<uint> areas = new HashSet<uint>();

            for (int x = 0; x < Constants.TileSize; x++)
            {
                for (int y = 0; y < Constants.TileSize; y++)
                {
                    var areaID = Tiles[x, y].areaNumber;
                    if (!areas.Contains(areaID))
                    {
                        if (DBCStorage.TryGetAreaByMapIdAndAreaNumber(mapID, areaID, out AreaTable areaTable))
                            yield return areaTable.Name;
                        else
                            yield return $"Unknown area {areaID}";
                        areas.Add(areaID);
                    }
                }
            }
        }
        #endregion

        public void WriteFiles(WDT wdt)
        {
            DataGenerator.WriteADTFiles(wdt, this);
        }

        public void Dispose()
        {
            AreaHeader = null;
            MTEXChunk = null;
            DoodadDefinitions?.Clear();
            DoodadDefinitions = null;
            ObjectDefinitions?.Clear();
            ObjectDefinitions = null;
            TilesInformation = null;
            LiquidsHeightmap?.Dispose();
            LiquidsHeightmap = null;

            foreach (var wmo in WMOs)
                wmo?.Dispose();
            WMOs?.Clear();
            WMOs = null;

            foreach (var mdx in MDXs)
                mdx?.Dispose();
            MDXs?.Clear();
            MDXs = null;

            foreach (var tile in Tiles)
                tile?.Dispose();
            Tiles = null;

            DataChunkHeader = null;
        }
    }
}
