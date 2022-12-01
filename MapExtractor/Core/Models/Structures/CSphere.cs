// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;

namespace AlphaCoreExtractor.Core.Models.Structures
{
    public class CSphere
    {
        public CVector3 Center;
        public float Radius;

        public CSphere(BinaryReader br)
        {
            Center = new CVector3(br);
            Radius = br.ReadSingle();
        }
    }
}
