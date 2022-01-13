
// Adapted to Jobs/Burst by Kenamis and then modified further for Deform
/* 
 * The following code was adapted from: https://schemingdeveloper.com
 *
 * Visit our game studio website: http://stopthegnomes.com
 *
 * License: You may use this code however you see fit, as long as you include this notice
 *          without any modifications.
 *
 *          You may not publish a paid asset on Unity store if its main function is based on
 *          the following code, but you may publish a paid asset that uses this code.
 *
 *          If you intend to use this in a Unity store asset or a commercial project, it would
 *          be appreciated, but not required, if you let me know with a link to the asset. If I
 *          don't get back to you just go ahead and use it anyway!
 */

using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using static Unity.Mathematics.math;

namespace Deform
{
    public static partial class MeshUtils
    {
        public static JobHandle RecalculateNormals(NativeMeshData data, float angle, JobHandle dependency = default)
        {
            data.VertexMap.Clear();
            data.VertexKeys.Clear();
            
            var normalsFirstJob = new CalculateVertexMapAndTriangleNormalsJob
            {
                triangles = data.IndexBuffer.Reinterpret<int, int3>(),
                vertices = data.VertexBuffer,
                triangleNormals = data.TriangleNormals,
                map = data.VertexMap.AsParallelWriter(),
            };
            var normalsFirstHandle = normalsFirstJob.Schedule(data.IndexBuffer.Length / 3, 32, dependency);

            var normalsSecondJob = new AddKeysFromMapJob
            {
                keys = data.VertexKeys,
                map = data.VertexMap
            };
            var normalsSecondHandle = normalsSecondJob.Schedule(normalsFirstHandle);

            var normalsLastJob = new CalculateNormalsJob
            {
                keys = data.VertexKeys.AsDeferredJobArray(),
                map = data.VertexMap,
                triNormals = data.TriangleNormals,
                normals = data.NormalBuffer,
                cosineThreshold = cos(angle * Mathf.Deg2Rad)
            };

            return normalsLastJob.Schedule(data.VertexKeys, 8, normalsSecondHandle);
        }

        // Change this if you require a different precision.
        private const int Tolerance = 100000;

        // Magic FNV values. Do not change these.
        private const long FNV32Init = 0x811c9dc5;
        private const long FNV32Prime = 0x01000193;

        private static int Float3Hash(float3 vector)
        {
            long rv = FNV32Init;
            rv ^= (long) round(vector.x * Tolerance);
            rv *= FNV32Prime;
            rv ^= (long) round(vector.y * Tolerance);
            rv *= FNV32Prime;
            rv ^= (long) round(vector.z * Tolerance);
            rv *= FNV32Prime;

            return rv.GetHashCode();
        }

        [BurstCompile(CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
        private struct CalculateVertexMapAndTriangleNormalsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<int3> triangles;
            [ReadOnly] public NativeArray<float3> vertices;
            public NativeArray<float3> triangleNormals;
            public NativeMultiHashMap<int, int2>.ParallelWriter map;

            public void Execute(int triIndex)
            {
                int3 tri = triangles[triIndex];

                float3 p1 = vertices[tri.y] - vertices[tri.x];
                float3 p2 = vertices[tri.z] - vertices[tri.x];
                float3 normal = normalizesafe(cross(p1, p2));

                triangleNormals[triIndex] = normal;

                int hash0 = Float3Hash(vertices[tri.x]);
                int hash1 = Float3Hash(vertices[tri.y]);
                int hash2 = Float3Hash(vertices[tri.z]);

                map.Add(hash0, new int2 {x = tri.x, y = triIndex});
                map.Add(hash1, new int2 {x = tri.y, y = triIndex});
                map.Add(hash2, new int2 {x = tri.z, y = triIndex});
            }
        }

        [BurstCompile(CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
        private struct AddKeysFromMapJob : IJob
        {
            public NativeList<int> keys;
            [ReadOnly] public NativeMultiHashMap<int, int2> map;

            public void Execute()
            {
                keys.AddRange(map.GetKeyArray(Allocator.Temp));
            }
        }

        [BurstCompile(CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
        private struct CalculateNormalsJob : IJobParallelForDefer
        {
            [ReadOnly] public NativeArray<int> keys;
            [ReadOnly] public NativeMultiHashMap<int, int2> map;
            [ReadOnly] public NativeArray<float3> triNormals;
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> normals;

            public float cosineThreshold;

            public void Execute(int index)
            {
                var values = map.GetValuesForKey(keys[index]);
                foreach (int2 lhsEntry in values)
                {
                    float3 sum = 0f;

                    foreach (int2 rhsEntry in values)
                    {
                        if (lhsEntry.x == rhsEntry.x)
                        {
                            sum += triNormals[rhsEntry.y];
                        }
                        else
                        {
                            // The dot product is the cosine of the angle between the two triangles.
                            // A larger cosine means a smaller angle.
                            float dot = math.dot(triNormals[lhsEntry.y], triNormals[rhsEntry.y]);
                            if (dot >= cosineThreshold)
                            {
                                sum += triNormals[rhsEntry.y];
                            }
                        }
                    }

                    normals[lhsEntry.x] = normalize(sum);
                }
            }
        }
    }
}