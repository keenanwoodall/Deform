using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Deform
{
	[Deformer (Name = "Inflate", Description = "Moves vertices along normals", Type = typeof (InflateDeformer))]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/InflateDeformer")]
    public class InflateDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = value;
		}
		public bool UseUpdatedNormals
		{
			get => useUpdatedNormals;
			set => useUpdatedNormals = value;
		}

		[SerializeField, HideInInspector] private float factor = 0f;
		[SerializeField, HideInInspector] private bool useUpdatedNormals;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (Factor == 0f)
				return dependency;

			if (UseUpdatedNormals)
				dependency = MeshUtils.RecalculateNormals (data.DynamicNative, dependency);

			return new InflateJob
			{
				factor = Factor,
				vertices = data.DynamicNative.VertexBuffer,
				normals = data.DynamicNative.NormalBuffer,
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct InflateJob : IJobParallelFor
		{
			public float factor;
			public NativeArray<float3> vertices;
			public NativeArray<float3> normals;

			public void Execute (int index)
			{
				vertices[index] += normals[index] * factor;
			}
		}
	}
}