using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using PathCreation;

namespace Deform
{
	[ExecuteAlways]
	[Deformer (Name = "Path", Description = "Orients and stretches mesh along a curve based on each vertices distance along an axis.", Type = typeof (PathDeformer), Category = Category.Normal)]
	public class PathDeformer : Deformer
	{
		public float Scale
		{
			get => scale;
			set => scale = Mathf.Max (value, 0f);
		}
		public float Offset
		{
			get => offset;
			set => offset = value;
		}
		public float Twist
		{
			get => twist;
			set => twist = value;
		}
		public float Speed
		{
			get => speed;
			set => speed = value;
		}
		public PathCreator Path
		{
			get => path;
			set => path = value;
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

		[SerializeField, HideInInspector] private float scale = 1f;
		[SerializeField, HideInInspector] private float offset = 0f;
		[SerializeField, HideInInspector] private float twist = 0f;
		[SerializeField, HideInInspector] private float speed = 0f;
		[SerializeField, HideInInspector] private PathCreator path;
		[SerializeField, HideInInspector] private Transform axis;

		private float speedOffset;
		private JobHandle handle;

		private NativePath pathData;

		public override DataFlags DataFlags => DataFlags.Vertices;

		private void Update ()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
				speedOffset += Speed * Time.deltaTime;
#else
			speedOffset += Speed * Time.deltaTime;
#endif
		}

		private void OnDisable ()
		{
			handle.Complete ();
			pathData.Dispose ();
		}

		public float GetTotalOffset ()
		{
			return Offset + speedOffset;
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			if (Path == null || Path.path.vertices == null)
				return dependency;

			pathData.Update (Path.path);

			var targetTransform = data.Target.GetTransform ();

			var meshToAxis = Axis.worldToLocalMatrix * targetTransform.localToWorldMatrix;
			var splineToWorld = Path.transform.localToWorldMatrix;
			var worldToMesh = targetTransform.worldToLocalMatrix;

			var newHandle = new PathJob
			{
				scale = Scale,
				offset = GetTotalOffset (),
				twist = Twist,
				loop = path.bezierPath.IsClosed,
				meshToAxis = meshToAxis,
				splineToWorld = splineToWorld,
				worldToMesh = worldToMesh,
				splineRotation = Path.transform.rotation,
				spline = pathData,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.length, BatchCount, dependency);

			handle = JobHandle.CombineDependencies (handle, newHandle);

			return newHandle;
		}

		[BurstCompile (CompileSynchronously = COMPILE_SYNCHRONOUSLY)]
		private struct PathJob : IJobParallelFor
		{
			public float scale;
			public float offset;
			public float twist;
			public bool loop;

			public float4x4 meshToAxis;
			public float4x4 splineToWorld;
			public float4x4 worldToMesh;
			public quaternion splineRotation;

			[ReadOnly]
			public NativePath spline;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				var axisPoint = mul (meshToAxis, float4 (vertices[index], 1f)).xyz;

				var t = axisPoint.z * scale + offset;

				var endInstruction = loop ? EndOfPathInstruction.Loop : EndOfPathInstruction.Stop;

				var worldPosition = mul (splineToWorld, float4 (spline.GetPointAtDistance (t, endInstruction), 1f)).xyz;
				var worldDirection = mul (splineRotation, spline.GetDirectionAtDistance (t, endInstruction));
				var worldUp = mul (splineRotation, spline.GetNormalAtDistance (t, endInstruction));
				var worldRight = cross (worldDirection, worldUp);

				var angle = atan2 (axisPoint.x, axisPoint.y) + t * twist;
				var radius = length (axisPoint.xy);

				var worldPoint = worldPosition + (worldRight * cos (angle) + worldUp * sin (angle)) * radius;
				if (any (isnan (worldPoint)) || any (isinf (worldPoint)))
					return;

				vertices[index] = mul (worldToMesh, float4 (worldPoint, 1f)).xyz;
			}
		}
	}
}