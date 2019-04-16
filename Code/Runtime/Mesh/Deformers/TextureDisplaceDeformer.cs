using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Beans.Unity.Collections;
using Beans.Unity.Mathematics;

namespace Deform
{
	[ExecuteAlways]
	[Deformer (Name = "Texture Displace", Description = "Displaces mesh based off a texture", Type = typeof (TextureDisplaceDeformer), XRotation = -90f)]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/TextureDisplaceDeformer")]
    public class TextureDisplaceDeformer : Deformer, IFactor
	{
		private const float _1OVER255 = 1f / 255f;

		public float Factor
		{
			get => factor;
			set => factor = value;
		}
		public bool Repeat
		{
			get => repeat;
			set => repeat = value;
		}
		public bool Bilinear
		{
			get => bilinear;
			set => bilinear = value;
		}
		public TextureSampleSpace Space
		{
			get => space;
			set => space = value; 
		}
		public ColorChannel Channel
		{
			get => channel;
			set => channel = value;
		}
		public Vector2 Offset
		{
			get => offset;
			set => offset = value;
		}
		public Vector2 Tiling
		{
			get => tiling;
			set => tiling = value;
		}
		public Texture2D Texture
		{
			get => texture;
			set
			{
				if (value != null)
				{
					texture = value;
					textureDirty = true;
				}
			}
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
		[SerializeField, HideInInspector] private TextureSampleSpace space;
		[SerializeField, HideInInspector] private ColorChannel channel;
		[SerializeField, HideInInspector] private bool repeat;
		[SerializeField, HideInInspector] private bool bilinear;
		[SerializeField, HideInInspector] private Vector2 offset = Vector2.zero;
		[SerializeField, HideInInspector] private Vector2 tiling = Vector2.one;
		[SerializeField, HideInInspector] private Texture2D texture;
		[SerializeField, HideInInspector] private Transform axis;

		private JobHandle handle;
		private Color32[] managedPixels;
		private NativeTexture2D nativeTexture;
		private bool textureDirty = false;

		public override DataFlags DataFlags => DataFlags.Vertices;

		private void OnEnable ()
		{
			textureDirty = true;
		}
		private void OnDisable ()
		{
			handle.Complete ();
			if (nativeTexture.IsCreated)
				nativeTexture.Dispose ();
		}

		public void MarkTextureDataDirty ()
		{
			textureDirty = true;
		}

		/// <summary>
		/// Forces the native texture to update it's data based off of the current texture. Changing the `Texture` property triggers this automatically.
		/// </summary>
		public void ForceUpdateNativeData ()
		{
			if (Texture != null && Texture.isReadable)
			{
				managedPixels = texture.GetPixels32 ();
				nativeTexture.Update (managedPixels, texture.width, texture.height);
			}
			else if (nativeTexture.IsCreated)
				nativeTexture.Dispose ();
			textureDirty = false;
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (textureDirty)
				ForceUpdateNativeData ();

			if (Mathf.Approximately (Factor, 0f) || Texture == null || !nativeTexture.IsCreated)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			JobHandle newHandle;

			switch (Space)
			{
				default:
					if (!Bilinear)
						newHandle = new WorldTextureDisplaceJob
						{
							factor = Factor,
							repeat = Repeat,
							channel = (int)Channel,
							offset = Offset,
							tiling = Tiling,
							direction = Quaternion.Inverse (data.Target.GetTransform ().rotation) * Axis.forward,
							meshToAxis = meshToAxis,
							texture = nativeTexture,
							vertices = data.DynamicNative.VertexBuffer,
							normals = data.DynamicNative.NormalBuffer
						}.Schedule (data.Length, 32, dependency);
					else
						newHandle = new WorldTextureDisplaceBilinearJob
						{
							factor = Factor,
							repeat = Repeat,
							channel = (int)Channel,
							offset = Offset,
							tiling = Tiling,
							direction = Quaternion.Inverse (data.Target.GetTransform ().rotation) * Axis.forward,
							meshToAxis = meshToAxis,
							texture = nativeTexture,
							vertices = data.DynamicNative.VertexBuffer,
							normals = data.DynamicNative.NormalBuffer
						}.Schedule (data.Length, 32, dependency);
					break;
				case TextureSampleSpace.UV:
					if (!Bilinear)
						newHandle = new UVTextureDisplaceJob
						{
							factor = Factor,
							repeat = Repeat,
							channel = (int)Channel,
							offset = Offset,
							tiling = Tiling,
							texture = nativeTexture,
							uvs = data.DynamicNative.UVBuffer,
							vertices = data.DynamicNative.VertexBuffer,
							normals = data.DynamicNative.NormalBuffer
						}.Schedule (data.Length, 32, dependency);
					else
						newHandle = new UVTextureDisplaceBilinearJob
						{
							factor = Factor,
							repeat = Repeat,
							channel = (int)Channel,
							offset = Offset,
							tiling = Tiling,
							texture = nativeTexture,
							uvs = data.DynamicNative.UVBuffer,
							vertices = data.DynamicNative.VertexBuffer,
							normals = data.DynamicNative.NormalBuffer
						}.Schedule (data.Length, 32, dependency);
					break;
			}

			handle = JobHandle.CombineDependencies (handle, newHandle);

			return newHandle;
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct WorldTextureDisplaceJob : IJobParallelFor
		{
			public float factor;
			public bool repeat;
			public int channel;
			public float2 offset;
			public float2 tiling;
			public float3 direction;

			public float4x4 meshToAxis;

			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<float3> normals;
			[ReadOnly]
			public NativeTexture2D texture;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var textureSize = int2 (texture.width, texture.height);

				var samplePosition = (int2)((point.xy + offset) * tiling * textureSize);
				samplePosition += textureSize / 2;

				if (repeat)
					samplePosition = mathx.repeat (samplePosition, textureSize);
				else if (OutsideTexture (samplePosition, textureSize))
					return;

				var color32 = texture.GetPixel (samplePosition.x, samplePosition.y);
				var color = float4 (color32.r * _1OVER255, color32.g * _1OVER255, color32.b * _1OVER255, color32.a * _1OVER255);

				vertices[index] += direction * (color[channel] * factor);
			}

			private bool OutsideTexture (int2 p, int2 size)
			{
				return p.x < 0 || p.y < 0 || p.x >= size.x || p.y >= size.y;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct WorldTextureDisplaceBilinearJob : IJobParallelFor
		{
			public float factor;
			public bool repeat;
			public int channel;
			public float2 offset;
			public float2 tiling;
			public float3 direction;

			public float4x4 meshToAxis;

			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<float3> normals;
			[ReadOnly]
			public NativeTexture2D texture;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var textureSize = int2 (texture.width, texture.height);

				var samplePosition = (point.xy + offset) * tiling * textureSize;
				samplePosition += textureSize / 2;
				samplePosition /= textureSize;

				if (!repeat && OutsideTexture (samplePosition))
					return;

				var color32 = texture.GetPixelBilinear (samplePosition.x, samplePosition.y);
				var color = float4 (color32.r * _1OVER255, color32.g * _1OVER255, color32.b * _1OVER255, color32.a * _1OVER255);

				vertices[index] += direction * (color[channel] * factor);
			}

			private bool OutsideTexture (float2 p)
			{
				return p.x < 0 || p.y < 0 || p.x > 1f || p.y > 1f;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct UVTextureDisplaceJob : IJobParallelFor
		{
			public float factor;
			public bool repeat;
			public int channel;
			public float2 offset;
			public float2 tiling;

			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<float3> normals;
			[ReadOnly]
			public NativeArray<float2> uvs;
			[ReadOnly]
			public NativeTexture2D texture;

			public void Execute (int index)
			{
				var uv = uvs[index];
				var textureSize = int2 (texture.width, texture.height);
				var samplePosition = (int2)((uv + offset) * tiling * textureSize);

				if (repeat)
					samplePosition = mathx.repeat (samplePosition, textureSize);
				else if (OutsideTexture (samplePosition, textureSize))
					return;

				var color32 = texture.GetPixel (samplePosition.x, samplePosition.y);
				var color = float4 (color32.r * _1OVER255, color32.g * _1OVER255, color32.b * _1OVER255, color32.a * _1OVER255);

				vertices[index] += normals[index] * (color[channel] * factor);
			}

			private bool OutsideTexture (int2 p, int2 size)
			{
				return p.x < 0 || p.y < 0 || p.x >= size.x || p.y >= size.y;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct UVTextureDisplaceBilinearJob : IJobParallelFor
		{
			public float factor;
			public bool repeat;
			public int channel;
			public float2 offset;
			public float2 tiling;

			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<float3> normals;
			[ReadOnly]
			public NativeArray<float2> uvs;
			[ReadOnly]
			public NativeTexture2D texture;

			public void Execute (int index)
			{
				var uv = uvs[index];
				var samplePosition = ((uv + offset) * tiling);

				if (!repeat && OutsideTexture (samplePosition))
					return;

				var color32 = texture.GetPixelBilinear (samplePosition.x, samplePosition.y);
				var color = float4 (color32.r * _1OVER255, color32.g * _1OVER255, color32.b * _1OVER255, color32.a * _1OVER255);

				vertices[index] += normals[index] * (color[channel] * factor);
			}

			private bool OutsideTexture (float2 p)
			{
				return p.x < 0 || p.y < 0 || p.x > 1f || p.y > 1f;
			}
		}
	}
}