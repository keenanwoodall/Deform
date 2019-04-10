using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Beans.Unity.Mathematics;

namespace Deform.Masking
{
	[Deformer (Name = "Sphere Mask", Description = "Masks deformation in a sphere", Type = typeof (SphereMask), Category = Category.Mask)]
	public class SphereMask : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = Mathf.Clamp (value, 0f, 1f);
		}
		public float Falloff
		{
			get => falloff;
			set => falloff = Mathf.Max (0f, value);
		}
		public float InnerRadius
		{
			get => innerRadius;
			set => innerRadius = Mathf.Min (value, OuterRadius);
		}
		public float OuterRadius
		{
			get => outerRadius;
			set => outerRadius = Mathf.Max (value, InnerRadius);
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
		[SerializeField, HideInInspector] private float falloff = 10f;
		[SerializeField, HideInInspector] private float innerRadius = 0.5f;
		[SerializeField, HideInInspector] private float outerRadius = 1f;
		[SerializeField, HideInInspector] private bool invert;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			if (Mathf.Approximately (OuterRadius, 0f))
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			if (!Invert)
				return new SphereMaskJob
				{
					factor = Factor,
					innerRadius = InnerRadius * 0.5f,
					outerRadius = OuterRadius * 0.5f,
					meshToAxis = meshToAxis,
					axisToMesh = meshToAxis.inverse,
					currentVertices = data.DynamicNative.VertexBuffer,
					maskVertices = data.DynamicNative.MaskVertexBuffer
				}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
			else
				return new InvertedSphereMaskJob
				{
					factor = Factor,
					innerRadius = InnerRadius * 0.5f,
					outerRadius = OuterRadius * 0.5f,
					meshToAxis = meshToAxis,
					axisToMesh = meshToAxis.inverse,
					currentVertices = data.DynamicNative.VertexBuffer,
					maskVertices = data.DynamicNative.MaskVertexBuffer
				}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct SphereMaskJob : IJobParallelFor
		{
			public float factor;
			public float innerRadius;
			public float outerRadius;

			public float4x4 meshToAxis;
			public float4x4 axisToMesh;

			public NativeArray<float3> currentVertices;
			[ReadOnly]
			public NativeArray<float3> maskVertices;

			public void Execute (int index)
			{
				var meshPoint = currentVertices[index];

				var dist = length (mul (meshToAxis, float4 (meshPoint, 1f)).xyz);

				var t = 0f;

				if (dist > outerRadius)
					t = 0f;
				else if (dist < innerRadius)
					t = 1f;
				else
					t = unlerp (outerRadius, innerRadius, dist);

				t *= factor;

				currentVertices[index] = lerp (meshPoint, maskVertices[index], saturate (t));
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct InvertedSphereMaskJob : IJobParallelFor
		{
			public float factor;
			public float innerRadius;
			public float outerRadius;

			public float4x4 meshToAxis;
			public float4x4 axisToMesh;

			public NativeArray<float3> currentVertices;
			[ReadOnly]
			public NativeArray<float3> maskVertices;

			public void Execute (int index)
			{
				var meshPoint = currentVertices[index];

				var dist = length (mul (meshToAxis, float4 (meshPoint, 1f)).xyz);

				var t = 0f;

				if (dist < innerRadius)
					t = 0f;
				else if (dist > outerRadius)
					t = 1f;
				else
					t = unlerp (innerRadius, outerRadius, dist);

				t *= factor;

				currentVertices[index] = lerp (meshPoint, maskVertices[index], saturate (t));
			}
		}
	}
}