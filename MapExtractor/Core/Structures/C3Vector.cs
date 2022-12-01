// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.Structures
{
    public class C3Vector
    {
        public float x;
        public float y;
        public float z;

        public C3Vector(BinaryReader reader, bool swapped = false)
        {
            if (!swapped)
            {
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                z = reader.ReadSingle();
            }
            // To get WoW coords, read it as Y,Z,X
            else
            {
                y = reader.ReadSingle();
                z = reader.ReadSingle();
                x = reader.ReadSingle();
            }
        }

        public C3Vector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
