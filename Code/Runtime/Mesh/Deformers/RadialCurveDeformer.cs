using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Beans.Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Deform
{
	[ExecuteAlways]
	[Deformer (Name = "Radial Curve", Description = "Moves vertices up based on distance from point along a curve (similar to ripple)", Type = typeof (RadialCurveDeformer), XRotation = -90f)]
	public class RadialCurveDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => factor;
			set => factor = value;
		}
		public float Offset
		{
			get => offset;
			set => offset = value;
		}
		public float Falloff
		{
			get => falloff;
			set => falloff = Mathf.Max (0f, falloff);
		}
		public AnimationCurve Curve
		{
			get => curve;
			set => curve = value;
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


		[SerializeField, HideInInspector] private float factor = 1f;
		[SerializeField, HideInInspector] private float offset = 0f;
		[SerializeField, HideInInspector] private float falloff = 0f;
		[SerializeField, HideInInspector] private AnimationCurve curve;
		[SerializeField, HideInInspector] private Transform axis;

		private JobHandle combinedHandle;
		private NativeCurve nativeCurve;

		public override DataFlags DataFlags => DataFlags.Vertices;

		private void OnDisable ()
		{
			combinedHandle.Complete ();
			if (nativeCurve.IsCreated)
				nativeCurve.Dispose ();
		}

		public override void PreProcess ()
		{
			if (curve != null)
				nativeCurve.Update (curve, 32);
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			if (!nativeCurve.IsCreated || curve == null)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			var newHandle = new RadialCurveJob
			{
				factor = Factor,
				offset = Offset,
				falloff = Falloff,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				curve = nativeCurve,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, 128, dependency);

			combinedHandle = JobHandle.CombineDependencies (combinedHandle, newHandle);

			return newHandle;
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct RadialCurveJob : IJobParallelFor
		{
			public float factor;
			public float offset;
			public float falloff;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			[ReadOnly]
			public NativeCurve curve;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var dist = length (point.xy);

				point.z += curve.Evaluate (dist + offset) * factor * (1f / (pow (dist + 1f, falloff * 2f)));

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}