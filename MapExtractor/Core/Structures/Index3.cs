// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Runtime.InteropServices;
using AlphaCoreExtractor.Core.Readers;

namespace AlphaCoreExtractor.Core.Structures
{
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    public struct Index3 : IEquatable<Index3>
    {
        [FieldOffset(0)]
        public short Index2;
        [FieldOffset(2)]
        public short Index1;
        [FieldOffset(4)]
        public short Index0;

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != typeof(Index3)) return false;
            return Equals((Index3)obj);
        }

        public bool Equals(Index3 other)
        {
            return other.Index0 == Index0 && other.Index1 == Index1 && other.Index2 == Index2;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Index0.GetHashCode();
                result = (result * 397) ^ Index1.GetHashCode();
                result = (result * 397) ^ Index2.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(Index3 left, Index3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Index3 left, Index3 right)
        {
            return !left.Equals(right);
        }

        public static Index3 FromReader(BinaryReaderProgress br)
        {
            var one = br.ReadInt16();
            var two = br.ReadInt16();
            var three = br.ReadInt16();
            return new Index3 { Index0 = three, Index1 = two, Index2 = one };
        }
    }
}
