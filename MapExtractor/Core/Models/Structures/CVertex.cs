// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;

namespace AlphaCoreExtractor.Core.Models.Structures
{
    public class CVertex
    {
        public ushort Vertex1;
        public ushort Vertex2;
        public ushort Vertex3;

        public CVertex()
        {

        }

        public CVertex(BinaryReader br)
        {
            Vertex1 = br.ReadUInt16();
            Vertex2 = br.ReadUInt16();
            Vertex3 = br.ReadUInt16();
        }

        public ushort[] ToArray(ushort offset = 0) => new ushort[]
        {
            (ushort)(Vertex1 + offset),
            (ushort)(Vertex2 + offset),
            (ushort)(Vertex3 + offset)
        };
    }
}
