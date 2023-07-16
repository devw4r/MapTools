// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Models.Structures;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
	public class GEOA : BaseChunk, IReadOnlyCollection<GeosetAnimation>
	{
		GeosetAnimation[] GeosetAnimations;

		public GEOA(BinaryReader br, uint version) : base(br)
		{
			br.BaseStream.Position += Size;
			return;

			GeosetAnimations = new GeosetAnimation[br.ReadInt32()];
			for (int i = 0; i < GeosetAnimations.Length; i++)
				GeosetAnimations[i] = new GeosetAnimation(br);
		}

		public int Count => GeosetAnimations.Length;
		public IEnumerator<GeosetAnimation> GetEnumerator() => GeosetAnimations.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GeosetAnimations.AsEnumerable().GetEnumerator();
	}

	public class GeosetAnimation
	{
		public int TotalSize;
		public float Alpha;
		public bool HasColorKeys;
		public CVector3 Color;
		public int GeosetId;

		public Track<float> AlphaKeys;
		public Track<CVector3> ColorKeys;

		public GeosetAnimation(BinaryReader br)
		{
			TotalSize = br.ReadInt32();
			long end = br.BaseStream.Position + TotalSize;

			GeosetId = br.ReadInt32();
			Alpha = br.ReadSingle();
			Color = new CVector3(br);
			
			HasColorKeys = br.ReadInt32() == 1;

			Color.Reverse();

			while (br.BaseStream.Position < end && !br.AtEnd())
			{
				string tagname = br.ReadString(4);
				switch (tagname)
				{
					case "KGAO": AlphaKeys = new Track<float>(br); break;
					case "KGAC": ColorKeys = new Track<CVector3>(br, true); break;
					default:
						br.BaseStream.Position -= 4;
						return;
				}
			}
		}
	}
}
