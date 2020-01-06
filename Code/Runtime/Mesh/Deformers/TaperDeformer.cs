using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Taper", Description = "Tapers a mesh", XRotation = -90f, Type = typeof (TaperDeformer))]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/TaperDeformer")]
    public class TaperDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => Vector2.Max (TopFactor, BottomFactor).magnitude; 
			set => TopFactor = BottomFactor = Vector2.one * value;
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
		public Vector2 TopFactor
		{
			get => topFactor;
			set => topFactor = value;
		}
		public Vector2 BottomFactor
		{
			get => bottomFactor;
			set => bottomFactor = value;
		}
		public float Curvature
		{
			get => curvature;
			set => curvature = value;
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

		[SerializeField, HideInInspector] private float top = 0.5f;
		[SerializeField, HideInInspector] private float bottom = -0.5f;
		[SerializeField, HideInInspector] private Vector2 topFactor = Vector2.one;
		[SerializeField, HideInInspector] private Vector2 bottomFactor = Vector2.one;
		[SerializeField, HideInInspector] private float curvature = 0f;
		[SerializeField, HideInInspector] private bool smooth = true;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			return new TaperJob
			{
				top = Top,
				bottom = Bottom,
				topFactor = TopFactor * new Vector2 (Axis.lossyScale.x, Axis.lossyScale.y),
				bottomFactor = BottomFactor * new Vector2 (Axis.lossyScale.x, Axis.lossyScale.y),
				curvature = Curvature,
				smooth = Smooth,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct TaperJob : IJobParallelFor
		{
			public float top;
			public float bottom;
			public float2 topFactor;
			public float2 bottomFactor;
			public float curvature;
			public bool smooth;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				if (bottom == top)
				{
					if (point.z > top)
						point.xy *= topFactor;
					else
						point.xy *= bottomFactor;
				}
				else
				{
					var normalizedDistanceBetweenBounds = (clamp (point.z, bottom, top) - bottom) / (top - bottom);
					if (smooth)
						normalizedDistanceBetweenBounds = smoothstep (0f, 1f, normalizedDistanceBetweenBounds);
					var signedDistanceBetweenBounds = (normalizedDistanceBetweenBounds - 0.5f) * 2f;
					var factor = lerp (bottomFactor, topFactor, normalizedDistanceBetweenBounds);
					var curvedFactor = signedDistanceBetweenBounds * signedDistanceBetweenBounds * curvature - curvature + 1f;
					point.xy *= factor * curvedFactor;
				}

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}