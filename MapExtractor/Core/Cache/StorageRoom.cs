using System.Collections.Generic;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.Cache
{
    public static class StorageRoom
    {
        private static Queue<Vector3> VectorCache = new Queue<Vector3>();
        private static HashSet<Vector3> HashSet = new HashSet<Vector3>();
        public static void PushVector3(Vector3 vector)
        {
            if (VectorCache.Count > 400000)
                return;

            if (!HashSet.Contains(vector))
            {
                VectorCache.Enqueue(vector);
                HashSet.Add(vector);
            }
        }

        public static Vector3 PopVector3()
        {
            if (VectorCache.Count > 0)
            {
                var vector = VectorCache.Dequeue();
                HashSet.Remove(vector);
                return vector;
            }

            return new Vector3();
        }

        public static Vector3 PopVector3(float x, float y, float z)
        {
            if(VectorCache.Count > 0)
            {
                var vector = VectorCache.Dequeue();
                HashSet.Remove(vector);
                vector.Set(x, y, z);
                return vector;
            }

            return new Vector3(x, y, z);
        }
    }
}
