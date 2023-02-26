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
    public class EVTS : BaseChunk, IReadOnlyCollection<Event>
    {
        readonly Event[] Events;

        public EVTS(BinaryReader br, uint version) : base(br)
        {
            br.BaseStream.Position += Size;
            return;

            Events = new Event[br.ReadInt32()];
            for (int i = 0; i < Events.Length; i++)
                Events[i] = new Event(br);
        }

        public int Count => Events.Length;
        public IEnumerator<Event> GetEnumerator() => Events.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Events.AsEnumerable().GetEnumerator();
    }

    public class Event : GenObject
    {
        public uint TotalSize;
        public SimpleTrack EventKeys;

        public Event(BinaryReader br)
        {
            TotalSize = br.ReadUInt32();
            long end = br.BaseStream.Position + TotalSize;

            ObjSize = br.ReadUInt32();
            Name = br.ReadCString(Constants.SizeName);
            ObjectId = br.ReadInt32();
            ParentId = br.ReadInt32();
            Flags = (GENOBJECTFLAGS)br.ReadUInt32();

            LoadTracks(br);

            while (br.BaseStream.Position < end && !br.AtEnd())
            {
                string tagname = br.ReadString(4);
                switch (tagname)
                {
                    case "KEVT": EventKeys = new SimpleTrack(br, false); break;
                    default:
                        br.BaseStream.Position -= 4;
                        return;
                }
            }
        }
    }
}
