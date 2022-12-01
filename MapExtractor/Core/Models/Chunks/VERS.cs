// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class VERS : BaseChunk
    {
        public new uint Version;
        public VERS(BinaryReader br, uint version) : base(br)
        {
            Version = br.ReadUInt32();
        }
    }
}
