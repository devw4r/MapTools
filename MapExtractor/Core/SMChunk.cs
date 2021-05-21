// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Core
{
    public class SMChunk : BinaryReader
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

        public SMChunk(byte[] chunk) : base(new MemoryStream(chunk))
        {
            flags = this.ReadUInt32();
            indexX = this.ReadUInt32();
            indexY = this.ReadUInt32();
            radius = this.ReadSingle();
            nLayers = this.ReadUInt32();
            nDoodadRefs = this.ReadUInt32();
            offsHeight = this.ReadUInt32(); // MCVT
            offsNormal = this.ReadUInt32(); // MCNR
            offsLayer = this.ReadUInt32();  // MCLY
            offsRefs = this.ReadUInt32();   // MCRF
            offsAlpha = this.ReadUInt32();  // MCAL
            sizeAlpha = this.ReadUInt32();
            offsShadow = this.ReadUInt32(); // MCSH
            sizeShadow = this.ReadUInt32();
            area = this.ReadUInt32(); // in alpha: zone id (4) sub zone id (4)
            nMapObjRefs = this.ReadUInt32();
            holes_low_res = this.ReadUInt16();
            padding = this.ReadUInt16();
            predTex = this.ReadBytes(16); //It is used to determine which detail doodads to show.
            noEffectDoodad = this.ReadBytes(8);
            offsSndEmitters = this.ReadUInt32(); // MCSE
            nSndEmitters = this.ReadUInt32();
            offsLiquid = this.ReadUInt32(); // MCLQ

            unused = this.ReadBytes(24);

            HeaderOffsetEnd = this.BaseStream.Position;

            // MCVT begin right after header.
            BuildSubMCVT(this, offsHeight);

            //Has MCNR SubChunk
            if (offsNormal > 0)
                BuildSubMCNR(this, offsNormal);

            if (offsLayer > 0)
                BuildMCLY(this, offsLayer);

            if (offsRefs > 0)
                BuildMCRF(this, offsRefs);

            if (offsAlpha > 0)
                BuildMCAL(this, offsAlpha, (int)sizeAlpha);

            if (offsShadow > 0)
                BuildMCSH(this, offsShadow, (int)sizeShadow);

            if (offsSndEmitters > 0)
                BuildMCSE(this, offsSndEmitters, (int)nSndEmitters);

            if (offsLiquid > 0)
                BuildSubMCLQ(this, offsLiquid);
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
            this.ReadBytes(dataHeader.Size);
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
            this.ReadBytes(dataHeader.Size);
        }
    }
}
