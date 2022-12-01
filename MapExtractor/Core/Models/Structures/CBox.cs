// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;

namespace AlphaCoreExtractor.Core.Models.Structures
{
    public class CBox
    {
        public CVector3 Min;
        public CVector3 Max;

        public CBox()
        {
            Min = new CVector3();
            Max = new CVector3();
        }

        public CBox(BinaryReader br)
        {
            Min = new CVector3(br);
            Max = new CVector3(br);
        }
    }
}
