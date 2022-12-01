// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core.Structures
{
    public class C3MCNRVector
    {
        public float x;
        public float y;
        public float z;

        // normalized. X, Z, Y. 127 == 1.0, -127 == -1.0.
        public C3MCNRVector(BinaryReader reader)
        {
            x = reader.ReadSByte() / 127.0f;
            y = reader.ReadSByte() / 127.0f;
            z = reader.ReadSByte() / 127.0f;
        }
    }
}
