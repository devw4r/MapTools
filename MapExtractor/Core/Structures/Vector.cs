// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using AlphaCoreExtractor.Helpers.Enums;

namespace AlphaCoreExtractor.Core.Structures
{
    public class Vector<T>
    {
        public CoordinatesType CoordsType = CoordinatesType.WoW;

        public T X;
        public T Y;
        public T Z;

        public static Vector<T> Build(T x, T y, T z)
        {
            var vector = new Vector<T>();
            vector.X = x;
            vector.Y = y;
            vector.Z = z;
            return vector;
        }
    }
}
