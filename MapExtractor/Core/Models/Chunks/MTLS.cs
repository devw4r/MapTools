// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;
using System.Linq;
using System.Collections;
using AlphaCoreExtractor.Helpers;
using System.Collections.Generic;
using AlphaCoreExtractor.Helpers.Enums;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class MTLS : BaseChunk, IReadOnlyCollection<ModelMaterial>
    {
        ModelMaterial[] Materials;
        public uint Padding;

        public MTLS(BinaryReader br, uint version) : base(br)
		{
            br.BaseStream.Position += Size;
            return;

            Materials = new ModelMaterial[br.ReadUInt32()];
            Padding = br.ReadUInt32(); // god knows why this exists, client literally ignores it

			for (int i = 0; i < Materials.Length; i++)
                Materials[i] = new ModelMaterial(br);
        }

        public int Count => Materials.Length;
        public IEnumerator<ModelMaterial> GetEnumerator() => Materials.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Materials.AsEnumerable().GetEnumerator();
    }

    public class ModelMaterial
    {
        public uint TotalSize;
        public int PriorityPlane;
        public uint NrOfLayers;
        public List<Layer> Layers = new List<Layer>();

        public ModelMaterial(BinaryReader br)
        {
            TotalSize = br.ReadUInt32();
            long end = br.BaseStream.Position + TotalSize;

            PriorityPlane = br.ReadInt32();
            NrOfLayers = br.ReadUInt32();
            for (int i = 0; i < NrOfLayers; i++)
                Layers.Add(new Layer(br, PriorityPlane));
        }
    }

    public class Layer
    {
        public uint TotalSize;
        public MDLTEXOP BlendMode;
        public MDLGEO Flags;
        public uint TextureId;
        public int TextureAnimationId;
        public uint CoordId;
        public float Alpha;

        public int PriorityPlane;

        public SimpleTrack FlipKeys;
        public Track<float> AlphaKeys;

        public Layer(BinaryReader br, int priorityplane)
        {
            PriorityPlane = priorityplane;

            TotalSize = br.ReadUInt32();
            long end = br.BaseStream.Position + TotalSize;

            BlendMode = (MDLTEXOP)br.ReadInt32();
            Flags = (MDLGEO)br.ReadUInt32();
			TextureId = br.ReadUInt32();
            TextureAnimationId = br.ReadInt32();
            CoordId = br.ReadUInt32();
            Alpha = br.ReadSingle();

            while (br.BaseStream.Position < end && !br.AtEnd())
            {
                string tagname = br.ReadString(4);
				switch (tagname)
                {
                    case "KMTA": AlphaKeys = new Track<float>(br); break;
                    case "KMTF": FlipKeys = new SimpleTrack(br, true); break;
                    default:
                        br.BaseStream.Position -= 4;
                        return;
                }
            }
        }
    }

}
