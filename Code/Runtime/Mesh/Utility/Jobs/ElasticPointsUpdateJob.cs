using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
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
}