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
	[Deformer (Name ="Curve Displace", Description = "Pushes vertices in a direction based on distance along a curve", Type = typeof (CurveDisplaceDeformer))]
	public class CurveDisplaceDeformer : Deformer, IFactor
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
			if (curve != null && curve.length > 0)
				nativeCurve.Update (curve, 32);
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			if (!nativeCurve.IsCreated || curve == null || curve.length == 0)
				return dependency;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			var newHandle = new CurveDisplaceJob
			{
				factor = Factor,
				offset = Offset,
				firstKeyTime = Curve.keys[0].time,
				lastKeyTime = Curve.keys[Curve.length - 1].time,
				meshToAxis = meshToAxis,
				axisToMesh = meshToAxis.inverse,
				curve = nativeCurve,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, 128, dependency);

			combinedHandle = JobHandle.CombineDependencies (combinedHandle, newHandle);

			return newHandle;
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		public struct CurveDisplaceJob : IJobParallelFor
		{
			public float factor;
			public float offset;
			public float firstKeyTime;
			public float lastKeyTime;
			public float4x4 meshToAxis;
			public float4x4 axisToMesh;
			[ReadOnly]
			public NativeCurve curve;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var point = mul (meshToAxis, float4 (vertices[index], 1f));

				var t = point.z + offset;
				var curvePoint = curve.Evaluate (t);
				point.y += curvePoint * factor;

				vertices[index] = mul (axisToMesh, point).xyz;
			}
		}
	}
}