// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Core
{
    public class DataChunkHeader
    {
        public int Token { get; private set; }
        public int Size { get; private set; }
        public string TokenName { get; private set; }

        public DataChunkHeader(BinaryReader reader)
        {
            TokenName = reader.ReadToken();
            Token = reader.ReadInt32();
            Size = reader.ReadInt32();

            if(Globals.Verbose)
                Console.WriteLine($"Token: {TokenName} Payload Length {Size}");
        }
    }
}
