// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Models.Structures;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
	public class MODL : BaseChunk
	{
		public string Name;
		public string AnimationFile;
		public CExtent Bounds;
		public uint BlendTime;
		public byte Flags;

		public MODL(BinaryReader br, uint version) : base(br)
		{
			br.BaseStream.Position += Size;
			return;

			Name = br.ReadCString(Constants.SizeName);
			AnimationFile = br.ReadCString(Constants.SizeFileName);
			Bounds = new CExtent(br);
			BlendTime = br.ReadUInt32();
			Flags = br.ReadByte();
		}
	}
}
