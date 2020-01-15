using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Radial Skew (WIP)", Description = "Skews vertices away from axis", Type = typeof (RadialSkewDeformer), Category = Category.WIP)]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/RadialSkewDeformer")]
    public class RadialSkewDeformer : Deformer, IFactor
	{
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
		[SerializeField, HideInInspector] private BoundsMode mode= BoundsMode.Unlimited;
		[SerializeField, HideInInspector] private float top = 0.5f;
		[SerializeField, HideInInspector] private float bottom = -0.5f;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (Mathf.Approximately (Factor, 0f))
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			switch (Mode)
			{
				default:
					return new UnlimitedRadialSkewJob
					{
						factor = Factor,
						meshToAxis = meshToAxis,
						axisToMesh = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer
					}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
				case BoundsMode.Limited:
					return new LimitedRadialSkewJob
					{
						factor = Factor,
						top = top,
						bottom = bottom,
						meshToAxis = meshToAxis,
						axisToMesh = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer
					}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct UnlimitedRadialSkewJob : IJobParallelFor
		{
			public float factor;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				point.xz += (point.y * factor) * normalize (point.xz);

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct LimitedRadialSkewJob : IJobParallelFor
		{
			public float factor;
			public float top;
			public float bottom;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var samplePoint = clamp (point.y, bottom, top);
				point.xz += (samplePoint * factor) * normalize (point.xz);

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}