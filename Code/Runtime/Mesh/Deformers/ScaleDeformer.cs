﻿using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Scale", Description = "Scales the mesh along an arbitrary axis", Type = typeof (ScaleDeformer), Category = Category.Normal)]
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

			return new ScaleDeformJob
			{
				scale = Axis.localScale,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.length, BatchCount, dependency);
		}

		private struct ScaleDeformJob : IJobParallelFor
		{
			public float3 scale;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				point *= scale;

				vertices[index] = mul (axisToMesh, float4 (point, 1f)).xyz;
			}
		}
	}
}