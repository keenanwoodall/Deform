using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[ExecuteAlways]
	[Deformer(Name = "Point Cache", Description = "Allows playback of a point cache on a mesh.", Category = Category.Normal, Type = typeof(PointCacheDeformer))]
	public class PointCacheDeformer : Deformer
	{
		[SerializeField]
		private PointCache pointCache;

		[SerializeField, Range(1, 48)]
		private int frameIndex = 1;

		public PointCache PointCache => pointCache;

		private NativeArray<Vector3> framePoints;
		private JobHandle handle;
		
		public override DataFlags DataFlags => DataFlags.Vertices;
		
		private void OnEnable()
		{
			if (pointCache != null)
				framePoints = new NativeArray<Vector3>(pointCache.Points, Allocator.Persistent);
		}
		
		private void OnDisable()
		{
			if (framePoints.IsCreated)
			{
				handle.Complete();
				framePoints.Dispose();
			}
		}
		
		public override void PreProcess()
		{
			if (pointCache != null)
			{
				if (!framePoints.IsCreated)
					framePoints = new NativeArray<Vector3>(pointCache.Points, Allocator.Persistent);
				else if (framePoints.Length != pointCache.Points.Length)
				{
					framePoints.Dispose();
					framePoints = new NativeArray<Vector3>(pointCache.Points, Allocator.Persistent);
				}
			}
		}

		public override JobHandle Process(MeshData data, JobHandle dependency = default)
		{
			if (pointCache == null)
				return dependency;
			return handle = new PointCacheJob
			{
				frameSize = pointCache.FrameSize,
				frameIndex = frameIndex - 1,
				framePoints = framePoints.Reinterpret<float3>(),
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule(data.Length, DEFAULT_BATCH_COUNT, dependency);
		}
		
		[BurstCompile(CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct PointCacheJob : IJobParallelFor
		{
			public int frameIndex;
			public int frameSize;

			[ReadOnly]
			public NativeArray<float3> framePoints;
			[WriteOnly]
			public NativeArray<float3> vertices;

			public void Execute(int index)
			{
				vertices[index] = framePoints[frameIndex * frameSize + index];
			}
		}
	}
}