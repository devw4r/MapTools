// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;

namespace AlphaCoreExtractor.Core.Models.Structures
{
    public class CVector2
    {
        public float X;
        public float Y;

        public CVector2() { }

        public CVector2(BinaryReader br)
        {
            X = br.ReadSingle();
            Y = br.ReadSingle();
        }

        //public C2Vector ToC2Vector => new C2Vector(X, -Y); //Inverse Y coord

        public override string ToString() => $"X: {X}, Y: {Y}";
    }
}
