using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Squash and Stretch", Description = "Squashes and stretches a mesh", XRotation = -90f, Type = typeof (SquashAndStretchDeformer))]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/SquashAndStretchDeformer")]
    public class SquashAndStretchDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = value;
		}
		public float Curvature
		{
			get => curvature;
			set => curvature = Mathf.Clamp01 (value);
		}
		public float Top
		{
			get => top;
			set => top = Mathf.Max (value, Bottom);
		}
		public float Bottom
		{
			get => bottom;
			set => bottom = Mathf.Min (value, Top);
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
		[SerializeField, HideInInspector] private float curvature = 1f;
		[SerializeField, HideInInspector] private float top = 0.5f;
		[SerializeField, HideInInspector] private float bottom = -0.5f;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			if (Factor == 0f || Top == Bottom)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			return new SquashAndStretchJob
			{
				factor = Factor,
				curvature = (Curvature >= 0f) ? Curvature + 1f : 1f / (-Curvature + 1f),
				top = Top,
				bottom = Bottom,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct SquashAndStretchJob : IJobParallelFor
		{
			public float factor;
			public float curvature;
			public float top;
			public float bottom;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var range = abs (top - bottom);
				var inverseRange = 1f / range;

				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var nDist = 0f;

				if (point.z > top)
					nDist = range * inverseRange;
				else if (point.z < bottom)
					nDist = (bottom - bottom) * inverseRange;
				else
					nDist = (point.z - bottom) * inverseRange;

				var squashAmount = 0f;
				var stretchAmount = 0f;

				if (factor > 0f)
				{
					squashAmount = 1f / (curvature * factor + 1f);
					stretchAmount = factor + 1f;
				}
				else
				{
					squashAmount = (curvature * -factor + 1f);
					stretchAmount = -1f / (factor - 1f);
				}

				var f = 4f * (1f - squashAmount);
				squashAmount = (((f * nDist) - f) * nDist) + 1f;

				point.xy *= squashAmount;

				if (point.z < bottom)
					point.z += (stretchAmount - 1f) * bottom;
				else if (point.z <= top)
					point.z *= stretchAmount;
				else if (point.z > top)
					point.z += (stretchAmount - 1f) * top;
				else
					point.z *= stretchAmount;

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}