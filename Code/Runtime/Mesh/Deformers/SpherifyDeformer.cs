using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Spherify", Description = "Morphs vertices onto a sphere", Type = typeof (SpherifyDeformer))]
	public class SpherifyDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = Mathf.Clamp01 (value);
		}
		public float Radius
		{
			get => radius;
			set => radius = value;
		}
		public BoundsMode Mode
		{
			get => mode;
			set => mode = value;
		}
		public bool Smooth
		{
			get => smooth;
			set => smooth = value;
		}
		public Transform Axis
		{
			get
			{
				if (axis == null)
					axis = transform;
				return axis;
			}
			set => axis = value;
		}

		[SerializeField, HideInInspector] private float factor = 0f;
		[SerializeField, HideInInspector] private float radius = 1f;
		[SerializeField, HideInInspector] private BoundsMode mode = BoundsMode.Unlimited;
		[SerializeField, HideInInspector] private bool smooth = false;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (Mathf.Approximately (Factor, 0f) || Mathf.Approximately (Radius, 0f))
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			switch (Mode)
			{
				default:
				case BoundsMode.Unlimited:
					return new UnlimitedSpherifyJob
					{
						factor = Factor,
						radius = Radius,
						meshToAxis = meshToAxis,
						axisToMesh = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer
					}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
				case BoundsMode.Limited:
					return new LimitedSpherifyJob
					{
						factor = Factor,
						radius = Radius,
						smooth = Smooth,
						meshToAxis = meshToAxis,
						axisToMesh = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer
					}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct UnlimitedSpherifyJob : IJobParallelFor
		{
			public float factor;
			public float radius;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;
				var goalPoint = normalize (point) * radius;

				point = lerp (point, goalPoint, factor);

				vertices[index] = mul (axisToMesh, float4 (point, 1f)).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct LimitedSpherifyJob : IJobParallelFor
		{
			public float factor;
			public float radius;
			public bool smooth;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				var dist = length (point);
				if (dist == 0f)
					return;

				var normalizedDistance = dist / radius;
				if (normalizedDistance < 1f)
				{
					var t = factor;
					if (smooth)
						t *= (1f - smoothstep (0f, 1f, normalizedDistance));
					point = lerp (point, normalize (point) * radius, t);
				}

				vertices[index] = mul (axisToMesh, float4 (point, 1f)).xyz;
			}
		}
	}
}