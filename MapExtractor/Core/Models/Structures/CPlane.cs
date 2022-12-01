// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;

namespace AlphaCoreExtractor.Core.Models.Structures
{
    public class CPlane
    {
        public float Length;
        public float Width;

        public CPlane(BinaryReader br)
        {
            Length = br.ReadSingle();
            Width = br.ReadSingle();
        }
    }
}
