using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Twirl", Description = "Rotates vertices around an axis based off of distance from that axis", XRotation = -90f, Type = typeof (TwirlDeformer))]
	public class TwirlDeformer : Deformer, IFactor
	{
		private const float MIN_RANGE = 0.0001f;

		public float Angle
		{
			get => angle;
			set => angle = value;
		}
		public float Factor
		{
			get => factor;
			set => factor = value;
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
		public float Inner
		{
			get => inner;
			set => inner = Mathf.Max (0f, Mathf.Min (value, Outer));
		}
		public float Outer
		{
			get => outer;
			set => outer = Mathf.Max (value, Inner);
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

		[SerializeField, HideInInspector] private float angle = 0f;
		[SerializeField, HideInInspector] private float factor = 1f;
		[SerializeField, HideInInspector] private BoundsMode mode = BoundsMode.Limited;
		[SerializeField, HideInInspector] private float inner = 0f;
		[SerializeField, HideInInspector] private float outer = 1f;
		[SerializeField, HideInInspector] private bool smooth = true;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (Factor == 0f)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			switch (Mode)
			{
				default:
					return new UnlimitedTwistJob
					{
						angle = Angle * Factor,
						meshToAxis = meshToAxis,
						axisToMesh = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer
					}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
				case BoundsMode.Limited:
					if (Mathf.Abs (Inner - Outer) < MIN_RANGE)
						return dependency;
					else
						return new LimitedTwistJob
						{
							angle = Angle * Factor,
							inner = Inner,
							outer = Outer,
							smooth = Smooth,
							meshToAxis = meshToAxis,
							axisToMesh = meshToAxis.inverse,
							vertices = data.DynamicNative.VertexBuffer
						}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct UnlimitedTwistJob : IJobParallelFor
		{
			public float angle;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));
				var distanceFromAxis = length (point.xy);
				var degrees = distanceFromAxis * angle;

				var rads = radians (degrees);
				point.xy = float2
				(
					point.x * cos (rads) - point.y * sin (rads),
					point.x * sin (rads) + point.y * cos (rads)
				);

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct LimitedTwistJob : IJobParallelFor
		{
			public float angle;
			public float inner;
			public float outer;
			public bool smooth;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var range = abs (outer - inner);

				var point = mul (meshToAxis, float4 (vertices[index], 1f));
				var distanceFromAxis = length (point.xy);
				var degrees = 0f;
				if (smooth)
					degrees = smoothstep (range, 0f, clamp (distanceFromAxis, inner, outer) - inner) * angle;
				else
					degrees = lerp (angle, 0f, (clamp (distanceFromAxis, inner, outer) - inner) / range);

				var rads = radians (degrees);
				point.xy = float2 
				(
					point.x * cos (rads) - point.y * sin (rads),
					point.x * sin (rads) + point.y * cos (rads)
				);

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}