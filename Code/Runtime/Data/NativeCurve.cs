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

		private void InitializeValues (int count)
		{
			if (values.IsCreated)
				values.Dispose ();

			values = new NativeArray<float> (count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}

		/// <summary>
		/// Updates the value cache.
		/// </summary>
		/// <param name="resolution">How many values should be calculated.</param>
		public void Update (AnimationCurve curve, int resolution)
		{
			if (curve == null)
				throw new ArgumentNullException ("curve");

			preWrapMode = curve.preWrapMode;
			postWrapMode = curve.postWrapMode;

			if (!values.IsCreated || values.Length != resolution)
				InitializeValues (resolution);

			for (int i = 0; i < resolution; i++)
				values[i] = curve.Evaluate ((float)i / (float)resolution);
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
						t = 1f - (abs (t) % 1f);
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