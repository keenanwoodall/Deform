using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Scale", Description = "Scales the mesh along an arbitrary axis", Type = typeof (ScaleDeformer), Category = Category.Normal)]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/ScaleDeformer")]
    public class ScaleDeformer : Deformer
	{
		public Transform Axis
		{
			get
			{
				if (axis == null)
					axis = transform;
				return axis;
			}
			set { axis = value; }
		}

		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			return new ScaleJob
			{
				scale = Axis.localScale,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct ScaleJob : IJobParallelFor
		{
			public float3 scale;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				point *= float4 (scale, 1f);

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}