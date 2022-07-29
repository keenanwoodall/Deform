using System;
using UnityEngine;
using Unity.Collections;
using static Unity.Mathematics.math;
using Beans.Unity.Mathematics;

namespace Beans.Unity.Collections
{
	/// <summary>
	/// Handles the caching of animation curve data for use with the job system.
	/// </summary>
	public struct NativeCurve : IDisposable
	{
		/// <summary>
		/// Is the inner native array created?
		/// </summary>
		public bool IsCreated => values.IsCreated;

		private NativeArray<float> values;
		private WrapMode preWrapMode;
		private WrapMode postWrapMode;

		public NativeCurve(AnimationCurve curve, int resolution, Allocator allocator)
		{
			if (resolution < 2)
				throw new ArgumentException("Resolution must be greater than two.");
			if (curve == null || curve.length == 0)
				throw new ArgumentException("Curve must have at least one keyframe.");
			
			preWrapMode = curve.preWrapMode;
			postWrapMode = curve.postWrapMode;
			
			values = new NativeArray<float> (resolution, allocator, NativeArrayOptions.UninitializedMemory);
			
			CacheValues(curve);
		}


		public void CacheValues(AnimationCurve curve)
		{
			if (curve == null)
				throw new ArgumentNullException ("curve");
			
			preWrapMode = curve.preWrapMode;
			postWrapMode = curve.postWrapMode;
			for (int i = 0; i < values.Length; i++)
				values[i] = curve.Evaluate ((float)i / values.Length);
		}

		/// <summary>
		/// Returns the height of the curve at t.
		/// </summary>
		public float Evaluate (float t)
		{
			var count = values.Length;

			if (count == 1)
				return values[0];

			if (t < 0f)
			{
				switch (preWrapMode)
				{
					default:
						return values[0];
					case WrapMode.Loop:
						t = 1f - abs (t) % 1f;
						break;
					case WrapMode.PingPong:
						t = mathx.pingpong (t, 1f);
						break;
				}
			}
			else if (t > 1f)
			{
				switch (postWrapMode)
				{
					default:
						return values[count - 1];
					case WrapMode.Loop:
						t %= 1f;
						break;
					case WrapMode.PingPong:
						t = mathx.pingpong (t, 1f);
						break;
				}
			}

			var it = t * (count - 1);

			var lower = (int)it;
			var upper = lower + 1;
			if (upper >= count)
				upper = count - 1;

			return lerp (values[lower], values[upper], it - lower);
		}

		/// <summary>
		/// Disposes of the native array.
		/// </summary>
		public void Dispose ()
		{
			if (values.IsCreated)
				values.Dispose ();
		}
	}
}