using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Melt", Description = "Melts mesh onto flat surface", XRotation = -90f, Type = typeof (MeltDeformer))]
	public class MeltDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = value;
		}
		public float Falloff
		{
			get => falloff;
			set => falloff = Mathf.Max (value, 0f);
		}
		public float Radius
		{
			get => radius;
			set => radius = value;
		}
		public bool UseNormals
		{
			get => useNormals;
			set => useNormals = value;
		}
		public bool ClampAtBottom
		{
			get => clampAtBottom;
			set => clampAtBottom = value;
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
		public float VerticalFrequency
		{
			get => verticalFrequency;
			set => verticalFrequency = value;
		}
		public float VerticalMagnitude
		{
			get => verticalMagnitude;
			set => verticalMagnitude = value;
		}
		public float RadialFrequency
		{
			get => radialFrequency;
			set => radialFrequency = value;
		}
		public float RadialMagnitude
		{
			get => radialMagnitude;
			set => radialMagnitude = value;
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
		[SerializeField, HideInInspector] private float falloff = 2f;
		[SerializeField, HideInInspector] private float radius = 1f;
		[SerializeField, HideInInspector] private bool useNormals = false;
		[SerializeField, HideInInspector] private bool clampAtBottom = true;
		[SerializeField, HideInInspector] private float top = 1f;
		[SerializeField, HideInInspector] private float bottom = 0f;
		[SerializeField, HideInInspector] private float verticalFrequency = 1f;
		[SerializeField, HideInInspector] private float verticalMagnitude = 0f;
		[SerializeField, HideInInspector] private float radialFrequency = 1f;
		[SerializeField, HideInInspector] private float radialMagnitude = 0f;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (Mathf.Approximately (Factor, 0f))
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			return new MeltJob
			{
				factor = Factor,
				radius = Radius,
				falloff = Falloff,
				useNormals = UseNormals,
				clampAtBottom = ClampAtBottom,
				top = Top,
				bottom = Bottom,
				verticalFrequency = VerticalFrequency,
				verticalMagnitude = VerticalMagnitude,
				radialFrequency = RadialFrequency,
				radialMagnitude = RadialMagnitude,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer,
				normals = data.DynamicNative.NormalBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct MeltJob : IJobParallelFor
		{
			public float factor;
			public float radius;
			public float falloff;
			public bool useNormals;
			public bool clampAtBottom;
			public float top;
			public float bottom;
			public float verticalFrequency;
			public float verticalMagnitude;
			public float radialFrequency;
			public float radialMagnitude;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;
			public NativeArray<float3> normals;

			public void Execute (int index)
			{
				if (top == bottom)
					return;

				var point = mul (meshToAxis, float4 (vertices[index], 1f));
				var normal = mul (meshToAxis, float4 (normals[index], 1f));

				var range = top - bottom;
				var meltAmount = pow (1f - saturate ((point.z - bottom) / range), falloff) * factor;

				if (useNormals)
					point.xy += normal.xy * meltAmount * radius;
				else
					point.xy += normalize (point.xy) * meltAmount * radius;

				var verticalNoise = noise.snoise (point * verticalFrequency) * verticalMagnitude;
				var verticalNoiseMask = sin (saturate ((point.z - bottom) / range) * (float)PI);
				point.z += verticalNoise * verticalNoiseMask;

				if (clampAtBottom)
					point.z = max (bottom, point.z);

				var radialNoise = noise.snoise (point.xy * radialFrequency) * radialMagnitude * meltAmount;
				point.xy += radialNoise;

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}