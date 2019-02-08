using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Sine", Description = "Moves vertices in direction based on distance along sine wave", Type = typeof (SineDeformer))]
	public class SineDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => Magnitude;
			set => Magnitude = value;
		}

		public float Frequency
		{
			get => frequency;
			set => frequency = value;
		}
		public float Magnitude
		{
			get => magnitude;
			set => magnitude = value;
		}
		public float Falloff
		{
			get => falloff;
			set => falloff = Mathf.Max (0f, value);
		}
		public float Offset
		{
			get => offset;
			set => offset = value;
		}
		public float Speed
		{
			get => speed;
			set => speed = value;
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

		[SerializeField, HideInInspector] private float frequency = 1f;
		[SerializeField, HideInInspector] private float magnitude = 0f;
		[SerializeField, HideInInspector] private float falloff = 0f;
		[SerializeField, HideInInspector] private float offset;
		[SerializeField, HideInInspector] private float speed;
		[SerializeField, HideInInspector] private Transform axis;

		[SerializeField, HideInInspector] private float speedOffset;

		public override DataFlags DataFlags => DataFlags.Vertices;

		private void Update ()
		{
			speedOffset += speed * Time.deltaTime;
		}

		public float GetTotalOffset ()
		{
			return Offset + speedOffset;
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (Magnitude == 0f)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			return new SinJob
			{
				frequency = Frequency,
				magnitude = Magnitude,
				falloff = Falloff,
				offset = GetTotalOffset (),
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, BatchCount, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		private struct SinJob : IJobParallelFor
		{
			public float frequency;
			public float magnitude;
			public float falloff;
			public float offset;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));
				var amount = sin ((point.z * frequency + offset) * Mathf.PI * 2f) * magnitude;
				amount *= exp (-falloff * abs (point.z));
				point.y += amount;
				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}