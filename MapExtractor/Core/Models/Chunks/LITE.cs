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
    public class LITE : BaseChunk, IReadOnlyCollection<Light>
    {
        Light[] Lights;

        public LITE(BinaryReader br, uint version) : base(br)
        {
            br.BaseStream.Position += Size;
            return;

            Lights = new Light[br.ReadInt32()];
            for (int i = 0; i < Lights.Length; i++)
                Lights[i] = new Light(br);
        }

        public int Count => Lights.Length;
        public IEnumerator<Light> GetEnumerator() => Lights.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Lights.AsEnumerable().GetEnumerator();
    }

    public class Light : GenObject
    {
        public uint TotalSize;
        public LIGHT_TYPE Type;

        public float AttenuationStart;
        public float AttenuationEnd;
        public CVector3 Color;
        public float Intensity;
        public CVector3 AmbientColor;
        public float AmbientIntensity;


        public Track<float> AttenStartKeys;
        public Track<float> AttenEndKeys;
        public Track<CVector3> ColorKeys;
        public Track<float> IntensityKeys;
        public Track<CVector3> AmbColorKeys;
        public Track<float> AmbIntensityKeys;
        public Track<float> VisibilityKeys;

        public Light(BinaryReader br)
        {
            TotalSize = br.ReadUInt32();
            long end = br.BaseStream.Position + TotalSize;

            ObjSize = br.ReadUInt32();
            Name = br.ReadCString(Constants.SizeName);
            ObjectId = br.ReadInt32();
            ParentId = br.ReadInt32();
            Flags = (GENOBJECTFLAGS)br.ReadUInt32();

            LoadTracks(br);

            Type = (LIGHT_TYPE)br.ReadInt32();
            AttenuationStart = br.ReadSingle();
            AttenuationEnd = br.ReadSingle();
            Color = new CVector3(br);
            Intensity = br.ReadSingle();
            AmbientColor = new CVector3(br);    // added at version 700
            AmbientIntensity = br.ReadSingle(); // added at version 700

            while (br.BaseStream.Position < end && !br.AtEnd())
            {
                string tagname = br.ReadString(4);
                switch (tagname)
                {
                    case "KLAI": IntensityKeys = new Track<float>(br); break;
                    case "KLBI": AmbIntensityKeys = new Track<float>(br); break;
                    case "KVIS": VisibilityKeys = new Track<float>(br); break;
                    case "KLAC": ColorKeys = new Track<CVector3>(br, true); break;
                    case "KLBC": AmbColorKeys = new Track<CVector3>(br, true); break;
                    case "KLAS": AttenStartKeys = new Track<float>(br); break;
                    case "KLAE": AttenEndKeys = new Track<float>(br); break;
                    default:
                        br.BaseStream.Position -= 4;
                        return;
                }
            }
        }
    }
}
