using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform.Masking
{
	[Deformer (Name = "Vertical Gradient Mask", Description = "Mask vertices based on distance along an axis", Type = typeof (VerticalGradientMask), Category = Category.Mask, XRotation = -90f)]
    [HelpURL ("https://github.com/keenanwoodall/Deform/wiki/VerticalGradientMask")]
    public class VerticalGradientMask : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = Mathf.Clamp (value, -1f, 1f);
		}
		public float Falloff
		{
			get => falloff;
			set => falloff = value;
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
		[SerializeField, HideInInspector] private bool invert;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			if (!invert)
				return new VerticalGradientJob
				{
					factor = Factor,
					falloff = Falloff,
					meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ()),
					currentVertices = data.DynamicNative.VertexBuffer,
					maskVertices = data.DynamicNative.MaskVertexBuffer
				}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
			else
				return new InvertedVerticalGradientJob
				{
					factor = Factor,
					falloff = Falloff,
					meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ()),
					currentVertices = data.DynamicNative.VertexBuffer,
					maskVertices = data.DynamicNative.MaskVertexBuffer
				}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct VerticalGradientJob : IJobParallelFor
		{
			public float factor;
			public float falloff;

			public float4x4 meshToAxis;

			public NativeArray<float3> currentVertices;
			[ReadOnly]
			public NativeArray<float3> maskVertices;

			public void Execute (int index)
			{
				var meshPoint = currentVertices[index];
				var point = mul (meshToAxis, float4 (meshPoint, 1f)).xyz;

				var t = saturate (exp (-falloff * point.z) * factor);

				currentVertices[index] = lerp (meshPoint, maskVertices[index], t);
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct InvertedVerticalGradientJob : IJobParallelFor
		{
			public float factor;
			public float falloff;

			public float4x4 meshToAxis;

			public NativeArray<float3> currentVertices;
			[ReadOnly]
			public NativeArray<float3> maskVertices;

			public void Execute (int index)
			{
				var meshPoint = currentVertices[index];
				var point = mul (meshToAxis, float4 (meshPoint, 1f)).xyz;

				var t = saturate (1f - ( exp (-falloff * point.z) * factor));

				currentVertices[index] = lerp (meshPoint, maskVertices[index], t);
			}
		}
	}
}