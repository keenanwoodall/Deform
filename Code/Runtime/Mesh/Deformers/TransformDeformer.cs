using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Transform", Description = "Gives the mesh a new position, rotation and scale", Type = typeof (TransformDeformer))]
	public class TransformDeformer : Deformer
	{
		public Transform Target
		{
			get
			{
				if (target == null)
					target = transform;
				return target;
			}
			set => target = value;
		}

		[SerializeField, HideInInspector]
		private Transform target;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			var dataTargetTransform = data.Target.GetTransform ();
			var matrix = dataTargetTransform.worldToLocalMatrix * Target.localToWorldMatrix;
			return new TransformJob
			{
				matrix = matrix,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, 256, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct TransformJob : IJobParallelFor
		{
			public float4x4 matrix;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				vertices[index] = mul (matrix, float4 (vertices[index], 1f)).xyz;
			}
		}
	}
}
