// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Core
{
    public class SMChunk
    {
        public uint flags;
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
        public uint area;
        public uint nMapObjRefs;
        public ushort holes_low_res;
        public ushort padding;
        public byte[] predTex = new byte[8];
        public byte[] noEffectDoodad = new byte[8];
        public uint offsSndEmitters;
        public uint nSndEmitters;
        public uint offsLiquid;
        public byte[] unused = new byte[24];
        private long HeaderOffsetEnd = 0;

        public MCNRSubChunk MCNRSubChunk;
        public MCVTSubChunk MCVTSubChunk;
        public MCLQSubChunk MCLQSubChunk;

        public SMChunk(BinaryReader reader)
        {
            flags = reader.ReadUInt32();
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
            area = reader.ReadUInt32(); // in alpha: zone id (4) sub zone id (4)
            nMapObjRefs = reader.ReadUInt32();
            holes_low_res = reader.ReadUInt16();
            padding = reader.ReadUInt16();
            predTex = reader.ReadBytes(16); //It is used to determine which detail doodads to show.
            noEffectDoodad = reader.ReadBytes(8);
            offsSndEmitters = reader.ReadUInt32(); // MCSE
            nSndEmitters = reader.ReadUInt32();
            offsLiquid = reader.ReadUInt32(); // MCLQ

            unused = reader.ReadBytes(24);

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

            if (offsLiquid > 0)
                BuildSubMCLQ(reader, offsLiquid);
        }

        private void BuildSubMCLQ(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            var dataHeader = new DataChunkHeader(reader);
            if (dataHeader.Token == Tokens.MODF)
                MCLQSubChunk = new MCLQSubChunk(reader);
        }

        private void BuildSubMCNR(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            MCNRSubChunk = new MCNRSubChunk(reader);
        }

        // Offsets are relative to the end of MCNK header, in this case 0, read right away.
        private void BuildSubMCVT(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            MCVTSubChunk = new MCVTSubChunk(reader);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void BuildMCSE(BinaryReader reader, uint offset, int count)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            reader.ReadBytes(76 * count); //size of struct CWSoundEmitter
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void BuildMCSH(BinaryReader reader, uint offset, int size)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void BuildMCAL(BinaryReader reader, uint offset, int size)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            reader.ReadBytes(size);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void BuildMCRF(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            var dataHeader = new DataChunkHeader(reader);
            if (dataHeader.Token != Tokens.MCRF)
                throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MCRF]"}");
            reader.ReadBytes(dataHeader.Size);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void BuildMCLY(BinaryReader reader, uint offset)
        {
            reader.SetPosition(offset + HeaderOffsetEnd);

            if (reader.IsEOF())
                return;

            var dataHeader = new DataChunkHeader(reader);
            if (dataHeader.Token != Tokens.MCLY)
                throw new Exception($"Invalid token, got [{dataHeader.Token}] expected {"[MCLY]"}");
            reader.ReadBytes(dataHeader.Size);
        }
    }
}
