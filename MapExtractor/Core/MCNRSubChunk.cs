// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core
{
    //normalized. X, Z, Y. 127 == 1.0, -127 == -1.0.
    //Same as 3.x format, except values order which is like alpha MCVT : all outer values first, then all inner ones.
    public class MCNRSubChunk
    {
        public MCNREntry[] Entries;
        private ushort[] pad;

        /*
         * TODO: PAD bytes do not match with the comment below, our offset might be off?
         * About pad: 0.5.3.3368 lists this as padding always 0 112 245  18 0  8 0 0  0 84  245 18 0.
         * TODO: How do we read this accorind to explanation 'all outer values first, then inner'
         */
        public MCNRSubChunk(BinaryReader reader)
        {
            Entries = new MCNREntry[9 * 9 + 8 * 8];
            for (int i = 0; i < Entries.Length; i++)
                Entries[i] = MCNREntry.Read(reader);

            pad = new ushort[13];
            for (int i = 0; i < 13; i++)
                pad[i] = reader.ReadUInt16();
        }
    }
}
