using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Cellular Noise", Description = "Adds cellular noise to mesh", Type = typeof (CellularNoiseDeformer))]
	public class CellularNoiseDeformer : NoiseDeformer, IFactor
	{
		protected override JobHandle CreateDerivativeNoiseJob (MeshData data, JobHandle dependency = default)
		{
			return new DerivativeNoiseJob
			{
				magnitude = GetActualMagnitude (),
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ()),
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, BatchCount, dependency);
		}

		protected override JobHandle CreateDirectionalNoiseJob (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());
			return new DirectionalNoiseJob
			{
				magnitude = MagnitudeScalar,
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				axisSpace = meshToAxis,
				inverseAxisSpace = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer,
				normals = data.DynamicNative.NormalBuffer
			}.Schedule (data.Length, BatchCount, dependency);
		}

		protected override JobHandle CreateNormalNoiseJob (MeshData data, JobHandle dependency = default)
		{
			return new NormalNoiseJob
			{
				magnitude = MagnitudeScalar,
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				axisSpace = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ()),
				vertices = data.DynamicNative.VertexBuffer,
				normals = data.DynamicNative.NormalBuffer
			}.Schedule (data.Length, BatchCount, dependency);
		}

		protected override JobHandle CreateSphericalNoiseJob (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());
			return new SphericalNoiseJob
			{
				magnitude = MagnitudeScalar,
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				axisSpace = meshToAxis,
				inverseAxisSpace = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer,
				normals = data.DynamicNative.NormalBuffer
			}.Schedule (data.Length, BatchCount, dependency);
		}

		protected override JobHandle CreateColorNoiseJob (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());
			return new ColorNoiseJob
			{
				magnitude = MagnitudeScalar,
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				axisSpace = meshToAxis,
				inverseAxisSpace = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer,
				colors = data.DynamicNative.ColorBuffer
			}.Schedule (data.Length, BatchCount, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct DerivativeNoiseJob : IJobParallelFor
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
					remap 
					(
						0f, 1f, -1f, 1f,
						noise.cellular
						(
							float3
							(
								scaledPoint.x - AXIS_OFFSET + offset.x,
								scaledPoint.y - AXIS_OFFSET + offset.y,
								scaledPoint.z - AXIS_OFFSET + offset.z
							)
						).x
					),
					remap
					(
						0f, 1f, -1f, 1f,
						noise.cellular
						(
							float3
							(
								scaledPoint.x + offset.x,
								scaledPoint.y + offset.y,
								scaledPoint.z + offset.z
							)
						).x
					),
					remap
					(
						0f, 1f, -1f, 1f,
						noise.cellular
						(
							float3
							(
								scaledPoint.x + AXIS_OFFSET + offset.x,
								scaledPoint.y + AXIS_OFFSET + offset.y,
								scaledPoint.z + AXIS_OFFSET + offset.z
							)
						).x
					)
				) * magnitude;

				vertices[index] += noiseOffset;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct DirectionalNoiseJob : IJobParallelFor
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

				var noiseOffset = float3 (0f, 0f, 1f) * remap
				(
					0f, 1f, -1f, 1f,
					noise.cellular
					(
						float3
						(
							point.x * frequency.x + offset.x,
							point.y * frequency.y + offset.y,
							point.z * frequency.z + offset.z
						)
					).x
				)* magnitude;

				point += noiseOffset;

				vertices[index] = mul (inverseAxisSpace, float4 (point, 1f)).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct NormalNoiseJob : IJobParallelFor
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

				var noiseOffset = float3 (0f, 0f, 1f) * remap
				(
					0f, 1f, -1f, 1f,
					noise.cellular
					(
						float3
						(
							point.x * frequency.x + offset.x,
							point.y * frequency.y + offset.y,
							point.z * frequency.z + offset.z
						)
					).x
					) * magnitude;

				vertices[index] += noiseOffset;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct SphericalNoiseJob : IJobParallelFor
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

				var noiseOffset = float3 (0f, 0f, 1f) * remap
				(
					0f, 1f, -1f, 1f,
					noise.cellular
					(
						float3
						(
							point.x * frequency.x + offset.x,
							point.y * frequency.y + offset.y,
							point.z * frequency.z + offset.z
						)
					).x
				)* magnitude;

				point += noiseOffset;

				vertices[index] = mul (inverseAxisSpace, float4 (point, 1f)).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		protected struct ColorNoiseJob : IJobParallelFor
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

				var noiseOffset = float3 (0f, 0f, 1f) * remap
				(
					0f, 1f, -1f, 1f,
					noise.cellular
					(
						float3
						(
							point.x * frequency.x + offset.x,
							point.y * frequency.y + offset.y,
							point.z * frequency.z + offset.z
						)
					).x
				) * magnitude;

				point += noiseOffset;

				vertices[index] = mul (inverseAxisSpace, float4 (point, 1f)).xyz;
			}
		}
	}
}