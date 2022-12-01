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
    public class CAMS : BaseChunk, IReadOnlyCollection<Camera>
    {
        Camera[] Cameras;

        public CAMS(BinaryReader br, uint version) : base(br)
        {
            br.BaseStream.Position += Size;
            return;

            Cameras = new Camera[br.ReadInt32()];
            for (int i = 0; i < Cameras.Length; i++)
                Cameras[i] = new Camera(br);
        }

        public int Count => Cameras.Length;
        public IEnumerator<Camera> GetEnumerator() => Cameras.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Cameras.AsEnumerable().GetEnumerator();
    }

    public class Camera
    {
        public uint TotalSize;
        public string Name;
        public CVector3 Pivot;
        public float FieldOfView;
        public float FarClip;
        public float NearClip;
        public CVector3 TargetPosition;

        public Camera(BinaryReader br)
        {
            TotalSize = br.ReadUInt32();
            long end = br.BaseStream.Position + TotalSize;

            Name = br.ReadCString(Constants.SizeName);
            Pivot = new CVector3(br);
            FieldOfView = br.ReadSingle();
            FarClip = br.ReadSingle();
            NearClip = br.ReadSingle();
            TargetPosition = new CVector3(br);

            Track<CVector3> TranslationKeys;
            Track<CVector3> TargetTranslationKeys;
            Track<float> RotationKeys;
            Track<float> VisibilityKeys;

            while (br.BaseStream.Position < end && !br.AtEnd())
            {
                string tagname = br.ReadString(4);
                switch (tagname)
                {
                    case "KCTR": TranslationKeys = new Track<CVector3>(br); break;
                    case "KCRL": RotationKeys = new Track<float>(br); break;
                    case "KTTR": TargetTranslationKeys = new Track<CVector3>(br); break;
                    case "KVIS": VisibilityKeys = new Track<float>(br); break;
                    default:
                        br.BaseStream.Position -= 4;
                        return;
                }
            }
        }
    }
}
