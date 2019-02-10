using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Beans.Unity.Mathematics;

namespace Beans.Unity.Collections
{
	public struct NativeTexture2D : IDisposable
	{
		public bool IsCreated => nativePixels.IsCreated;

		public int width { get; private set; }
		public int height { get; private set; }

		private NativeArray<Color32> nativePixels;

		private void InitializeNativeData (Color32[] managedPixels)
		{
			if (nativePixels.IsCreated)
				nativePixels.Dispose ();
			nativePixels = new NativeArray<Color32> (managedPixels, Allocator.Persistent);
		}

		public void Update (Color32[] managedPixels, int width, int height)
		{
			if (!nativePixels.IsCreated || nativePixels.Length != managedPixels.Length)
				InitializeNativeData (managedPixels);
			else
				nativePixels.CopyFrom (managedPixels);
			this.width = width;
			this.height = height;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public Color32 GetPixel (int x, int y)
		{
			return nativePixels[x + y * width];
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public Color32 GetPixelBilinear (float x, float y)
		{
			x = mathx.repeat (x, 1f);
			y = mathx.repeat (y, 1f);

			var xMin = mathx.repeat ((int)(x * width), width);
			var yMin = mathx.repeat ((int)(y * height), height);
			var xMax = mathx.repeat ((int)((x + 1) * width), width);
			var yMax = mathx.repeat ((int)((y + 1) * height), height);

			var bottomLeft	= GetPixel (xMin, yMin);
			var bottomRight = GetPixel (xMax, yMin);
			var topLeft		= GetPixel (xMin, yMax);
			var topRight	= GetPixel (xMax, yMax);

			var xt = (x * width) - xMin;
			var yt = (y * height) - yMin;

			var leftColor = Color32.Lerp (bottomLeft, topLeft, yt);
			var rightColor = Color32.Lerp (bottomRight, topRight, yt);

			return Color32.Lerp (leftColor, rightColor, xt);
		}

		public void Dispose ()
		{
			nativePixels.Dispose ();
		}
	}
}