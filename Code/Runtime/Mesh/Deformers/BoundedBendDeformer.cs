using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Beans.Unity.Mathematics;

namespace Deform
{
	[Deformer (Name = "Bounded Bend (WIP)", Description = "Bends a mesh within a bounding box", Type = typeof (BoundedBendDeformer), Category = Category.WIP)]
    public class BoundedBendDeformer : Deformer, IFactor
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
		public BoundsMode Mode
		{
			get => mode;
			set => mode = value;
		}
		public Bounds Bounds
		{
			get => bounds;
			set => bounds = value;
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
		[SerializeField, HideInInspector] private BoundsMode mode = BoundsMode.Limited;
		[SerializeField, HideInInspector] Bounds bounds;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			var totalAngle = Angle * Factor;
			if (Mathf.Approximately (totalAngle, 0f) || Mathf.Approximately (Bounds.size.y, 0f))
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			switch (mode)
			{
				default:
				case BoundsMode.Unlimited:
					return new UnlimitedBendJob
					{
						angle = totalAngle,
						bounds = Bounds,
						meshToAxis = meshToAxis,
						axisToMesh = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer
					}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
				case BoundsMode.Limited:
					return new LimitedBendJob
					{
						angle = totalAngle,
						bounds = Bounds,
						meshToAxis = meshToAxis,
						axisToMesh = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer
					}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct UnlimitedBendJob : IJobParallelFor
		{
			public float angle;
			public bounds bounds;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var angleRadians = radians (angle) * (1f / (bounds.max.y - bounds.min.y));
				var scale = 1f / angleRadians;
				var rotation = point.y * angleRadians;

				var c = cos ((float)PI - rotation);
				var s = sin ((float)PI - rotation);
				point.xy = float2
				(
					(scale * c) + scale - (point.x * c),
					(scale * s) - (point.x * s)
				);

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct LimitedBendJob : IJobParallelFor
		{
			public float angle;
			public bounds bounds;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				if (point.x > bounds.max.x || point.x < bounds.min.x || point.z > bounds.max.z || point.z < bounds.min.z || point.y < bounds.min.y)
					return;

				var unbentPoint = point;
				var top = bounds.max.y;
				var bottom = bounds.min.y;

				var angleRadians = radians (angle);
				var scale = 1f / (angleRadians * (1f / (top - bottom)));
				var rotation = (clamp (point.y, bottom, top) - bottom) / (top - bottom) * angleRadians;

				var c = cos ((float)PI - rotation);
				var s = sin ((float)PI - rotation);
				point.xy = float2 
				(
					(scale * c) + scale - (point.x * c),
					(scale * s) - (point.x * s)
				);

				if (unbentPoint.y > top)
				{
					point.y += -c * (unbentPoint.y - top);
					point.x += s * (unbentPoint.y - top);
				}
				else if (unbentPoint.y < bottom)
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