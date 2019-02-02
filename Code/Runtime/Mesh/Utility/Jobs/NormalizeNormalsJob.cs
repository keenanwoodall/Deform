using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	/// <summary>
	/// Normalized every normal in the NativeMeshData.
	/// </summary>
	[BurstCompile (CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
	public struct NormalizeNormalsJob : IJobParallelFor
	{
		public NativeArray<float3> normals;

		public void Execute (int index)
		{
			normals[index] = normalize (normals[index]);
		}
	}
}