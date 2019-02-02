using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	/// <summary>
	/// Adds the normal of every triangle to each vertex that it consists of.
	/// </summary>
	[BurstCompile (CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
	public struct AddTriangleNormalToNormalsJob : IJob
	{
		public NativeArray<int> triangles;
		public NativeArray<float3> vertices;
		public NativeArray<float3> normals;

		public void Execute ()
		{
			// Loop through each triangle.
			for (int i = 0; i < triangles.Length; i += 3)
			{
				// Get indices of the three vertices that make the current triangle.
				var t0 = triangles[i];
				var t1 = triangles[i + 1];
				var t2 = triangles[i + 2];
				// Get each vertice of the current triangle.
				var v0 = vertices[t0];
				var v1 = vertices[t1];
				var v2 = vertices[t2];
				// Calculate the triangle normal.
				var n = float3
				(
					v0.y * v1.z - v0.y * v2.z - v1.y * v0.z + v1.y * v2.z + v2.y * v0.z - v2.y * v1.z,
					-v0.x * v1.z + v0.x * v2.z + v1.x * v0.z - v1.x * v2.z - v2.x * v0.z + v2.x * v1.z,
					v0.x * v1.y - v0.x * v2.y - v1.x * v0.y + v1.x * v2.y + v2.x * v0.y - v2.x * v1.y
				);
				// Add the normal of the triangle to each of its vertices.
				normals[t0] += n;
				normals[t1] += n;
				normals[t2] += n;
			}
		}
	}
}