// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;

namespace AlphaCoreExtractor.Core.Models.Structures
{
    public class CCylinder
    {
        public CVector3 Base;
        public float Height;
        public float Radius;

        public CCylinder(BinaryReader br)
        {
            Base = new CVector3(br);
            Height = br.ReadSingle();
            Radius = br.ReadSingle();
        }
    }
}
