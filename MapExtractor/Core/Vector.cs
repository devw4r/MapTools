// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

namespace AlphaCoreExtractor.Core
{
    public class Vector<T>
    {
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
