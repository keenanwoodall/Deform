using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Beans.Unity.Collections
{
	public static class NativeArrayx
	{
		// Managed -> Native

		/// <summary>
		/// Copies a managed int array into a native int array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this int[] source, NativeArray<int> destination)
		{
			fixed (void* managedArrayPointer = source)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (destination),
					source: managedArrayPointer,
					size: source.Length * (long)UnsafeUtility.SizeOf<int> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed int array into a native int array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this float[] source, NativeArray<float> destination)
		{
			fixed (void* managedArrayPointer = source)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (destination),
					source: managedArrayPointer,
					size: source.Length * (long)UnsafeUtility.SizeOf<float> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed Vector2 array into a native float2 array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this Vector2[] source, NativeArray<float2> destination)
		{
			fixed (void* managedArrayPointer = source)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (destination),
					source: managedArrayPointer,
					size: source.Length * (long)UnsafeUtility.SizeOf<Vector2> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed Vector3 array into a native float3 array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this Vector3[] source, NativeArray<float3> destination)
		{
			fixed (void* managedArrayPointer = source)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (destination),
					source: managedArrayPointer,
					size: source.Length * (long)UnsafeUtility.SizeOf<Vector3> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed Vector4 array into a native float4 array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this Vector4[] source, NativeArray<float4> destination)
		{
			fixed (void* managedArrayPointer = source)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (destination),
					source: managedArrayPointer,
					size: source.Length * (long)UnsafeUtility.SizeOf<Vector4> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed Color array into a native float4 array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this Color[] source, NativeArray<float4> destination)
		{
			fixed (void* managedArrayPointer = source)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (destination),
					source: managedArrayPointer,
					size: source.Length * (long)UnsafeUtility.SizeOf<Color> ()
				);
			}
		}

		// Native -> Managed

		/// <summary>
		/// Copies a native int array into a managed int array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this NativeArray<int> source, int[] destination)
		{
			fixed (void* managedArrayPointer = destination)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (source),
					size: destination.Length * (long)UnsafeUtility.SizeOf<int> ()
				);
			}
		}
		/// <summary>
		/// Copies a native int array into a managed int array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this NativeArray<float> source, float[] destination)
		{
			fixed (void* managedArrayPointer = destination)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (source),
					size: destination.Length * (long)UnsafeUtility.SizeOf<int> ()
				);
			}
		}
		/// <summary>
		/// Copies a native float2 array into a managed Vector2 array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this NativeArray<float2> source, Vector2[] destination)
		{
			fixed (void* managedArrayPointer = destination)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (source),
					size: destination.Length * (long)UnsafeUtility.SizeOf<Vector2> ()
				);
			}
		}
		/// <summary>
		/// Copies a native float3 array into a managed Vector3 array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this NativeArray<float3> source, Vector3[] destination)
		{
			fixed (void* managedArrayPointer = destination)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (source),
					size: destination.Length * (long)UnsafeUtility.SizeOf<Vector3> ()
				);
			}
		}
		/// <summary>
		/// Copies a native float4 array into a managed Vector4 array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this NativeArray<float4> source, Vector4[] destination)
		{
			fixed (void* managedArrayPointer = destination)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (source),
					size: destination.Length * (long)UnsafeUtility.SizeOf<Vector4> ()
				);
			}
		}
		/// <summary>
		/// Copies a native float4 array into a managed Color array. Array lengths MUST be the same.
		/// </summary>
		public static unsafe void MemCpy (this NativeArray<float4> source, Color[] destination)
		{
			fixed (void* managedArrayPointer = destination)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (source),
					size: destination.Length * (long)UnsafeUtility.SizeOf<Color> ()
				);
			}
		}
	}
}