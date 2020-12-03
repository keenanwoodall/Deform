using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[BurstCompile(CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
	public struct TransformPointsJob : IJobParallelFor
	{
		public NativeArray<float3> points;
		public float4x4 matrix;
		
		public void Execute(int index)
		{
			points[index] = mul(matrix, float4(points[index], 1f)).xyz;
		}
	}
}