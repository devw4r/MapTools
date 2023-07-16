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

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class HELP : BaseChunk, IReadOnlyCollection<Helper>
    {
        Helper[] Helpers;

        public HELP(BinaryReader br, uint version) : base(br)
		{
            Helpers = new Helper[br.ReadInt32()];
            for (int i = 0; i < Helpers.Length; i++)
                Helpers[i] = new Helper(br);
        }

        public int Count => Helpers.Length;
        public IEnumerator<Helper> GetEnumerator() => Helpers.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Helpers.AsEnumerable().GetEnumerator();
    }

    public class Helper : GenObject
    {
        public Helper(BinaryReader br)
        {
            ObjSize = br.ReadUInt32();
            Name = br.ReadCString(Constants.SizeName);
            ObjectId = br.ReadInt32();
            ParentId = br.ReadInt32();
            Flags = (GENOBJECTFLAGS)br.ReadUInt32();

			LoadTracks(br);
        }
    }
}
