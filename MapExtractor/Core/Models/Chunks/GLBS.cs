// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class GLBS : BaseChunk, IReadOnlyCollection<int>
    {
        int[] Duration;

        public GLBS(BinaryReader br, uint version) : base(br)
		{
            br.BaseStream.Position += Size;
            return;

            Duration = new int[Size / 4];
            for (int i = 0; i < Duration.Length; i++)
                Duration[i] = br.ReadInt32();
        }

        public int Count => Duration.Length;
        public IEnumerator<int> GetEnumerator() => Duration.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Duration.AsEnumerable().GetEnumerator();
    }
}
