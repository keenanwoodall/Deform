using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Beans.Unity.Collections;

namespace Deform
{
	[ExecuteAlways]
	[Deformer (Name = "Texture Displace", Description = "Displaces mesh based off a texture", Type = typeof (TextureDisplaceDeformer), XRotation = -90f)]
	public class TextureDisplaceDeformer : Deformer, IFactor
	{
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
		public TextureSampleMode Mode
		{
			get => mode;
			set => mode = value; 
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
				texture = value;
				Initialize ();
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
		[SerializeField, HideInInspector] private TextureSampleMode mode;
		[SerializeField, HideInInspector] private bool repeat;
		[SerializeField, HideInInspector] private Vector2 offset = Vector2.zero;
		[SerializeField, HideInInspector] private Vector2 tiling = Vector2.one;
		[SerializeField, HideInInspector] private Texture2D texture;
		[SerializeField, HideInInspector] private Transform axis;

		private JobHandle handle;
		private Color[] managedPixels;
		private NativeArray<float4> nativePixels;

		public override int BatchCount => 32;
		public override DataFlags DataFlags => DataFlags.Vertices;

		public bool Initialize ()
		{
			if (nativePixels.IsCreated)
				nativePixels.Dispose ();

			if (Texture == null || !Texture.isReadable)
				return false;

			managedPixels = Texture.GetPixels ();

			if (nativePixels.Length != managedPixels.Length)
			{
				if (nativePixels.IsCreated)
					nativePixels.Dispose ();
				nativePixels = new NativeArray<float4> (managedPixels.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			}

			managedPixels.MemCpy (nativePixels);

			return true;
		}

		private void OnEnable ()
		{
			Initialize ();
		}
		private void OnDisable ()
		{
			handle.Complete ();
			if (nativePixels.IsCreated)
				nativePixels.Dispose ();
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (!nativePixels.IsCreated)
				if (!Initialize ())
					return dependency;
			if (Factor == 0f || Texture == null || !nativePixels.IsCreated)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			JobHandle newHandle;

			switch (Mode)
			{
				default:
					newHandle = new WorldTextureDisplaceJob
					{
						factor = Factor,
						repeat = Repeat,
						offset = Offset,
						tiling = Tiling,
						width = Texture.width,
						height = Texture.height,
						direction = Quaternion.Inverse (data.Target.GetTransform ().rotation) * Axis.forward,
						meshToAxis = meshToAxis,
						pixels = nativePixels,
						vertices = data.DynamicNative.VertexBuffer,
						normals = data.DynamicNative.NormalBuffer
					}.Schedule (data.length, BatchCount, dependency);
					break;
				case TextureSampleMode.UV:
					newHandle = new UVTextureDisplaceJob
					{
						factor = Factor,
						repeat = Repeat,
						offset = Offset,
						tiling = Tiling,
						width = Texture.width,
						height = Texture.height,
						pixels = nativePixels,
						uvs = data.DynamicNative.UVBuffer,
						vertices = data.DynamicNative.VertexBuffer,
						normals = data.DynamicNative.NormalBuffer
					}.Schedule (data.length, BatchCount, dependency);
					break;
			}

			handle = JobHandle.CombineDependencies (handle, newHandle);

			return newHandle;
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		private struct WorldTextureDisplaceJob : IJobParallelFor
		{
			public float factor;
			public bool repeat;
			public float2 offset;
			public float2 tiling;
			public int width;
			public int height;
			public float3 direction;

			public float4x4 meshToAxis;

			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<float3> normals;
			[ReadOnly]
			public NativeArray<float4> pixels;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var textureSize = int2 (width, height);

				var samplePosition = (int2)((point.xy + offset) * tiling * textureSize);
				samplePosition += textureSize / 2;

				if (!repeat && outsidetexture (samplePosition, textureSize))
					return;

				samplePosition = repeatsample (samplePosition, textureSize);

				var pixelIndex = (samplePosition.x + samplePosition.y * width);
				var color = pixels[pixelIndex];
				var strength = length (color.xyz);

				vertices[index] += direction * (strength * factor);
			}

			private int2 repeatsample (int2 p, int2 size)
			{
				while (p.x < 0)
					p.x += size.x;
				while (p.y < 0)
					p.y += size.y;

				p.x %= size.x - 1;
				p.y %= size.y - 1;

				return p;
			}

			private bool outsidetexture (int2 p, int2 size)
			{
				return p.x < 0 || p.y < 0 || p.x >= size.x || p.y >= size.y;
			}
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		private struct UVTextureDisplaceJob : IJobParallelFor
		{
			public float factor;
			public bool repeat;
			public float2 offset;
			public float2 tiling;
			public int width;
			public int height;

			public NativeArray<float3> vertices;
			[ReadOnly]
			public NativeArray<float3> normals;
			[ReadOnly]
			public NativeArray<float2> uvs;
			[ReadOnly]
			public NativeArray<float4> pixels;

			public void Execute (int index)
			{
				var uv = uvs[index];
				var textureSize = int2 (width, height);

				var samplePosition = (int2)((uv + offset) * tiling * textureSize);

				if (!repeat)
					if (outsidetexture (samplePosition, textureSize))
						return;

				samplePosition = repeatsample (samplePosition, textureSize);

				var pixelIndex = (samplePosition.x + samplePosition.y * width);
				var color = pixels[pixelIndex];

				vertices[index] += normals[index] * (length (color.xyz) * factor);
			}

			private int2 repeatsample (int2 p, int2 size)
			{
				while (p.x < 0)
					p.x += size.x;
				while (p.y < 0)
					p.y += size.y;

				p.x %= size.x - 1;
				p.y %= size.y - 1;

				return p;
			}

			private bool outsidetexture (int2 p, int2 size)
			{
				return p.x < 0 || p.y < 0 || p.x >= size.x || p.y >= size.y;
			}
		}
	}
}