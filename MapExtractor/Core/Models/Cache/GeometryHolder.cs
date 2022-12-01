// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.Models.Cache
{
    /// <summary>
    /// Holds the default CLID from MDX in order to reuse them for transformations.
    /// Should improve RAM usage, hopefuly.
    /// </summary>
    public class GeometryHolder
    {
        public Vector3[] BoundingVertices;
        public Index3[] BoundingTriangles;
        public Vector3[] BoundingNormals;
    }
}
