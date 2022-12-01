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
    public class ATCH : BaseChunk, IReadOnlyCollection<Attachment>
    {
        private Attachment[] Attachments;
        public int Padding;

        public ATCH(BinaryReader br, uint version) : base(br)
        {
            br.BaseStream.Position += Size;
            return;

            Attachments = new Attachment[br.ReadInt32()];
            Padding = br.ReadInt32(); // ignored by client

            for (int i = 0; i < Attachments.Length; i++)
                Attachments[i] = new Attachment(br);
        }

        public int Count => Attachments.Length;
        public IEnumerator<Attachment> GetEnumerator() => Attachments.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Attachments.AsEnumerable().GetEnumerator();
    }

    public class Attachment : GenObject
    {
        public uint TotalSize;
        public int AttachmentId;
        public byte Padding;
        public string Path;

        Track<float> VisibilityKeys;

        public Attachment(BinaryReader br)
        {
            TotalSize = br.ReadUInt32();
            long end = br.BaseStream.Position + TotalSize;

            ObjSize = br.ReadUInt32();
            Name = br.ReadCString(Constants.SizeName);
            ObjectId = br.ReadInt32();
            ParentId = br.ReadInt32();
            Flags = (GENOBJECTFLAGS)br.ReadUInt32();

			LoadTracks(br);

            AttachmentId = br.ReadInt32();
			
			Padding = br.ReadByte(); // confirmed padding         
            Path = br.ReadCString(Constants.SizeFileName);

			while (br.BaseStream.Position < end && !br.AtEnd())
            {
                string tagname = br.ReadString(4);
				switch (tagname)
                {
                    case "KVIS": VisibilityKeys = new Track<float>(br); break;
                    default:
                        br.BaseStream.Position -= 4;
                        return;
                }
            }
        }
    }
}
