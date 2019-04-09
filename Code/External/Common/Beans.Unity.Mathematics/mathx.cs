using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Beans.Unity.Mathematics
{
	public static class mathx
	{
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static quaternion lookrot (float3 forward, float3 up)
		{
			normalize (forward);

			var v1 = forward;
			var v2 = normalize (cross (up, v1));
			var v3 = cross (v1, v2);

			var m00 = v2.x;
			var m01 = v2.y;
			var m02 = v2.z;
			var m10 = v3.x;
			var m11 = v3.y;
			var m12 = v3.z;
			var m20 = v1.x;
			var m21 = v1.y;
			var m22 = v1.z;

			var n8 = (m00 + m11) + m22;
			var rotation = float4 (0f);
			if (n8 > 0f)
			{
				var n = sqrt (n8 + 1f);
				rotation.w = n * 0.5f;
				n = 0.5f / n;
				rotation.x = (m12 - m21) * n;
				rotation.y = (m20 - m02) * n;
				rotation.z = (m01 - m10) * n;
				return new quaternion (rotation);
			}
			if ((m00 >= m11) && (m00 >= m22))
			{
				var n7 = sqrt (1f + m00 - m11 - m22);
				var n4 = 0.5f / n7;
				rotation.x = 0.5f * n7;
				rotation.y = (m01 + m10) * n4;
				rotation.z = (m02 + m20) * n4;
				rotation.w = (m12 - m21) * n4;
				return new quaternion (rotation);
			}
			if (m11 > m22)
			{
				var n6 = sqrt (1f + m11 - m00 - m22);
				var n3 = 0.5f / n6;
				rotation.x = (m10 + m01) * n3;
				rotation.y = 0.5f * n6;
				rotation.z = (m21 + m12) * n3;
				rotation.w = (m20 - m02) * n3;
				return new quaternion (rotation);
			}
			var n5 = sqrt (1f + m22 - m00 - m11);
			var n2 = 0.5f / n5;
			rotation.x = (m20 + m02) * n2;
			rotation.y = (m21 + m12) * n2;
			rotation.z = 0.5f * n5;
			rotation.w = (m01 - m10) * n2;
			return new quaternion (rotation);
		}

		/// <summary>
		/// Loops t every length.
		/// </summary>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static float repeat (float t, float length)
		{
			if (t > 0)
				return t % length;
			else
				return length - (abs (t) % length);
		}
		/// <summary>
		/// Loops t every length.
		/// </summary>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static int repeat (int t, int length)
		{
			length -= 1;
			if (t > 0)
				return t % length;
			else
				return length - (abs (t) % length);
		}
		/// <summary>
		/// Loops t every length.
		/// </summary>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static int2 repeat (int2 t, int2 length)
		{
			return int2 (repeat (t.x, length.x), repeat (t.y, length.y));
		}

		/// <summary>
		/// Pingpongs t back and forth from length.
		/// </summary>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static float pingpong (float t, float length)
		{
			return length - abs (repeat (t, length * 2f) - length);
		}

		/// <summary>
		/// Returns the shortest distance between a box and point.
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <param name="l"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static float distfrombox (float w, float h, float l, float3 p)
		{
			var dx = max (max (-w - p.x, p.x - w), 0f);
			var dy = max (max (-h - p.y, p.y - h), 0f);
			var dz = max (max (-l - p.z, p.z - l), 0f);

			return length (float3 (dx, dy, dz));
		}

		/// <summary>
		/// Returns a 2D point at t distance along four points' bezier curve.
		/// </summary>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static float2 bezier2D (float2 p1, float2 p2, float2 p3, float2 p4, float2 t)
		{
			var omt = 1f - t;
			var omt2 = omt * omt;
			var t2 = t * t;
			return
				p1 * (omt2 * omt) +
				p2 * (3f * omt2 * t) +
				p3 * (3f * omt * t2) +
				p4 * (t2 * t);
		}

		/// <summary>
		/// Returns a 2D tangent at t distance along four points' bezier curve.
		/// </summary>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static float2 beziertangent2D (float2 p1, float2 p2, float2 p3, float2 p4, float2 t)
		{
			var omt = 1f - t;
			var omt2 = omt * omt;
			var t2 = t * t;

			return normalize
			(
				p1 * (-omt2) +
				p2 * (3f * omt2 - 2f * omt) +
				p3 * (-3f * t2 + 2f * t) +
				p4 * (t2)
			);
		}
	}
}