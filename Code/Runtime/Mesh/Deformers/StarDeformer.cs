using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[Deformer (Name = "Star", Description = "Moves vertices away from axis based on angle around axis on a sine wave", Type = typeof (StarDeformer))]
	public class StarDeformer : Deformer, IFactor
	{
		public float Factor { get => Magnitude; set => Magnitude = value; }

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

		[SerializeField, HideInInspector] private float frequency = 5f;
		[SerializeField, HideInInspector] private float magnitude = 0f;
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

			return new StarJob
			{
				frequency = Frequency,
				magnitude = Magnitude,
				offset = GetTotalOffset (),
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, DEFAULT_BATCH_COUNT, dependency);
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct StarJob : IJobParallelFor
		{
			public float frequency;
			public float magnitude;
			public float offset;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				if (length (point.xy) == 0f)
					return;

				var npoint = normalize (point.xy);
				var angle = atan2 (npoint.y, npoint.x);
				var amount = sin ((frequency * angle) + offset) * magnitude * length (point.xy);

				point.xy += npoint.xy * amount;

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}