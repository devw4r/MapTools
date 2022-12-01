// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.WorldObject.Chunks.WMOGroups
{
    public class MOBA
    {
        public byte LightMap;
        public byte Texture;
        public short BottomX;
        public short BottomY;
        public short BottomZ;
        public short TopX;
        public short TopY;
        public short TopZ;
        public int StartIndex;
        public ushort IndexCount;
        public ushort VertexStart;
        public ushort VertexEnd;
        public byte Flags;
        public byte TextureIndex;

        public MOBA(BinaryReader br, uint version)
        {
            LightMap = br.ReadByte();
            Texture = br.ReadByte();
            BottomX = br.ReadInt16();
            BottomY = br.ReadInt16();
            BottomZ = br.ReadInt16();
            TopX = br.ReadInt16();
            TopY = br.ReadInt16();
            TopZ = br.ReadInt16();
            StartIndex = br.ReadInt32();
            IndexCount = br.ReadUInt16();
            VertexStart = br.ReadUInt16();
            VertexEnd = br.ReadUInt16();
            Flags = br.ReadByte();
            TextureIndex = br.ReadByte();
        }
    }
}


