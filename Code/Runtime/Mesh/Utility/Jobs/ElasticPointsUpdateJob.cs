using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static Beans.Unity.Mathematics.mathx;

namespace Deform
{
	[BurstCompile(CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
	public struct ElasticPointsUpdateJob : IJobParallelFor
	{
		public float dampingRatio, angularFrequency, deltaTime;

		public NativeArray<float3> velocities;
		public NativeArray<float3> currentPoints;
		[ReadOnly]
		public NativeArray<float3> targetPoints;

		public void Execute(int index)
		{
			var currentPoint = currentPoints[index];
			var targetPoint = targetPoints[index];
			var velocity = velocities[index];

			spring(ref currentPoint, ref velocity, targetPoint, dampingRatio, angularFrequency, deltaTime);

			currentPoints[index] = currentPoint;
			velocities[index] = velocity;
		}
	}
	[BurstCompile(CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
	public struct MaskedElasticPointsUpdateJob : IJobParallelFor
	{
		public float unmaskedDampingRatio, unmaskedAngularFrequency, maskedDampingRatio, maskedAngularFrequency, deltaTime;
		public int maskIndex;
		
		public NativeArray<float3> velocities;
		public NativeArray<float3> currentPoints;
		[ReadOnly]
		public NativeArray<float3> targetPoints;
		[ReadOnly]
		public NativeArray<float4> colors;

		public void Execute(int index)
		{
			var currentPoint = currentPoints[index];
			var targetPoint = targetPoints[index];
			var velocity = velocities[index];

			var mask = colors[index][maskIndex];

			var dampingRatio = lerp(maskedDampingRatio, unmaskedDampingRatio, mask);
			var angularFrequency = lerp(maskedAngularFrequency, unmaskedAngularFrequency, mask);

			spring(ref currentPoint, ref velocity, targetPoint, dampingRatio, angularFrequency, deltaTime);

			currentPoints[index] = currentPoint;
			velocities[index] = velocity;
		}
	}
}