// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.Core.Models.Structures;

namespace AlphaCoreExtractor.Core.Models.Chunks
{
    public class GenObject
    {
        public uint ObjSize;
        public string Name;
        public int ObjectId;
        public int ParentId;
        public GENOBJECTFLAGS Flags;

        public Track<CVector3> TranslationKeys;
        public Track<CVector4> RotationKeys;
        public Track<CVector3> ScaleKeys;

		public void LoadTracks(BinaryReader br)
        {
			while (!br.AtEnd())
            {
                string tagname = br.ReadString(4);
				switch (tagname)
                {
                    case "KGTR":
                        TranslationKeys = new Track<CVector3>(br);
                        break;
                    case "KGRT":
                        RotationKeys = new Track<CVector4>(br);
                        break;
                    case "KGSC":
                        ScaleKeys = new Track<CVector3>(br);
                        break;
                    default:
                        br.BaseStream.Position -= 4;
						return;
                }
            }

			
		}
    }
}
