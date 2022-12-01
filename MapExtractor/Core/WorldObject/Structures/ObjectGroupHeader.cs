// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.Collections.Generic;
using AlphaCoreExtractor.Core.Readers;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.WorldObject.Structures
{
    public class ObjectGroupHeader
    {
        public string GroupName = string.Empty;
        public string GroupDescritiveName = string.Empty;
        public int GroupNameStart;
        public int DescriptiveGroupNameStart;
        public MOGP_Flags Flags;
        public CAaBox BoundingBox;
        public uint PortalStart;
        public uint PortalCount;
        public byte[] FogIDS;
        public LiquidTypes GroupLiquid;
        public List<SMOGxBatch> IntBatch = new List<SMOGxBatch>();
        public List<SMOGxBatch> ExtBatch = new List<SMOGxBatch>();
        public uint WMOGroupId;

        public ObjectGroupHeader(BinaryReaderProgress br)
        {
            GroupNameStart = br.ReadInt32();         
            DescriptiveGroupNameStart = br.ReadInt32();
            Flags = (MOGP_Flags)br.ReadUInt32();
            BoundingBox = new CAaBox(br);
            PortalStart = br.ReadUInt32();
            PortalCount = br.ReadUInt32();
            FogIDS = br.ReadBytes(4);
            GroupLiquid = (LiquidTypes)br.ReadUInt32();
            for (int i = 0; i < 4; i++)
                IntBatch.Add(new SMOGxBatch(br));
            for (int i = 0; i < 4; i++)
                ExtBatch.Add(new SMOGxBatch(br));
            WMOGroupId = br.ReadUInt32();
            
            br.ReadBytes(8); // Padding

            if(br is WMO wmo)
            {
                if (GroupNameStart != -1)
                    GroupName = wmo.GroupNames[GroupNameStart];
                if (DescriptiveGroupNameStart > 0)
                    GroupDescritiveName = wmo.GroupNames[DescriptiveGroupNameStart];
            }
        }
    }
}
