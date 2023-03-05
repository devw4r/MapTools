// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Chunks.WMOGroups
{
    /// <summary>
    /// MLIQ Info
    /// </summary>
    public class LiquidInformation
    {
        /// <summary>
        /// number of X vertices (xverts)
        /// </summary>
        public int XVertexCount;
        /// <summary>
        /// number of Y vertices (yverts)
        /// </summary>
        public int YVertexCount;
        /// <summary>
        /// number of X tiles (xtiles = xverts-1)
        /// </summary>
        public int XTileCount;
        /// <summary>
        /// number of Y tiles (ytiles = yverts-1)
        /// </summary>
        public int YTileCount;

        public Vector3 BaseCoordinates;
        public ushort MaterialId;

        public int VertexCount
        {
            get { return XVertexCount * YVertexCount; }
        }
        public int TileCount
        {
            get { return XTileCount * YTileCount; }
        }

        /// <summary>
        /// Grid of the Upper height bounds for the liquid vertices
        /// </summary>
        public float[,] HeightMapMax;

        /// <summary>
        /// Grid of the lower height bounds for the liquid vertices
        /// </summary>
        public float[,] HeightMapMin;

        /// <summary>
        /// Grid of flags for the liquid tiles
        /// They are often masked with 0xF
        /// They seem to determine the liquid type?
        /// </summary>
        public byte[,] LiquidTileFlags;

        public BoundingBox Bounds;

        /// <summary>
        /// The calculated LiquidType (for the DBC lookup) for the whole WMO
        /// Not parsed initially
        /// </summary>
        public uint LiquidType;

        public LiquidInformation(BinaryReader br)
        {
            XVertexCount = br.ReadInt32();
            YVertexCount = br.ReadInt32();
            XTileCount = br.ReadInt32();
            YTileCount = br.ReadInt32();
            BaseCoordinates = Vector3.FromReader(br);
            MaterialId = br.ReadUInt16();

            HeightMapMax = new float[YVertexCount, XVertexCount];
            HeightMapMin = new float[YVertexCount, XVertexCount];

            for (var y = 0; y < YVertexCount; y++)
            {
                for (var x = 0; x < XVertexCount; x++)
                {
                    HeightMapMin[y, x] = br.ReadSingle();
                    HeightMapMax[y, x] = br.ReadSingle();
                }
            }

            LiquidTileFlags = new byte[YTileCount, XTileCount];
            for (var y = 0; y < YTileCount; y++)
            {
                for (var x = 0; x < XTileCount; x++)
                {
                    LiquidTileFlags[y, x] = br.ReadByte();
                }
            }
        }
    }
}
