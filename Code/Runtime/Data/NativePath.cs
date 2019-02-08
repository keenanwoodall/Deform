using System;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Beans.Unity.Mathematics;
using Beans.Unity.Collections;
using PathCreation;

namespace Deform
{
	public struct NativePath : IDisposable
	{
		public bool IsCreated { get => points.IsCreated; }

		private NativeArray<float3> points;
		private NativeArray<float3> normals;
		private NativeArray<float3> tangents;
		private NativeArray<float> times;

		private float length;
		private int vertexCount;

		private bool disposed;

		public void Update (VertexPath path, Allocator allocator = Allocator.Persistent)
		{
			vertexCount = path.NumVertices;
			length = path.length;

			var currentVertCount = disposed ? -1 : points.Length;

			if (disposed || currentVertCount != vertexCount)
			{
				if (!disposed)
					Dispose ();

				points		= new NativeArray<float3> (vertexCount, allocator, NativeArrayOptions.UninitializedMemory);
				normals		= new NativeArray<float3> (vertexCount, allocator, NativeArrayOptions.UninitializedMemory);
				tangents	= new NativeArray<float3> (vertexCount, allocator, NativeArrayOptions.UninitializedMemory);
				times		= new NativeArray<float> (vertexCount, allocator, NativeArrayOptions.UninitializedMemory);

				disposed = false;
			}

			path.vertices.MemCpy (points);
			path.normals.MemCpy (normals);
			path.tangents.MemCpy (tangents);
			path.times.MemCpy (times);
		}

		public float3 GetPointAtDistance (float distance, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
		{
			var t = distance / length;
			return GetPoint (t, endOfPathInstruction);
		}

		public float3 GetDirectionAtDistance (float distance, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
		{
			var t = distance / length;
			return GetDirection (t, endOfPathInstruction);
		}

		public float3 GetNormalAtDistance (float distance, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
		{
			var t = distance / length;
			return GetNormal (t, endOfPathInstruction);
		}

		public quaternion GetRotationAtDistance (float distance, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
		{
			var t = distance / length;
			return GetRotation (t, endOfPathInstruction);
		}

		public float3 GetPoint (float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
		{
			var data = CalculatePercentOnPathData (t, endOfPathInstruction);
			return lerp (points[data.previousIndex], points[data.nextIndex], data.percentBetweenIndices);
		}

		public float3 GetDirection (float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
		{
			var data = CalculatePercentOnPathData (t, endOfPathInstruction);
			return lerp (tangents[data.previousIndex], tangents[data.nextIndex], data.percentBetweenIndices);
		}

		public float3 GetNormal (float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
		{
			var data = CalculatePercentOnPathData (t, endOfPathInstruction);
			return lerp (normals[data.previousIndex], normals[data.nextIndex], data.percentBetweenIndices);
		}

		public quaternion GetRotation (float t, EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Loop)
		{
			var data = CalculatePercentOnPathData (t, endOfPathInstruction);
			var direction = lerp (tangents[data.previousIndex], tangents[data.nextIndex], data.percentBetweenIndices);
			var normal = lerp (normals[data.previousIndex], normals[data.nextIndex], data.percentBetweenIndices);

			return mathx.lookrot (direction, normal);
		}

		private VertexPath.TimeOnPathData CalculatePercentOnPathData (float t, EndOfPathInstruction endOfPathInstruction)
		{
			switch (endOfPathInstruction)
			{
				case EndOfPathInstruction.Loop:
					if (t < 0f)
						t += ceil (abs (t));
					t %= 1f;
					break;
				case EndOfPathInstruction.Reverse:
					t = mathx.pingpong (t, 1f);
					break;
				case EndOfPathInstruction.Stop:
					t = saturate (t);
					break;
			}

			var prevIndex = 0;
			var nextIndex = vertexCount - 1;
			var i = (int)(round (t * nextIndex));

			while (true)
			{
				if (t <= times[i])
					nextIndex = i;
				else
					prevIndex = i;

				i = (nextIndex + prevIndex) / 2;

				if (nextIndex - prevIndex <= 1)
					break;
			}

			float abPercent = mathx.invlerp (times[prevIndex], times[nextIndex], t);

			return new VertexPath.TimeOnPathData (prevIndex, nextIndex, abPercent);
		}

		public void Dispose ()
		{
			if (points.IsCreated)
				points.Dispose ();
			if (normals.IsCreated)
				normals.Dispose ();
			if (tangents.IsCreated)
				tangents.Dispose ();
			if (times.IsCreated)
				times.Dispose ();

			disposed = true;
		}
	}
}