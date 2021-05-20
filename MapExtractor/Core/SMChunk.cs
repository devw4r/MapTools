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

            if (offsHeight > 0) //Has MCVT SubChunk
            {
                BuildSubMCVT(this, offsHeight);

                if (Globals.Verbose)
                    Console.WriteLine($"Built MCVT SubChunk for Area: {area}");
            }

            if (offsNormal > 0) //Has MCNR SubChunk
            {
                BuildSubMCNR(this, offsNormal);

                if (Globals.Verbose)
                    Console.WriteLine($"Built MCNR SubChunk for Area: {area}");
            }
        }

        // Offsets are relative to the end of MCNK header.
        // Todo, is this the correct way? given offset - HeaderOffsetEnd(128) ?
        private void BuildSubMCNR(BinaryReader reader, uint offsNormal)
        {
            reader.BaseStream.Position = offsNormal - HeaderOffsetEnd;
            MCNRSubChunk = new MCNRSubChunk(reader);
        }

        private void BuildSubMCVT(BinaryReader reader, uint offsHeight)
        {
            reader.BaseStream.Position = offsHeight - HeaderOffsetEnd;
            MCVTSubChunk = new MCVTSubChunk(reader);
        }
    }
}
