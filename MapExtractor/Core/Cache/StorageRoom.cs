// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.Collections.Concurrent;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.Cache
{
    public static class StorageRoom
    {
        private static readonly ConcurrentQueue<Vector3> VectorCache = new ConcurrentQueue<Vector3>();
        private static readonly ConcurrentHashSet<Vector3> HashSet = new ConcurrentHashSet<Vector3>();
        public static void PushVector3(Vector3 vector)
        {
            if (VectorCache.Count > 100000)
                return;

            if (!HashSet.Contains(vector))
            {
                VectorCache.Enqueue(vector);
                HashSet.Add(vector);
            }
        }

        public static Vector3 PopVector3()
        {
            if (VectorCache.Count > 0 && VectorCache.TryDequeue(out Vector3 vector))
            {
                HashSet.Remove(vector);
                return vector;
            }

            return new Vector3();
        }

        public static Vector3 PopVector3(float x, float y, float z)
        {
            if(VectorCache.Count > 0 && VectorCache.TryDequeue(out Vector3 vector))
            {
                HashSet.Remove(vector);
                vector.Set(x, y, z);
                return vector;
            }

            return new Vector3(x, y, z);
        }
    }
}
