// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;

namespace AlphaCoreExtractor.Core.Models.Structures
{
    public class CExtent
    {
        public float Radius;
        public CBox Extent;

        public CExtent()
        {
            Extent = new CBox();
        }

        public CExtent(BinaryReader br)
        {
            Radius = br.ReadSingle();
            Extent = new CBox(br);
        }

        public override string ToString() => $"R: {Radius}, Min: {Extent.Min}, Max: {Extent.Max}";

    }
}
