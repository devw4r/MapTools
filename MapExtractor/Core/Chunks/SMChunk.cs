﻿// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Collections.Generic;

using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Terrain;
using AlphaCoreExtractor.Core.Readers;
using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.Chunks
{
    public class SMChunk : IDisposable
    {
        public SMChunkFlags Flags;
        public uint indexX;
        public uint indexY;
        public float radius;
        public uint nLayers;
        public uint nDoodadRefs;
        public uint offsHeight;
        public uint offsNormal;
        public uint offsLayer;
        public uint offsRefs;
        public uint offsAlpha;
        public uint sizeAlpha;
        public uint offsShadow;
        public uint sizeShadow;
        public uint areaNumber;
        public uint nMapObjRefs;
        public ushort holes_low_mask;
        public ushort padding;
        public byte[] predTex = new byte[8];
        public byte[] noEffectDoodad = new byte[8];
        public uint offsSndEmitters;
        public uint nSndEmitters;
        public uint offsLiquid;
        public byte[] unused = new byte[24];
        private readonly long HeaderOffsetEnd = 0;

        public bool HasLiquids = false;

        private bool[,] holesMap;
        public bool[,] HolesMap
        {
            get
            {
                if (holesMap == null)
                {
                    holesMap = new bool[4, 4];
                    for (var i = 0; i < 16; i++)
                        holesMap[i / 4, i % 4] = (((holes_low_mask >> (i)) & 1) == 1);
                }
                return holesMap;
            }
        }

        /// MDX index reference to MDNM
        public List<int> MDXindexReference = new List<int>();
        /// WMO index reference to MONM
        public List<int> WMOindexReference = new List<int>();

        public MCNRSubChunk MCNRSubChunk;
        public MCVTSubChunk MCVTSubChunk;
        public MCSHSubChunk MCSHSubChunk;
        public List<MCSESubChunk> MCSESubChunsk = new List<MCSESubChunk>();
        public List<MCLQSubChunk> MCLQSubChunks = new List<MCLQSubChunk>();

        public SMChunk(BinaryReader reader, ADT adt)
        {
            Flags = (SMChunkFlags)reader.ReadUInt32();
            HasLiquids = (Flags & SMChunkFlags.HasLiquid) != 0;
            indexX = reader.ReadUInt32();
            indexY = reader.ReadUInt32();
            radius = reader.ReadSingle();
            nLayers = reader.ReadUInt32();
            nDoodadRefs = reader.ReadUInt32();
            offsHeight = reader.ReadUInt32(); // MCVT
            offsNormal = reader.ReadUInt32(); // MCNR
            offsLayer = reader.ReadUInt32();  // MCLY
            offsRefs = reader.ReadUInt32();   // MCRF
            offsAlpha = reader.ReadUInt32();  // MCAL
            sizeAlpha = reader.ReadUInt32();
            offsShadow = reader.ReadUInt32(); // MCSH
            sizeShadow = reader.ReadUInt32();
            areaNumber = reader.ReadUInt32(); // in alpha: zone id (4) sub zone id (4)
            nMapObjRefs = reader.ReadUInt32();
            holes_low_mask = reader.ReadUInt16();
            padding = reader.ReadUInt16();
            predTex = reader.ReadBytes(16); //It is used to determine which detail doodads to show. 2 bit 8*8 arr unsigned integers naming the layer.
            noEffectDoodad = reader.ReadBytes(8); // 1 bit 8*8 arr, doodads disabled if 1
            offsSndEmitters = reader.ReadUInt32(); // MCSE
            nSndEmitters = reader.ReadUInt32();
            offsLiquid = reader.ReadUInt32(); // MCLQ

            unused = reader.ReadBytes(24); //Padding

            HeaderOffsetEnd = reader.BaseStream.Position;

            // MCVT begin right after header.
            BuildSubMCVT(reader, offsHeight);

            //Has MCNR SubChunk
            if (offsNormal > 0)
                BuildSubMCNR(reader, offsNormal);

            if (offsLayer > 0)
                BuildMCLY(reader, offsLayer);

            if (offsRefs > 0)
                BuildMCRF(reader, offsRefs);

            if (offsAlpha > 0)
                BuildMCAL(reader, offsAlpha, (int)sizeAlpha);

            if (offsShadow > 0)
                BuildMCSH(reader, offsShadow, (int)sizeShadow);

            if (offsSndEmitters > 0)
                BuildMCSE(reader, offsSndEmitters, (int)nSndEmitters);

            if (offsLiquid > 0 && HasLiquids)
                BuildSubMCLQ(reader, offsLiquid);
        }

        private void BuildSubMCLQ(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            // In the alpha clients there is no size indicator at all. The optimal way of parsing this chunk is to (sequentially) validate what LQ_* flags are set,
            // if any, and read accordingly - this will also provide the liquid type and therefore what SLVert to use.
            foreach (SMChunkFlags flag in Flags.GetMCNKFlags())
                MCLQSubChunks.Add(new MCLQSubChunk(reader, flag));
        }

        private void BuildSubMCNR(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            //MCNRSubChunk = new MCNRSubChunk(reader);
        }

        // Offsets are relative to the end of MCNK header, in this case 0, read right away.
        private void BuildSubMCVT(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            MCVTSubChunk = new MCVTSubChunk(reader);
        }

        private void BuildMCSE(BinaryReader reader, uint offset, int count)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            //for(int i = 0; i < count; i++)
            //    MCSESubChunsk.Add(new MCSESubChunk(reader));
        }


        private void BuildMCSH(BinaryReader reader, uint offset, int size)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            //MCSHSubChunk = new MCSHSubChunk(reader);
        }

        /// <summary>
        /// Since there are no MMDX/MWMO MMID/MWID in alpha ADT, MCRF entries directly point to index in MDNM and MONM chunks.
        /// </summary>
        private void BuildMCRF(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            var dataHeader = new DataChunkHeader(reader);
            if (dataHeader.Token != Tokens.MCRF)
                throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MCRF]"}");

            //for (var i = 0; i < nDoodadRefs; i++)
            //    MDXindexReference.Add(reader.ReadInt32());

            //for (var i = 0; i < nMapObjRefs; i++)
            //    WMOindexReference.Add(reader.ReadInt32());
        }

        #region TODO
        private void BuildMCAL(BinaryReader reader, uint offset, int size)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            //reader.ReadBytes(size);
        }

        private void BuildMCLY(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            var dataHeader = new DataChunkHeader(reader);
            if (dataHeader.Token != Tokens.MCLY)
                throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MCLY]"}");
            //reader.ReadBytes(dataHeader.Size);
        }
        #endregion

        public void Dispose()
        {
            MDXindexReference.Clear();
            MDXindexReference = null;
            WMOindexReference.Clear();
            WMOindexReference = null;
            holesMap = null;
            MCNRSubChunk = null;
            MCVTSubChunk = null;
            MCLQSubChunks.Clear();
            MCLQSubChunks = null;
            MCSESubChunsk.Clear();
            MCSESubChunsk = null;
        }
    }
}
