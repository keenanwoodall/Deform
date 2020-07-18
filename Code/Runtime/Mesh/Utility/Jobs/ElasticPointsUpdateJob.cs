using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

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

			Spring(ref currentPoint, ref velocity, targetPoint, dampingRatio, angularFrequency, deltaTime);

			currentPoints[index] = currentPoint;
			velocities[index] = velocity;
		}
		
		/// <summary>
		/// Moves value towards target with spring forces.
		/// Equation used from this article http://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/
		/// </summary>
		/// <param name="value"></param>
		/// <param name="velocity"></param>
		/// <param name="target"></param>
		/// <param name="dampingRatio">A value of zero will result in infinite oscillation. A value of one will result in no oscillation.</param>
		/// <param name="angularFrequency">An angular frequency of 1 means the oscillation completes one full period over one second.</param>
		/// <param name="timeStep"></param>
		private void Spring(ref float3 value, ref float3 velocity, float3 target, float dampingRatio, float angularFrequency, float timeStep)
		{
			float r = angularFrequency * 2f * math.PI;
			float f = 1f + 2f * timeStep * dampingRatio * r;
			float rSquared = r * r;
			float hoo = timeStep * rSquared;
			float hhoo = timeStep * hoo;
			float detInv = 1f / (f + hhoo);
			float3 detX = f * value + timeStep * velocity + hhoo * target;
			float3 detV = velocity + hoo * (target - value);
			value = detX * detInv;
			velocity = detV * detInv;
		}
	}
}