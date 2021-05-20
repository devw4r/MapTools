// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;

namespace AlphaCoreExtractor.Core
{
    public class BinaryReaderProgress : BinaryReader
    {
        public EventHandler OnRead;
        public BinaryReaderProgress(MemoryStream ms) : base(ms) { }

        public override uint ReadUInt32()
        {
            NotifyProgress();
            return base.ReadUInt32();
        }

        public override ushort ReadUInt16()
        {
            NotifyProgress();
            return base.ReadUInt16();
        }

        public override float ReadSingle()
        {
            NotifyProgress();
            return base.ReadSingle();
        }

        public override byte[] ReadBytes(int count)
        {
            NotifyProgress();
            return base.ReadBytes(count);
        }

        public override string ReadString()
        {
            NotifyProgress();
            return base.ReadString();
        }

        public override sbyte ReadSByte()
        {
            NotifyProgress();
            return base.ReadSByte();
        }

        /// <summary>
        /// Not real 'progress' but could be eventually.
        /// </summary>
        private void NotifyProgress()
        {
            OnRead?.Invoke(null, null);
        }
    }
}
