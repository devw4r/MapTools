// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Text;

namespace AlphaCoreExtractor.Core
{
    public class SMOHeader : BinaryReader
    {
        public uint TextureCount;
        public uint GroupCount;
        public uint PortalCount;
        public uint LighsCount;
        public uint DoodadNamesCount;
        public uint DoodadDefsCount;
        public uint DoodadSetsCount;
        public CImVector ambColor;
        public uint WmoID;
        public byte[] pad = new byte[28];

        public SMOHeader(byte[] chunk) : base(new MemoryStream(chunk))
        {
            TextureCount = this.ReadUInt32();
            GroupCount = this.ReadUInt32();
            PortalCount = this.ReadUInt32();
            LighsCount = this.ReadUInt32();
            DoodadNamesCount = this.ReadUInt32();
            DoodadDefsCount = this.ReadUInt32();
            DoodadSetsCount = this.ReadUInt32();
            ambColor = new CImVector(this);
            WmoID = this.ReadUInt32();
            pad = this.ReadBytes(28);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Header:");
            if (TextureCount > 0)
                sb.AppendLine($" Textures: {TextureCount}");
            if (GroupCount > 0)
                sb.AppendLine($" Groups: {GroupCount}");
            if (PortalCount > 0)
                sb.AppendLine($" Portals: {PortalCount}");
            if (LighsCount > 0)
                sb.AppendLine($" Lights: {LighsCount}");
            if (DoodadNamesCount > 0)
                sb.AppendLine($" DoodadNames: {DoodadNamesCount}");
            if (DoodadDefsCount > 0)
                sb.AppendLine($" DoodadDefs: {DoodadDefsCount}");
            if (DoodadSetsCount > 0)
                sb.AppendLine($" DoodadSets: {DoodadSetsCount}");
            return sb.ToString();
        }
    }
}
