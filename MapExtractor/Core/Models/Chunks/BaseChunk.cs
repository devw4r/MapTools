// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System;
using System.IO;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class BaseChunk
    {
        public string Type;
        public uint Size;
		protected uint Version;

        public BaseChunk(BinaryReader br)
        {
            Type = br.ReadString(4);
            Size = br.ReadUInt32();

            if (Type != GetType().Name)
                throw new Exception($"Expected {GetType().Name}, got {Type}");
        }
    }
}
