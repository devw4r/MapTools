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
    public class SEQS : BaseChunk, IReadOnlyCollection<Sequence>
    {
        Sequence[] Sequences;

        public SEQS(BinaryReader br, uint version) : base(br)
        {
            br.BaseStream.Position += Size;
            return;

            Sequences = new Sequence[br.ReadUInt32()];
            for (int i = 0; i < Sequences.Length; i++)
                Sequences[i] = new Sequence(br);
        }

        public int Count => Sequences.Length;
        public IEnumerator<Sequence> GetEnumerator() => Sequences.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Sequences.AsEnumerable().GetEnumerator();
    }

    public class Sequence
    {
        public string Name;
        public int MinTime;
        public int MaxTime;
        public float MoveSpeed;
        public bool NonLooping;
        public CExtent Bounds;
        public float Frequency;
        public int MinReplay;
        public int MaxReplay;
        public uint BlendTime;
        public Sequence(BinaryReader br)
        {
            Name = br.ReadCString(Constants.SizeName);
            MinTime = br.ReadInt32();
            MaxTime = br.ReadInt32();
            MoveSpeed = br.ReadSingle();

            NonLooping = br.ReadInt32() == 1;
            Bounds = new CExtent(br);
            Frequency = br.ReadSingle();
            MinReplay = br.ReadInt32();
            MaxReplay = br.ReadInt32();
            BlendTime = br.ReadUInt32();
        }
    }
}
