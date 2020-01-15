using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace Deform
{
	[BurstCompile(CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
	public struct ElasticVertexUpdateJob : IJobParallelFor
	{
		public float strength, dampening, deltaTime;
		public float4x4 localToWorld, worldToLocal;

		public NativeArray<float3> velocities;
		public NativeArray<float3> currentVertices;
		[ReadOnly]
		public NativeArray<float3> targetVertices;

		public void Execute(int index)
		{
			var currentVertice = currentVertices[index];
			var targetVertice = targetVertices[index];

			var currentWorldVertice = math.mul(localToWorld, math.float4(currentVertice, 1f)).xyz;
			var targetWorldVertice = math.mul(localToWorld, math.float4(targetVertice, 1f)).xyz;

			var difference = currentWorldVertice - targetWorldVertice;
			var distance = math.length(difference);

			const float MinDistance = 0.0001f;
			const float MinVelocity = 0.0001f;

			if (distance < MinDistance && math.lengthsq(velocities[index]) < MinVelocity)
				return;

			var direction = math.normalize(difference);

			var velocity = direction * (distance * strength * deltaTime);
			velocities[index] -= velocity;
			velocities[index] *= dampening;

			currentWorldVertice += velocities[index];

			currentVertices[index] = math.mul(worldToLocal, math.float4(currentWorldVertice, 1f)).xyz;
		}
	}
}