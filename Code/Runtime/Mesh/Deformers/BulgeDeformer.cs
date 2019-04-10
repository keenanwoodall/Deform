using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Bulge", Description = "Bulges a mesh", Type = typeof (BulgeDeformer), XRotation = -90f)]
	public class BulgeDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = value;
		}
		public float Top
		{
			get => top;
			set => top = Mathf.Max (value, bottom);
		}
		public float Bottom
		{
			get => bottom;
			set => bottom = Mathf.Min (value, top);
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
			set { axis = value; }
		}

		[SerializeField, HideInInspector] private float factor;
		[SerializeField, HideInInspector] private float top = 0.5f;
		[SerializeField, HideInInspector] private float bottom = -0.5f;
		[SerializeField, HideInInspector] private bool smooth = true;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (Mathf.Approximately (top, bottom) || Mathf.Approximately (Factor, 0f))
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			return new BulgeJob
			{
				factor = Factor,
				top = Top,
				bottom = Bottom,
				smooth = Smooth,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct BulgeJob : IJobParallelFor
		{
			public float factor;
			public float top;
			public float bottom;
			public bool smooth;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var normalizedDistanceBetweenBounds = (clamp (point.z, bottom, top) - bottom) / (top - bottom);
				if (smooth)
					normalizedDistanceBetweenBounds = smoothstep (0f, 1f, normalizedDistanceBetweenBounds);
				var signedDistanceBetweenBounds = (normalizedDistanceBetweenBounds - 0.5f) * 2f;

				point.xy *= signedDistanceBetweenBounds * signedDistanceBetweenBounds * -factor + factor + 1f;

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}