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
    public class TXAN : BaseChunk, IReadOnlyCollection<TextureAnimation>
    {
        TextureAnimation[] TextureAnimations;

        public TXAN(BinaryReader br, uint version) : base(br)
        {
            br.BaseStream.Position += Size;
            return;

            TextureAnimations = new TextureAnimation[br.ReadInt32()];
            for (int i = 0; i < TextureAnimations.Length; i++)
                TextureAnimations[i] = new TextureAnimation(br);
        }

        public int Count => TextureAnimations.Length;
        public IEnumerator<TextureAnimation> GetEnumerator() => TextureAnimations.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => TextureAnimations.AsEnumerable().GetEnumerator();
    }

    public class TextureAnimation
    {
        public uint TotalSize;

        public Track<CVector3> TranslationKeys;
        public Track<CVector4> RotationKeys;
        public Track<CVector3> ScaleKeys;

        public TextureAnimation(BinaryReader br)
        {
            TotalSize = br.ReadUInt32();
            long end = br.BaseStream.Position + TotalSize;

            while (br.BaseStream.Position < end && !br.AtEnd())
            {
                string tagname = br.ReadString(4);
                switch (tagname)
                {
                    case "KTAT": TranslationKeys = new Track<CVector3>(br); break;
                    case "KTAR": RotationKeys = new Track<CVector4>(br); break;
                    case "KTAS": ScaleKeys = new Track<CVector3>(br); break;
                    default:
                        br.BaseStream.Position -= 4;
                        return;
                }
            }
        }
    }
}
