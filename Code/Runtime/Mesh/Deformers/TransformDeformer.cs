using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Transform", Description = "Gives the mesh a new position, rotation and scale", Type = typeof (TransformDeformer))]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/TransformDeformer")]
    public class TransformDeformer : Deformer, IFactor
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
		public float Factor
		{
			get => factor;
			set => factor = value;
		}

		[SerializeField, HideInInspector]
		private Transform target;
		[SerializeField, HideInInspector]
		private float factor = 1f;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			Factor = Mathf.Clamp(Factor, 0f, 1f);
			if (Factor == 0f)
				return dependency;
			var dataTargetTransform = data.Target.GetTransform ();
			Matrix4x4 matrix = new Matrix4x4();
			matrix.SetTRS(
				Vector3.Lerp(dataTargetTransform.position, Target.position, Factor),
				Quaternion.Lerp(dataTargetTransform.rotation, Target.rotation, Factor),
				Vector3.Lerp(dataTargetTransform.localScale, Target.localScale, Factor)
				);
			matrix = dataTargetTransform.worldToLocalMatrix * matrix;
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
