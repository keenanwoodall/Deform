using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Magnet", Description = "Attracts or repels vertices from a point", Type = typeof (MagnetDeformer))]
	public class MagnetDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = value;
		}
		public float Falloff
		{
			get => falloff;
			set => falloff = Mathf.Max (value, 0f);
		}
		public Transform Center
		{
			get
			{
				if (center == null)
					center = transform;
				return center;
			}
			set => center = value;
		}

		[SerializeField, HideInInspector] private float factor = 0f;
		[SerializeField, HideInInspector] private float falloff = 2f;
		[SerializeField, HideInInspector] private Transform center;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (Mathf.Approximately (Factor, 0f))
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Center, data.Target.GetTransform ());

			return new MagnetJob
			{
				factor = Factor,
				falloff = Falloff,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct MagnetJob : IJobParallelFor
		{
			public float factor;
			public float falloff;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				var dist = pow (length (point), 2f) / factor;
				if (dist == 0f)
					return;
				var t = factor * (1f / (pow (abs (dist), falloff)));
				var ut = clamp (t, float.MinValue, 1f);
				point = lerp (point, float3 (0), ut);

				vertices[index] = mul (axisToMesh, float4 (point, 1f)).xyz;
			}
		}
	}
}