// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using AlphaCoreExtractor.Core.Models.Structures;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class PIVT : BaseChunk, IReadOnlyCollection<CVector3>
    {
        CVector3[] PivotPoints;

        public PIVT(BinaryReader br, uint version) : base(br)
		{
            br.BaseStream.Position += Size;
            return;

            PivotPoints = new CVector3[Size / 0xC];
            for (int i = 0; i < PivotPoints.Length; i++)
                PivotPoints[i] = new CVector3(br);
        }

        public int Count => PivotPoints.Length;
        public IEnumerator<CVector3> GetEnumerator() => PivotPoints.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => PivotPoints.AsEnumerable().GetEnumerator();
    }
}
