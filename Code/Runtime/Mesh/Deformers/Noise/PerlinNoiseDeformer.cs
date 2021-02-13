using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Perlin Noise", Description = "Adds perlin noise to mesh", Type = typeof (PerlinNoiseDeformer), Category = Category.Noise)]
    [HelpURL ("https://github.com/keenanwoodall/Deform/wiki/PerlinNoiseDeformer")]
	public class PerlinNoiseDeformer : NoiseDeformer, IFactor
	{
		protected override JobHandle Create3DNoiseJob (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());
			return new _3DNoiseJob
			{
				magnitude = GetActualMagnitude (),
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse, 
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		protected override JobHandle CreateDirectionalNoiseJob (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());
			return new DirectionalNoiseJob
			{
				magnitude = MagnitudeScalar,
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer,
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		protected override JobHandle CreateNormalNoiseJob (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());
			return new NormalNoiseJob
			{
				magnitude = MagnitudeScalar,
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer,
				normals = data.DynamicNative.NormalBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		protected override JobHandle CreateSphericalNoiseJob (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());
			return new SphericalNoiseJob
			{
				magnitude = MagnitudeScalar,
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer,
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		protected override JobHandle CreateColorNoiseJob (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());
			return new ColorNoiseJob
			{
				magnitude = MagnitudeScalar,
				frequency = GetActualFrequency (),
				offset = GetActualOffset (),
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer,
				colors = data.DynamicNative.ColorBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct _3DNoiseJob : IJobParallelFor
		{
			public float3 magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				var scaledPoint = point * frequency;
				var nabla = frequency * 0.5f;

				point += float3
				(
					noise.cnoise
					(
						float4
						(
							scaledPoint.x - nabla.x + offset.x,
							scaledPoint.y - nabla.y + offset.y,
							scaledPoint.z - nabla.z + offset.z,
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
							scaledPoint.x + nabla.x + offset.x,
							scaledPoint.y + nabla.y + offset.y,
							scaledPoint.z + nabla.z + offset.z,
							offset.w
						)
					)
				) * magnitude;

				vertices[index] = mul (axisToMesh, float4 (point, 1f)).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct DirectionalNoiseJob : IJobParallelFor
		{
			public float magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				point += float3 (0f, 0f, 1f) * noise.cnoise
				(
					float4
					(
						point.x * frequency.x + offset.x,
						point.y * frequency.y + offset.y,
						point.z * frequency.z + offset.z,
						offset.w
					)
				) * magnitude;

				vertices[index] = mul (axisToMesh, float4 (point, 1f)).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct NormalNoiseJob : IJobParallelFor
		{
			public float magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;
			public NativeArray<float3> normals;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				point += normals[index]* noise.cnoise
				(
					float4
					(
						point.x * frequency.x + offset.x,
						point.y * frequency.y + offset.y,
						point.z * frequency.z + offset.z,
						offset.w
					)
				) * magnitude;
				
				vertices[index] = mul (axisToMesh, float4 (point, 1f)).xyz;;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct SphericalNoiseJob : IJobParallelFor
		{
			public float magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				point += normalize (point) * noise.cnoise
				(
					float4
					(
						point.x * frequency.x + offset.x,
						point.y * frequency.y + offset.y,
						point.z * frequency.z + offset.z,
						offset.w
					)
				) * magnitude;

				vertices[index] = mul (axisToMesh, float4 (point, 1f)).xyz;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct ColorNoiseJob : IJobParallelFor
		{
			public float magnitude;
			public float3 frequency;
			public float4 offset;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;
			public NativeArray<float4> colors;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				point += colors[index].xyz * noise.cnoise
				(
					float4
					(
						point.x * frequency.x + offset.x,
						point.y * frequency.y + offset.y,
						point.z * frequency.z + offset.z,
						offset.w
					)
				) * magnitude;

				vertices[index] = mul (axisToMesh, float4 (point, 1f)).xyz;
			}
		}
	}
}