// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.Chunks
{
    public class MCNREntry
    {
        public C3MCNRVector Normal;

        public static MCNREntry Read(BinaryReader reader)
        {
            var mcnrEntry = new MCNREntry
            {
                Normal = new C3MCNRVector(reader)
            };
            return mcnrEntry;
        }
    }
}
