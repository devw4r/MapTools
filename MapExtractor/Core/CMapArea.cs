// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Collections.Generic;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Core
{
    public class CMapArea
    {
        public SMAreaHeader AreaHeader;
        public MTEXChunk MTEXChunk;
        public SMDoodadDef[] DoodadRefs;
        public SMMapObjDef[] SMMapObjDefs;
        public SMChunkInfo[] ChunkInformation;
        public List<SMChunk> MapChunks = new List<SMChunk>();

        public uint ADT_BlockX = 0;
        public uint ADT_BlockY = 0;
        public bool Errors = true;

        public CMapArea(uint offset, BinaryReader reader, uint adtNumber)
        {
            // This would be the BlockX,BlockY of the adt file, if we had them split.
            // World/Maps/<InternalMapName>/<InternalMapName>_<BlockX>_<BlockY>.adt.
            ADT_BlockX = adtNumber % 64;
            ADT_BlockY = adtNumber / 64;

            // MHDR offset
            reader.BaseStream.Position = offset;

            // AreaHeader
            if (!BuildAreaHeader(reader))
                return;

            // MCIN, 256 Entries, so a 16*16 Chunkmap.
            if (!BuildMCIN(reader))
                return;

            // MTEX, List of textures used for texturing the terrain in this map tile.
            if (!BuildMTEX(reader))
                return;

            // MDDF, Placement information for doodads (M2 models)
            // Additional to this, the models to render are referenced in each MCRF chunk.
            if (!BuildMDDF(reader))
                return;

            // MODF, Placement information for WMOs.
            // Additional to this, the WMOs to render are referenced in each MCRF chunk.
            if (!BuildMODF(reader))
                return;

            // The MCNK chunks have a large block of data that starts with a header, and then has sub-chunks of its own.
            // Each map chunk has 9x9 vertices, and in between them 8x8 additional vertices, several texture layers, normal vectors,
            // a shadow map, etc.
            // MCNK, The header is 128 bytes like later versions, but information inside is placed slightly differently.
            // Offsets are relative to the end of MCNK header.
            if (!BuildMCNK(reader))
                return;

            Errors = false;
        }

        private bool BuildMCNK(BinaryReader reader)
        {
            try
            {
                foreach (var chunkInformation in ChunkInformation)
                {
                    reader.BaseStream.Position = chunkInformation.offset;

                    var dataHeader = new DataChunkHeader(reader);
                    if (dataHeader.Token != Tokens.MCNK)
                        throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MCNK]"}");

                    var dataChunk = reader.ReadBytes(dataHeader.Size);
                    MapChunks.Add(new SMChunk(dataChunk));
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private bool BuildMODF(BinaryReader reader)
        {
            try
            {
                var dataHeader = new DataChunkHeader(reader);
                if (dataHeader.Token != Tokens.MODF)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MODF]"}");

                //MODF (Placement information for WMOs. Additional to this, the WMOs to render are referenced in each MCRF chunk)
                var dataChunk = reader.ReadBytes(dataHeader.Size);
                SMMapObjDefs = SMMapObjDef.BuildFromChunk(dataChunk);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private bool BuildMDDF(BinaryReader reader)
        {
            try
            {
                var dataHeader = new DataChunkHeader(reader);
                if (dataHeader.Token != Tokens.MDDF)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MDDF]"}");

                var dataChunk = reader.ReadBytes(dataHeader.Size);
                DoodadRefs = SMDoodadDef.BuildFromChunck(dataChunk);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private bool BuildMTEX(BinaryReader reader)
        {
            try
            {
                var dataHeader = new DataChunkHeader(reader);
                if (dataHeader.Token != Tokens.MTEX)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MTEX]"}");

                var dataChunk = reader.ReadBytes(dataHeader.Size);
                MTEXChunk = MTEXChunk.BuildFromChunk(dataChunk);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private bool BuildMCIN(BinaryReader reader)
        {
            try
            {
                var dataHeader = new DataChunkHeader(reader);
                if (dataHeader.Token != Tokens.MCIN)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MCIN]"}");

                var dataChunk = reader.ReadBytes(dataHeader.Size);
                ChunkInformation = SMChunkInfo.BuildFromChunk(dataChunk);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        private bool BuildAreaHeader(BinaryReader reader)
        {
            try
            {
                var dataHeader = new DataChunkHeader(reader);
                if (dataHeader.Token != Tokens.MHDRChunk)
                    throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MHDRChunk]"}");

                AreaHeader = new SMAreaHeader(reader);
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
