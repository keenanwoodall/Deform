using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	/// <summary>
	/// Sets every normal in the NativeMeshData to 'float3 (0f, 0f, 0f).'
	/// </summary>
	[BurstCompile (CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
	public struct ResetNormalsJob : IJobParallelFor
	{
		public NativeArray<float3> normals;

		public void Execute (int index)
		{
			normals[index] = float3 (0f, 0f, 0f);
		}
	}
}