using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[BurstCompile(CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
	public struct TransformPointsFromJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<float3> from;
		[WriteOnly]
		public NativeArray<float3> to;

		public float4x4 matrix;

		public void Execute(int index)
		{
			to[index] = mul(matrix, float4(from[index], 1f)).xyz;
		}
	}
}