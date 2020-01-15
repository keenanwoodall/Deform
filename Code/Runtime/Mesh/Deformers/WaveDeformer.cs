using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Wave (WIP)", Description = "Moves vertices up and down based on distance along a gerstner wave", Type = typeof (WaveDeformer), Category = Category.WIP)]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/WaveDeformer")]
    public class WaveDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => Steepness;
			set => Steepness = Factor;
		}

		public float WaveLength
		{
			get => waveLength;
			set => waveLength = Mathf.Clamp (value, 0f, float.MaxValue);
		}
		public float Steepness
		{
			get => steepness;
			set => steepness = Mathf.Clamp01 (value);
		}
		public float Speed
		{
			get => speed;
			set => speed = value;
		}
		public float Offset
		{
			get => offset;
			set => offset = value;
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

		[SerializeField, HideInInspector] private float waveLength = 1f;
		[SerializeField, HideInInspector] private float steepness = 0f;
		[SerializeField, HideInInspector] private float speed = 1f;
		[SerializeField, HideInInspector] private float offset = 0f;
		[SerializeField, HideInInspector] private Transform axis;

		private float speedOffset;

		public override DataFlags DataFlags => DataFlags.Vertices;

		private void Update ()
		{
			speedOffset += Speed * Time.deltaTime;
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			if (waveLength <= 0f)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			return new WaveJob
			{
				waveLength = WaveLength,
				steepness = Steepness,
				offset = GetTotalOffset (),
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		public float GetTotalOffset ()
		{
			return Offset + speedOffset;
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct WaveJob : IJobParallelFor
		{
			public float waveLength;
			public float steepness;
			public float offset;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				float a = point.z;
				float k = 2f * (float)PI / waveLength;
				float c = sqrt (1f / k);
				float b = steepness - 1f;

				point.z += exp (k * b) / k * sin (k * (a + c * offset));
				point.y += -exp (k * b) / k * cos (k * (a + c * offset));

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}