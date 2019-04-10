using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Beans.Unity.Mathematics;

namespace Deform.Masking
{
	[Deformer (Name = "Box Mask", Description = "Masks deformation in a box", Type = typeof (BoxMask), Category = Category.Mask)]
	public class BoxMask : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = Mathf.Clamp (value, -1f, 1f);
		}
		public Bounds InnerBounds
		{
			get => innerBounds;
			set => innerBounds = value;
		}
		public Bounds OuterBounds
		{
			get => outerBounds;
			set => outerBounds = value;
		}
		public bool Invert
		{
			get => invert;
			set => invert = value;
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

		[SerializeField, HideInInspector] private float factor;
		[SerializeField, HideInInspector] private Bounds innerBounds = new Bounds (Vector3.zero, Vector3.one * 0.5f);
		[SerializeField, HideInInspector] private Bounds outerBounds = new Bounds (Vector3.zero, Vector3.one);
		[SerializeField, HideInInspector] private bool invert;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			if (!invert)
				return new CubeMaskJob
				{
					factor = Factor,
					innerBounds = InnerBounds,
					outerBounds = OuterBounds,
					meshToAxis = meshToAxis,
					axisToMesh = meshToAxis.inverse,
					currentVertices = data.DynamicNative.VertexBuffer,
					maskVertices = data.DynamicNative.MaskVertexBuffer
				}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
			else
				return new InvertedCubeMaskJob
				{
					factor = Factor,
					innerBounds = InnerBounds,
					outerBounds = OuterBounds,
					meshToAxis = meshToAxis,
					axisToMesh = meshToAxis.inverse,
					currentVertices = data.DynamicNative.VertexBuffer,
					maskVertices = data.DynamicNative.MaskVertexBuffer
				}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct CubeMaskJob : IJobParallelFor
		{
			public float factor;
			public bounds innerBounds;
			public bounds outerBounds;

			public float4x4 meshToAxis;
			public float4x4 axisToMesh;

			public NativeArray<float3> currentVertices;
			[ReadOnly]
			public NativeArray<float3> maskVertices;

			public void Execute (int index)
			{
				var meshPoint = currentVertices[index];
				var point = mul (meshToAxis, float4 (meshPoint, 1f)).xyz;

				var t = 0f;

				if (innerBounds.contains (point))
					t = 1f;
				else
				{
					var innerPoint = innerBounds.closestsurfacepoint (point);
					var outerPoint = outerBounds.closestsurfacepoint (point);

					t = 1f - distance (innerPoint, point) / distance (innerPoint, outerPoint);
				}

				t *= factor;

				currentVertices[index] = lerp (meshPoint, maskVertices[index], saturate (t));
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct InvertedCubeMaskJob : IJobParallelFor
		{
			public float factor;
			public bounds innerBounds;
			public bounds outerBounds;

			public float4x4 meshToAxis;
			public float4x4 axisToMesh;

			public NativeArray<float3> currentVertices;
			[ReadOnly]
			public NativeArray<float3> maskVertices;

			public void Execute (int index)
			{
				var meshPoint = currentVertices[index];
				var point = mul (meshToAxis, float4 (meshPoint, 1f)).xyz;

				var t = 0f;

				if (innerBounds.contains (point))
					t = 0f;
				else
				{
					var innerPoint = innerBounds.closestsurfacepoint (point);
					var outerPoint = outerBounds.closestsurfacepoint (point);

					t = distance (innerPoint, point) / distance (innerPoint, outerPoint);
				}

				t *= factor;

				currentVertices[index] = lerp (meshPoint, maskVertices[index], saturate (t));
			}
		}
	}
}