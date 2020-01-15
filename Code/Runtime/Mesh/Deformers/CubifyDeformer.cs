using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Cubify (WIP)", Description = "Morphs mesh into a cube", Type = typeof (CubifyDeformer), Category = Category.WIP)]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/CubifyDeformer")]
    public class CubifyDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = Mathf.Clamp01 (value);
		}
		public float Width
		{
			get => width;
			set => width = value;
		}
		public float Height
		{
			get => height;
			set => height = value;
		}
		public float Length
		{
			get => length;
			set => length = value;
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

		[SerializeField, HideInInspector] private float factor = 0f;
		[SerializeField, HideInInspector] private float width = 1f;
		[SerializeField, HideInInspector] private float height = 1f;
		[SerializeField, HideInInspector] private float length = 1f;
		[SerializeField, HideInInspector] private Transform axis;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (Factor == 0f)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			return new CubifyJob
			{
				factor = Factor,
				width = Width,
				height = Height,
				length = Length,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct CubifyJob : IJobParallelFor
		{
			public float factor;
			public float width;
			public float height;
			public float length;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var halfWidth = width * 0.5f;
				var halfHeight = height * 0.5f;
				var halfLength = length * 0.5f;

				var xDist = point.x / width;
				var yDist = point.y / height;
				var zDist = point.z / length;

				var inRightHalf = xDist > 0f;
				var inTopHalf = yDist > 0f;
				var inFrontHalf = zDist > 0f;

				var goalX = (select (-halfWidth, halfWidth, inRightHalf) / ((factor * 0.5f) + 1f)) * 1.5f;
				var goalY = (select (-halfHeight, halfHeight, inTopHalf) / ((factor * 0.5f) + 1f)) * 1.5f;
				var goalZ = (select (-halfLength, halfLength, inFrontHalf) / ((factor * 0.5f) + 1f)) * 1.5f;

				var xt = abs (xDist) * factor * 2f;
				xt = saturate (xt);
				point.x = lerp (point.x, goalX, xt);

				var yt = abs (yDist) * factor * 2f;
				yt = saturate (yt);
				point.y = lerp (point.y, goalY, yt);

				var zt = abs (zDist) * factor * 2f;
				zt = saturate (zt);
				point.z = lerp (point.z, goalZ, zt);

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}