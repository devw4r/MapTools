using AlphaCoreExtractor.Core.Readers;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Chunks
{
    /// <summary>
    /// MOGI
    /// </summary>
    public class GroupInformation
    {
        public uint Offset;
        public uint Size;
        public MOGP_Flags Flags;
        public CAaBox AaBox;
        public int NameIndex;

        public GroupInformation(BinaryReaderProgress br)
        {
            Offset = br.ReadUInt32();
            Size = br.ReadUInt32();
            Flags = (MOGP_Flags)br.ReadUInt32();
            AaBox = new CAaBox(br);
            NameIndex = br.ReadInt32();
        }
    }
}
