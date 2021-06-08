// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;

namespace AlphaCoreExtractor.Core
{
    public class BitReader : IDisposable
    {
        int CurrentBit;
        byte CurrentByte;
        Stream BaseStream;
        public BitReader(Stream stream)
        { 
            BaseStream = stream;
            CurrentByte = (byte)stream.ReadByte();
        }

        public bool ReadBit(bool BE = false)
        {
            if (CurrentBit == 8)
            {
                var _byte = BaseStream.ReadByte();
                CurrentBit = 0;
                CurrentByte = (byte)_byte;
            }

            bool value;
            if (!BE)
                value = (CurrentByte & (1 << CurrentBit)) > 0;
            else //LE
                value = (CurrentByte & (1 << (7 - CurrentBit))) > 0;

            CurrentBit++;
            return value;
        }

        public void Dispose()
        {
            BaseStream = null;
        }
    }
}
