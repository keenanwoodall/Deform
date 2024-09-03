using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Serialization;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Bend", Description = "Bends a mesh", Type = typeof (BendDeformer))]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/BendDeformer")]
    public class BendDeformer : Deformer, IFactor
	{
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
		public BoundsMode BottomMode
		{
			get => bottomMode;
			set => bottomMode = value;
		}
		public float Top
		{
			get => top;
			set => top = Mathf.Max (value, bottom);
		}
		public BoundsMode TopMode
		{
			get => topMode;
			set => topMode = value;
		}
		public float Bottom
		{
			get => bottom;
			set => bottom = Mathf.Min (value, top);
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

		[SerializeField, HideInInspector] private float angle;
		[SerializeField, HideInInspector] private float factor = 1f;
		[SerializeField, HideInInspector] private float top = 1f;
		[FormerlySerializedAs("mode")]
		[SerializeField, HideInInspector] private BoundsMode topMode = BoundsMode.Limited;
		[SerializeField, HideInInspector] private float bottom = 0f;
		[FormerlySerializedAs("mode")]
		[SerializeField, HideInInspector] private BoundsMode bottomMode = BoundsMode.Limited;
		[SerializeField, HideInInspector] private Transform axis;

		[HideInInspector]
		public float minValidBendAngle = 1e-03f;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			var totalAngle = Angle * Factor;
			if (Mathf.Abs(totalAngle) < minValidBendAngle || Mathf.Approximately (Top, Bottom))
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			return new BendJob
			{
				angle = totalAngle,
				top = Top,
				limitTop = TopMode == BoundsMode.Limited,
				bottom = Bottom,
				limitBottom = BottomMode == BoundsMode.Limited,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct UnlimitedBendJob : IJobParallelFor
		{
			public float angle;
			public float top;
			public float bottom;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var angleRadians = radians (angle) * (1f / (top - bottom));
				var scale = 1f / angleRadians;
				var rotation = point.y * angleRadians;

				var c = cos (PI - rotation);
				var s = sin (PI - rotation);
				point.xy = float2
				(
					scale * c + scale - point.x * c,
					scale * s - point.x * s
				);

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct BendJob : IJobParallelFor
		{
			public float angle;
			public float top;
			public float bottom;
			public bool limitTop;
			public bool limitBottom;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var unbentPoint = point;

				var angleRadians = radians (angle);
				var scale = 1f / (angleRadians * (1f / (top - bottom)));
				var limitedY = point.y;
				if (limitTop)
					limitedY = min(top, limitedY);
				if (limitBottom)
					limitedY = max(bottom, limitedY);
				var rotation = (limitedY - bottom) / (top - bottom) * angleRadians;

				var c = cos (PI - rotation);
				var s = sin (PI - rotation);
				point.xy = float2 
				(
					scale * c + scale - point.x * c,
					scale * s - point.x * s
				);

				if (limitTop && unbentPoint.y > top)
				{
					point.y += -c * (unbentPoint.y - top);
					point.x += s * (unbentPoint.y - top);
				}
				
				else if (limitBottom && unbentPoint.y < bottom)
				{
					point.y += -c * (unbentPoint.y - bottom);
					point.x += s * (unbentPoint.y - bottom);
				}

				point.y += bottom;

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}