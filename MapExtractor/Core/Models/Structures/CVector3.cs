// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System.IO;
using AlphaCoreExtractor.Core.Structures;

namespace AlphaCoreExtractor.Core.Models.Structures
{
	public class CVector3
	{
		public float X; //B
		public float Y; //G
		public float Z; //R

		public CVector3() { }

		public CVector3(BinaryReader br)
		{
			X = br.ReadSingle();
			Y = br.ReadSingle();
			Z = br.ReadSingle();
		}

		public CVector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public void Reverse()
		{
			var tmp = new CVector3(Z, Y, X);
			X = tmp.X;
			Z = tmp.Z;
		}

		public static bool operator ==(CVector3 left, CVector3 right) => left.Equals(right);

		public static bool operator !=(CVector3 left, CVector3 right) => !left.Equals(right);

		public Vector3 ToVector3 => new Vector3(X, Y, Z);

		public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}";

		public override int GetHashCode()
		{
			int hash = X.GetHashCode();
			hash ^= Y.GetHashCode();
			hash ^= Z.GetHashCode();
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (obj is CVector3 vec)
				return X == vec.X && Y == vec.Y & Z == vec.Z;

			return false;
		}
	}
}
