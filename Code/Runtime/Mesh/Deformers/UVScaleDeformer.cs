using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Deform
{
	[Deformer (Name = "UV Scale", Description = "Scales the mesh's UVs", Type = typeof (UVScaleDeformer), Category = Category.Normal)]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/UVScaleDeformer")]
    public class UVScaleDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = value;
		}
		public Vector2 Scale
		{
			get => scale;
			set => scale = value;
		}

		[SerializeField, HideInInspector] private float factor = 1f;
		[SerializeField, HideInInspector] private Vector2 scale = Vector2.one;

		public override DataFlags DataFlags => DataFlags.UVs;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			return new UVScaleJob
			{
				scale = Scale * Factor,
				uvs = data.DynamicNative.UVBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct UVScaleJob : IJobParallelFor
		{
			public float2 scale;
			public NativeArray<float2> uvs;

			public void Execute (int index)
			{
				uvs[index] *= scale;
			}
		}
	}
}