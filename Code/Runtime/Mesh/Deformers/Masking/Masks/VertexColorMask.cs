using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform.Masking
{
	[Deformer (Name = "Vertex Color Mask", Description = "Masks vertices based on their color", Type = typeof (VertexColorMask), Category = Category.Mask)]
	public class VertexColorMask : Deformer, IFactor
	{
		public enum ColorChannel { R, G, B, A }

		public float Factor
		{
			get => factor;
			set => factor = value;
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
		public ColorChannel Channel
		{
			get => channel;
			set => channel = value;
		}

		[SerializeField, HideInInspector] private float factor;
		[SerializeField, HideInInspector] private float falloff = 1;
		[SerializeField, HideInInspector] private bool invert;
		[SerializeField, HideInInspector] private ColorChannel channel;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			if (invert)
				return new VertexColorJob
				{
					factor = Factor,
					falloff = Falloff,
					channel = Channel,
					currentVertices = data.DynamicNative.VertexBuffer,
					maskVertices = data.DynamicNative.MaskVertexBuffer,
					colors = data.DynamicNative.ColorBuffer,
				}.Schedule (data.length, BatchCount, dependency);
			else
				return new InvertedVertexColorJob
				{
					factor = Factor,
					falloff = Falloff,
					channel = Channel,
					currentVertices = data.DynamicNative.VertexBuffer,
					maskVertices = data.DynamicNative.MaskVertexBuffer,
					colors = data.DynamicNative.ColorBuffer,
				}.Schedule (data.length, BatchCount, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		private struct VertexColorJob : IJobParallelFor
		{
			public float factor;
			public float falloff;
			public ColorChannel channel;

			public NativeArray<float3> currentVertices;
			[ReadOnly]
			public NativeArray<float3> maskVertices;
			[ReadOnly]
			public NativeArray<float4> colors;

			public void Execute (int index)
			{
				var color = colors[index];
				var t = 0f;

				switch (channel)
				{
					case ColorChannel.R:
						t = color.x;
						break;
					case ColorChannel.G:
						t = color.y;
						break;
					case ColorChannel.B:
						t = color.z;
						break;
					case ColorChannel.A:
						t = color.w;
						break;
				}

				t = exp (-falloff * t) * factor;

				currentVertices[index] = lerp (currentVertices[index], maskVertices[index], saturate (t));
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		private struct InvertedVertexColorJob : IJobParallelFor
		{
			public float factor;
			public float falloff;
			public ColorChannel channel;

			public NativeArray<float3> currentVertices;
			[ReadOnly]
			public NativeArray<float3> maskVertices;
			[ReadOnly]
			public NativeArray<float4> colors;

			public void Execute (int index)
			{
				var color = colors[index];
				var t = 0f;

				switch (channel)
				{
					case ColorChannel.R:
						t = color.x;
						break;
					case ColorChannel.G:
						t = color.y;
						break;
					case ColorChannel.B:
						t = color.z;
						break;
					case ColorChannel.A:
						t = color.w;
						break;
				}

				t = 1f - (exp (-falloff * t) * factor);

				currentVertices[index] = lerp (currentVertices[index], maskVertices[index], saturate (t));
			}
		}
	}
}