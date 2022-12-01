// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.Core.Models.Structures;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class RIBB : BaseChunk, IReadOnlyCollection<RibbonEmitter>
    {
        RibbonEmitter[] RibbonEmitters;

        public RIBB(BinaryReader br, uint version) : base(br)
		{
            br.BaseStream.Position += Size;
            return;

            RibbonEmitters = new RibbonEmitter[br.ReadInt32()];
            for (int i = 0; i < RibbonEmitters.Length; i++)
                RibbonEmitters[i] = new RibbonEmitter(br);
        }

        public int Count => RibbonEmitters.Length;
        public IEnumerator<RibbonEmitter> GetEnumerator() => RibbonEmitters.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => RibbonEmitters.AsEnumerable().GetEnumerator();
    }

    public class RibbonEmitter : GenObject
    {
        public uint TotalSize;
        public uint NodeSize;
        public float HeightAbove;
        public float HeightBelow;
        public float Alpha;
        public CVector3 Color;
        public uint EdgesPerSecond;
        public float Lifetime;
        public uint TextureSlot;
        public uint TextureRows;
        public uint TextureColumns;
        public uint MaterialId;
        public float Gravity;

        public Track<float> HeightAboveKeys;
        public Track<float> HeightBelowKeys;
        public Track<float> AlphaKeys;
        public Track<float> VisibilityKeys;
		public Track<CVector3> ColorKeys;
		public SimpleTrack TextureKeys;

		public RibbonEmitter(BinaryReader br)
        {
            TotalSize = br.ReadUInt32();
            long end = br.BaseStream.Position + TotalSize;

            ObjSize = br.ReadUInt32();
            Name = br.ReadCString(Constants.SizeName);
            ObjectId = br.ReadInt32();
            ParentId = br.ReadInt32();
            Flags = (GENOBJECTFLAGS)br.ReadUInt32();
						
			LoadTracks(br);

            NodeSize = br.ReadUInt32();
            HeightAbove = br.ReadSingle();
            HeightBelow = br.ReadSingle();
            Alpha = br.ReadSingle();
            Color = new CVector3(br);
            Lifetime = br.ReadSingle();
            TextureSlot = br.ReadUInt32();
			EdgesPerSecond = br.ReadUInt32();
            TextureRows = br.ReadUInt32();
            TextureColumns = br.ReadUInt32();
            MaterialId = br.ReadUInt32();
            Gravity = br.ReadSingle();

            while (br.BaseStream.Position < end && !br.AtEnd())
            {
                string tagname = br.ReadString(4);
				switch (tagname)
                {
                    case "KRHA": HeightAboveKeys = new Track<float>(br); break;
                    case "KRHB": HeightBelowKeys = new Track<float>(br); break;
                    case "KRAL": AlphaKeys = new Track<float>(br); break;
                    case "KVIS": VisibilityKeys = new Track<float>(br); break;
					case "KRCO": ColorKeys = new Track<CVector3>(br, true); break;
					case "KRTX": TextureKeys = new SimpleTrack(br, true); break;
					default:
                        br.BaseStream.Position -= 4;
                        return;
                }
            }
        }
    }
}
