// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project
// MDXParser bt barncastle: https://github.com/barncastle/MDX-Parser

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Core.Cache;
using AlphaCoreExtractor.Core.Chunks;
using AlphaCoreExtractor.Core.Structures;
using AlphaCoreExtractor.Core.Models.Cache;
using AlphaCoreExtractor.Core.Models.Chunks;
using AlphaCoreExtractor.Core.Models.Structures;
using AlphaCoreExtractor.Core.WorldObject.Chunks;

namespace AlphaCoreExtractor.Core.Models
{
    public class MDX : IDisposable
    {
        /// <summary>
        /// Default MDX collision geometry, can be referenced across different MDX instances to avoid loading the same Vectors and Indices all over again.
        /// </summary>
        public GeometryHolder GeometryHolder;

        /// <summary>
        /// Do we have default CLID?.
        /// </summary>
        public bool HasCollision => GeometryHolder != null;

        /// <summary>
        /// List of vertices used for rendering this MDX in World Space
        /// </summary>
        public List<Vector3> Vertices = new List<Vector3>();

        /// <summary>
        /// List of indicies used for rendering this MDX in World Space
        /// </summary>
        public List<int> Indices = new List<int>();

        /// <summary>
        /// The Triangles formed from the vertices and indices.
        /// </summary>
        public List<Triangle> Triangles = new List<Triangle>();

        public readonly string BaseFile;
        public readonly string Magic;
        public IReadOnlyList<BaseChunk> Chunks;
        public IReadOnlyList<GenObject> Hierachy;
        public string Name => Get<MODL>().Name;
        public string AnimationFile => Get<MODL>().AnimationFile;
        public CExtent Bounds => Get<MODL>().Bounds;
        public uint BlendTime => Get<MODL>().BlendTime;
        public byte Flags => Get<MODL>().Flags;
        public uint Version { get; private set; } = 0;
        public bool Has<T>() => Chunks?.Any(x => x.GetType() == typeof(T)) ?? false;
        public T Get<T>() where T : class => (T)(object)(Chunks.FirstOrDefault(x => x.GetType() == typeof(T)));

        public MDX(string file, bool wmo)
        {
            BaseFile = file;

            // If we still don't have the CLID for this MDX, parse it.
            if (!CacheManager.GetMDXCollisionCache(file, out GeometryHolder))
            {
                List<BaseChunk> chunks = new List<BaseChunk>();
                using (var fs = new FileInfo(file).OpenRead())
                using (var br = new BinaryReader(fs))
                {
                    Magic = br.ReadString(4);
                    while (!br.AtEnd())
                        ReadChunk(br, chunks, wmo);
                }

                Chunks = null;
                Hierachy = null;

                if (HasCollision)
                    CacheManager.AddMDXCollisionCache(file, GeometryHolder);
                else
                    CacheManager.PushNoCollision(file);

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
            }
        }

        public void TransformMDX(MapDoodadDefinition doodadDefinition)
        {
            foreach (var vector in Vertices)
                StorageRoom.PushVector3(vector);
            Vertices.Clear();
            Indices.Clear();

            var tempIndices = new List<int>();
            foreach (var tri in GeometryHolder.BoundingTriangles)
            {
                tempIndices.Add(tri.Index0);
                tempIndices.Add(tri.Index1);
                tempIndices.Add(tri.Index2);
            }

            var posX = (doodadDefinition.Position.x - Constants.CenterPoint) * -1;
            var posY = (doodadDefinition.Position.y - Constants.CenterPoint) * -1;
            var origin = StorageRoom.PopVector3(posX, posY, doodadDefinition.Position.z);

            // Create the scale matrix used in the following loop.
            Matrix scaleMatrix;
            Matrix.CreateScale(doodadDefinition.Scale, out scaleMatrix);

            // Creation the rotations
            var rotateZ = Matrix.CreateRotationZ(MathHelper.ToRadians(doodadDefinition.OrientationB + 180));
            var rotateY = Matrix.CreateRotationY(MathHelper.ToRadians(doodadDefinition.OrientationA));
            var rotateX = Matrix.CreateRotationX(MathHelper.ToRadians(doodadDefinition.OrientationC));

            var worldMatrix = Matrix.Multiply(scaleMatrix, rotateZ);
            worldMatrix = Matrix.Multiply(worldMatrix, rotateX);
            worldMatrix = Matrix.Multiply(worldMatrix, rotateY);

            for (var i = 0; i < GeometryHolder.BoundingVertices.Length; i++)
            {
                var position = GeometryHolder.BoundingVertices[i];
                Vector3 normal = StorageRoom.PopVector3();

                // Not sure why we are missing one normal.
                // Close it pointing to the first face.
                if (i < GeometryHolder.BoundingNormals.Length)
                    normal = GeometryHolder.BoundingNormals[i];
                else
                    normal = GeometryHolder.BoundingNormals[0];

                // Scale and Rotate
                Vector3 rotatedPosition = StorageRoom.PopVector3();
                Vector3.Transform(ref position, ref worldMatrix, out rotatedPosition);

                Vector3 rotatedNormal = StorageRoom.PopVector3();
                Vector3.Transform(ref normal, ref worldMatrix, out rotatedNormal);
                rotatedNormal.Normalize();

                // Translate
                Vector3 finalVector = StorageRoom.PopVector3();
                Vector3.Add(ref rotatedPosition, ref origin, out finalVector);

                Vertices.Add(finalVector);
            }

            Indices.AddRange(tempIndices);
            tempIndices.Clear();
        }

        public void TransformWMOMDX(List<int> indices, DoodadDefinition doodadDefinition)
        {
            foreach (var vector in Vertices)
                StorageRoom.PushVector3(vector);
            Vertices.Clear();
            Indices.Clear();

            var origin = StorageRoom.PopVector3(doodadDefinition.Position.x, doodadDefinition.Position.y, doodadDefinition.Position.z);

            // Create the scalar
            var scalar = doodadDefinition.Scale;
            var scaleMatrix = Matrix.CreateScale(scalar);

            // Create the rotations
            var quatX = doodadDefinition.Rotation.x;
            var quatY = doodadDefinition.Rotation.y;
            var quatZ = doodadDefinition.Rotation.z;
            var quatW = doodadDefinition.Rotation.w;

            var rotQuat = new Quaternion(quatX, quatY, quatZ, quatW);
            var rotMatrix = Matrix.CreateFromQuaternion(rotQuat);

            var compositeMatrix = Matrix.Multiply(scaleMatrix, rotMatrix);

            for (var i = 0; i < GeometryHolder.BoundingVertices.Length; i++)
            {
                // Scale and transform
                var basePosVector = GeometryHolder.BoundingVertices[i];

                // Not sure why we are missing one normal.
                // Close it pointing to the first face.
                Vector3 baseNormVector = StorageRoom.PopVector3();
                if (i < GeometryHolder.BoundingNormals.Length)
                    baseNormVector = GeometryHolder.BoundingNormals[i];
                else
                    baseNormVector = GeometryHolder.BoundingNormals[0];

                // Rotate
                Vector3 rotatedPosVector = StorageRoom.PopVector3();
                Vector3.Transform(ref basePosVector, ref compositeMatrix, out rotatedPosVector);

                Vector3 rotatedNormVector = StorageRoom.PopVector3();
                Vector3.Transform(ref baseNormVector, ref compositeMatrix, out rotatedNormVector);
                rotatedNormVector.Normalize();

                // Translate
                Vector3 finalPosVector = StorageRoom.PopVector3();
                Vector3.Add(ref rotatedPosVector, ref origin, out finalPosVector);

                Vertices.Add(finalPosVector);
            }

            Indices.AddRange(indices);
            indices.Clear();
        }

        private void ReadChunk(BinaryReader br, List<BaseChunk> chunks, bool wmo)
        {
            Dictionary<string, Type> typeLookup = new Dictionary<string, Type>()
            {
                { "VERS", typeof(VERS) },
                { "MODL", typeof(MODL) },
                { "SEQS", typeof(SEQS) },
                { "MTLS", typeof(MTLS) },
                { "TEXS", typeof(TEXS) },
                { "GEOS", typeof(GEOS) },
                { "BONE", typeof(BONE) },
                { "HELP", typeof(HELP) },
                { "ATCH", typeof(ATCH) },
                { "PIVT", typeof(PIVT) },
                { "CAMS", typeof(CAMS) },
                { "EVTS", typeof(EVTS) },
                { "HTST", typeof(HTST) },
                { "CLID", typeof(CLID) },
                { "GLBS", typeof(GLBS) },
                { "GEOA", typeof(GEOA) },
                { "PRE2", typeof(PRE2) },
                { "RIBB", typeof(RIBB) },
                { "LITE", typeof(LITE) },
                { "TXAN", typeof(TXAN) },
            };

            // no point parsing last 8 bytes as it's either padding or an empty chunk
            if (br.BaseStream.Length - br.BaseStream.Position <= 8)
                return;

            string tag = br.ReadString(4);
            if (typeLookup.ContainsKey(tag))
            {
                br.BaseStream.Position -= 4;
                if (tag == "CLID")
                    chunks.Add((BaseChunk)Activator.CreateInstance(typeLookup[tag], br, Version, wmo, this));
                else
                    chunks.Add((BaseChunk)Activator.CreateInstance(typeLookup[tag], br, Version));

                if (tag == "VERS")
                    Version = ((VERS)chunks.Last()).Version;
            }
            else
            {
                throw new Exception($"Unknown type {tag}");
            }
        }

        private void PopulateHierachy()
        {
            List<GenObject> hierachy = new List<GenObject>();

            // inherits MDLGENOBJECT
            foreach (var chunk in Chunks)
                if (chunk is IReadOnlyCollection<GenObject> collection)
                    hierachy.AddRange(collection);

            hierachy.Sort((x, y) => x.ObjectId.CompareTo(y.ObjectId));
            Hierachy = hierachy;
        }

        public void Dispose()
        {
            GeometryHolder = null;
            Vertices?.Clear();
            Vertices = null;
            Indices?.Clear();
            Indices = null;
            Triangles?.Clear();
            Triangles = null;
        }
    }
}
