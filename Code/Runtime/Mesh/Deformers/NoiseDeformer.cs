using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Noise", Description = "Adds noise to mesh", Type = typeof (NoiseDeformer))]
	public class NoiseDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => MagnitudeScalar;
			set => MagnitudeScalar = value;
		}

		public NoiseMode Mode
		{
			get => mode;
			set => mode = value;
		}
		public float MagnitudeScalar
		{
			get => magnitudeScalar;
			set => magnitudeScalar = value;
		}
		public Vector3 MagnitudeVector
		{
			get => magnitudeVector;
			set => magnitudeVector = value;
		}
		public float FrequencyScalar
		{
			get => frequencyScalar;
			set => frequencyScalar = value;
		}
		public Vector3 FrequencyVector
		{
			get => frequencyVector;
			set => frequencyVector = value;
		}
		public Vector4 OffsetVector
		{
			get => offsetVector;
			set => offsetVector = value;
		}
		public float OffsetSpeedScalar
		{
			get => offsetSpeedScalar;
			set => offsetSpeedScalar = value;
		}
		public Vector4 OffsetSpeedVector
		{
			get => offsetSpeedVector;
			set => offsetSpeedVector = value;
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

		[SerializeField, HideInInspector] private NoiseMode mode = NoiseMode.Derivative;
		[SerializeField, HideInInspector] private float magnitudeScalar = 0f;
		[SerializeField, HideInInspector] private Vector3 magnitudeVector = Vector3.one;
		[SerializeField, HideInInspector] private float frequencyScalar = 3f;
		[SerializeField, HideInInspector] private Vector3 frequencyVector = Vector3.one;
		[SerializeField, HideInInspector] private Vector4 offsetVector;
		[SerializeField, HideInInspector] private float offsetSpeedScalar = 1f;
		[SerializeField, HideInInspector] private Vector4 offsetSpeedVector = new Vector4 (0f, 0f, 0f);
		[SerializeField, HideInInspector] private Transform axis;

		protected Vector4 speedOffset;

		public override int BatchCount => 64;
		public override DataFlags DataFlags => DataFlags.Vertices;

		private void Update ()
		{
			speedOffset += OffsetSpeedVector * (OffsetSpeedScalar * Time.deltaTime);
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (MagnitudeScalar == 0f)
				return dependency;

			var actualMagnitude = MagnitudeVector * MagnitudeScalar;
			var actualFrequency = FrequencyVector * FrequencyScalar;
			var actualOffset = speedOffset + OffsetVector;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			switch (Mode)
			{
				default:
					return new DerivativeNoiseDeformJob
					{
						magnitude = actualMagnitude,
						frequency = actualFrequency,
						offset = actualOffset,
						meshToAxis = meshToAxis,
						vertices = data.DynamicNative.VertexBuffer
					}.Schedule (data.Length, BatchCount, dependency);
				case NoiseMode.Directional:
					return new DirectionalNoiseDeformJob
					{
						magnitude = MagnitudeScalar,
						frequency = actualFrequency,
						offset = actualOffset,
						axisSpace = meshToAxis,
						inverseAxisSpace = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer,
						normals = data.DynamicNative.NormalBuffer
					}.Schedule (data.Length, BatchCount, dependency);
				case NoiseMode.Normal:
					return new NormalNoiseDeformJob
					{
						magnitude = MagnitudeScalar,
						frequency = actualFrequency,
						offset = actualOffset,
						axisSpace = meshToAxis,
						vertices = data.DynamicNative.VertexBuffer,
						normals = data.DynamicNative.NormalBuffer
					}.Schedule (data.Length, BatchCount, dependency);
				case NoiseMode.Spherical:
					return new SphericalNoiseDeformJob
					{
						magnitude = MagnitudeScalar,
						frequency = actualFrequency,
						offset = actualOffset,
						axisSpace = meshToAxis,
						inverseAxisSpace = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer,
						normals = data.DynamicNative.NormalBuffer
					}.Schedule (data.Length, BatchCount, dependency);
				case NoiseMode.Color:
					return new ColorNoiseDeformJob
					{
						magnitude = MagnitudeScalar,
						frequency = actualFrequency,
						offset = actualOffset,
						axisSpace = meshToAxis,
						inverseAxisSpace = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer,
						colors = data.DynamicNative.ColorBuffer
					}.Schedule (data.Length, BatchCount, dependency);
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct DerivativeNoiseDeformJob : IJobParallelFor
		{
			private const float AXIS_OFFSET = 1000f;

			public float3 magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 meshToAxis;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				var scaledPoint = point * frequency;
				
				var noiseOffset = float3
				(
					noise.cnoise
					(
						float4
						(
							scaledPoint.x - AXIS_OFFSET + offset.x,
							scaledPoint.y - AXIS_OFFSET + offset.y,
							scaledPoint.z - AXIS_OFFSET + offset.z,
							offset.w
						)
					),
					noise.cnoise
					(
						float4
						(
							scaledPoint.x + offset.x,
							scaledPoint.y + offset.y,
							scaledPoint.z + offset.z,
							offset.w
						)
					),
					noise.cnoise
					(
						float4
						(
							scaledPoint.x + AXIS_OFFSET + offset.x,
							scaledPoint.y + AXIS_OFFSET + offset.y,
							scaledPoint.z + AXIS_OFFSET + offset.z,
							offset.w
						)
					)
				) * magnitude;

				vertices[index] += noiseOffset;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct DirectionalNoiseDeformJob : IJobParallelFor
		{
			public float magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 axisSpace;
			public float4x4 inverseAxisSpace;
			public NativeArray<float3> vertices;
			public NativeArray<float3> normals;

			public void Execute (int index)
			{
				var point = mul (axisSpace, float4 (vertices[index], 1f)).xyz;

				var noiseOffset = float3 (0f, 0f, 1f) * noise.cnoise
				(
					float4
					(
						point.x * frequency.x + offset.x,
						point.y * frequency.y + offset.y,
						point.z * frequency.z + offset.z,
						offset.w
					)
				) * magnitude;

				point += noiseOffset;

				vertices[index] = mul (inverseAxisSpace, float4 (point, 1f)).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct NormalNoiseDeformJob : IJobParallelFor
		{
			public float magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 axisSpace;
			public NativeArray<float3> vertices;
			public NativeArray<float3> normals;

			public void Execute (int index)
			{
				var point = mul (axisSpace, float4 (vertices[index], 1f)).xyz;

				var noiseOffset = normals[index] * noise.cnoise
				(
					float4
					(
						point.x * frequency.x + offset.x,
						point.y * frequency.y + offset.y,
						point.z * frequency.z + offset.z,
						offset.w
					)
				) * magnitude;

				vertices[index] += noiseOffset;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct SphericalNoiseDeformJob : IJobParallelFor
		{
			public float magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 axisSpace;
			public float4x4 inverseAxisSpace;
			public NativeArray<float3> vertices;
			public NativeArray<float3> normals;

			public void Execute (int index)
			{
				var point = mul (axisSpace, float4 (vertices[index], 1f)).xyz;

				var noiseOffset = normalize (point) * noise.cnoise
				(
					float4
					(
						point.x * frequency.x + offset.x,
						point.y * frequency.y + offset.y,
						point.z * frequency.z + offset.z,
						offset.w
					)
				) * magnitude;

				point += noiseOffset;

				vertices[index] = mul (inverseAxisSpace, float4 (point, 1f)).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct ColorNoiseDeformJob : IJobParallelFor
		{
			public float magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 axisSpace;
			public float4x4 inverseAxisSpace;
			public NativeArray<float3> vertices;
			public NativeArray<float4> colors;

			public void Execute (int index)
			{
				var point = mul (axisSpace, float4 (vertices[index], 1f)).xyz;

				var noiseOffset = colors[index].xyz * noise.cnoise
				(
					float4
					(
						point.x * frequency.x + offset.x,
						point.y * frequency.y + offset.y,
						point.z * frequency.z + offset.z,
						offset.w
					)
				) * magnitude;

				point += noiseOffset;

				vertices[index] = mul (inverseAxisSpace, float4 (point, 1f)).xyz;
			}
		}
	}
}