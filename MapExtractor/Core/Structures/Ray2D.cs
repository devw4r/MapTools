// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

namespace AlphaCoreExtractor.Core.Structures
{
    public struct Ray2D
    {
        public Vector2 Position;
        public Vector2 Direction;
        public Ray2D(Vector2 position, Vector2 direction)
        {
            Position = position;
            Direction = direction.NormalizedCopy();
        }
    }
}
