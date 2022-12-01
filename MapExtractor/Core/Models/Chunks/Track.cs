// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System;
using System.IO;

using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.Core.Models.Structures;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class Track<T> where T : new()
    {
        public string Name;
        public uint NrOfTracks;
        public MDLTRACKTYPE InterpolationType;
        public int GlobalSequenceId;
        public CAnimatorNode<T>[] Nodes;

        public Track(BinaryReader br, bool reverse = false)
        {
            br.BaseStream.Position -= 4;

            Name = br.ReadString(Constants.SizeTag);
            NrOfTracks = br.ReadUInt32();
            InterpolationType = (MDLTRACKTYPE)br.ReadUInt32();
            GlobalSequenceId = br.ReadInt32();

            Nodes = new CAnimatorNode<T>[NrOfTracks];
            for (int i = 0; i < NrOfTracks; i++)
            {
                uint Time = br.ReadUInt32();
                T Value = CreateInstance(br);

                if (InterpolationType > MDLTRACKTYPE.TRACK_LINEAR)
                {
                    T InTangent = CreateInstance(br);
                    T OutTrangent = CreateInstance(br);

                    Nodes[i] = new CAnimatorNode<T>(Time, reverse, Value, InTangent, OutTrangent);
                }
                else
                {
                    Nodes[i] = new CAnimatorNode<T>(Time, reverse, Value);
                }
            }
        }


        //public void PopulateM2Track<TType>(M2Track<TType> track, IEnumerable<Sequence> seqs) where TType : new()
        //{
        //    track.GlobalSequence = (short)GlobalSequenceId;
        //    track.InterpolationType = M2Track<TType>.InterpolationTypes.Linear;// (M2Track<TType>.InterpolationTypes)InterpolationType;
        //    var type = MDLTRACKTYPE.TRACK_LINEAR;// InterpolationType;

        //    int index = 0;
        //    foreach (var s in seqs)
        //    {
        //        var nodes = Nodes.Where(x => x.Time >= s.MinTime && x.Time <= s.MaxTime).OrderBy(x => x.Time);

        //        if (track.Timestamps.Count < index + 1)
        //            track.Timestamps.Add(new M2Array<uint>());
        //        if (track.Values.Count < index + 1)
        //            track.Values.Add(new M2Array<TType>());

        //        track.Timestamps[index].AddRange(nodes.Select(x => x.Time - (uint)s.MinTime));
        //        track.Values[index].AddRange(nodes.SelectMany(x => x.Convert<TType>(type)));

        //        index++;
        //    }
        //}

        private T CreateInstance(BinaryReader br)
        {
            switch (typeof(T).Name)
            {
                case "Single":
                    return (T)(object)br.ReadSingle();
                case "Int32":
                    return (T)(object)br.ReadInt32();
                default:
                    return (T)Activator.CreateInstance(typeof(T), br);
            }
        }
    }

    public class CAnimatorNode<T>
    {
        public uint Time;
        public T Value;
        public T InTangent;
        public T OutTangent;

        public CAnimatorNode(uint Time, bool Reverse, T Value)
        {
            this.Time = Time;
            this.Value = Value;

            if (Reverse)
                ((CVector3)(object)Value).Reverse();
        }

        public CAnimatorNode(uint Time, bool Reverse, T Value, T InTangent, T OutTangent)
        {
            this.Time = Time;
            this.Value = Value;
            this.InTangent = InTangent;
            this.OutTangent = OutTangent;

            if (Reverse)
            {
                ((CVector3)(object)Value).Reverse();
                ((CVector3)(object)InTangent).Reverse();
                ((CVector3)(object)OutTangent).Reverse();
            }                
        }
    }
}
