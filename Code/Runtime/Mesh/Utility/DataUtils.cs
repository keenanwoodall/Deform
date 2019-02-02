using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Deform
{
	/// <summary>
	/// Contains utility methods for managing and transferring mesh data.
	/// </summary>
	public static class DataUtils
	{
		/// <summary>
		/// Copies mesh data from managed arrays into native ones.
		/// </summary>
		/// <param name="onlyEssentials">If true, only vertices, normals, bounds, and mask vertices will be copied.</param>
		public static void CopyManagedToNativeMeshData (ManagedMeshData managed, NativeMeshData native, DataFlags dataFlags)
		{
			if (!managed.HasValidData () || !native.HasValidData ())
			{
				Debug.LogError ("Cannot copy data as some of it is invalid");
				return;
			}

			if ((dataFlags & DataFlags.Vertices) != 0)
				CopyManagedToNative (managed.Vertices, native.VertexBuffer);
			if ((dataFlags & DataFlags.Normals) != 0)
				CopyManagedToNative (managed.Normals, native.NormalBuffer);
			if ((dataFlags & DataFlags.MaskVertices) != 0)
				CopyManagedToNative (managed.Vertices, native.MaskVertexBuffer);
			if ((dataFlags & DataFlags.Tangents) != 0)
				CopyManagedToNative (managed.Tangents, native.TangentBuffer);
			if ((dataFlags & DataFlags.UVs) != 0)
				CopyManagedToNative (managed.UVs, native.UVBuffer);
			if ((dataFlags & DataFlags.Colors) != 0)
				CopyManagedToNative (managed.Colors, native.ColorBuffer);
			if ((dataFlags & DataFlags.Triangles) != 0)
				CopyManagedToNative (managed.Triangles, native.IndexBuffer);
			if ((dataFlags & DataFlags.Bounds) != 0)
				native.Bounds[0] = managed.Bounds;
		}

		/// <summary>
		/// Copies mesh data from native arrays into managed ones.
		/// </summary>
		/// <param name="onlyEssentials">If true, only vertices, normals and bounds are copied. The mask data isn't copied because is only exists in native data.</param>
		public static void CopyNativeDataToManagedData (ManagedMeshData managed, NativeMeshData native, DataFlags dataFlags)
		{
			if (!managed.HasValidData () || !native.HasValidData ())
			{
				Debug.LogError ("Cannot copy data as some of it is invalid");
				return;
			}

			if ((dataFlags & DataFlags.Vertices) != 0)
				CopyNativeToManaged (managed.Vertices, native.VertexBuffer);
			if ((dataFlags & DataFlags.Normals) != 0)
				CopyNativeToManaged (managed.Normals, native.NormalBuffer);
			if ((dataFlags & DataFlags.Tangents) != 0)
				CopyNativeToManaged (managed.Tangents, native.TangentBuffer);
			if ((dataFlags & DataFlags.UVs) != 0)
				CopyNativeToManaged (managed.UVs, native.UVBuffer);
			if ((dataFlags & DataFlags.Colors) != 0)
				CopyNativeToManaged (managed.Colors, native.ColorBuffer);
			if ((dataFlags & DataFlags.Triangles) != 0)
				CopyNativeToManaged (managed.Triangles, native.IndexBuffer);
			if ((dataFlags & DataFlags.Bounds) != 0)
				managed.Bounds = native.Bounds[0];
		}

		/// <summary>
		/// Copies mesh data from one native array to another
		/// </summary>
		/// <param name="onlyEssentials">If true, only vertices, normals, bounds, and mask vertices will be copied.</param>
		public static void CopyNativeDataToNativeData (NativeMeshData from, NativeMeshData to, DataFlags dataFlags)
		{
			if (!to.HasValidData () || !from.HasValidData ())
			{
				Debug.LogError ("Cannot copy data as some of it is invalid");
				return;
			}

			if ((dataFlags & DataFlags.Vertices) != 0)
				from.VertexBuffer.CopyTo (to.VertexBuffer);
			if ((dataFlags & DataFlags.Normals) != 0)
				from.NormalBuffer.CopyTo (to.NormalBuffer);
			if ((dataFlags & DataFlags.MaskVertices) != 0)
				from.MaskVertexBuffer.CopyTo (to.MaskVertexBuffer);
			if ((dataFlags & DataFlags.Tangents) != 0)
				from.TangentBuffer.CopyTo (to.TangentBuffer);
			if ((dataFlags & DataFlags.UVs) != 0)
				from.UVBuffer.CopyTo (to.UVBuffer);
			if ((dataFlags & DataFlags.Colors) != 0)
				from.ColorBuffer.CopyTo (to.ColorBuffer);
			if ((dataFlags & DataFlags.Triangles) != 0)
				from.IndexBuffer.CopyTo (to.IndexBuffer);
			if ((dataFlags & DataFlags.Bounds) != 0)
				from.Bounds.CopyTo (to.Bounds);
		}

		// Managed -> Native

		/// <summary>
		/// Copies a managed int array into a native int array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyManagedToNative (int[] managed, NativeArray<int> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					source: managedArrayPointer,
					size: managed.Length * (long)UnsafeUtility.SizeOf<int> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed int array into a native int array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyManagedToNative (float[] managed, NativeArray<float> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					source: managedArrayPointer,
					size: managed.Length * (long)UnsafeUtility.SizeOf<float> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed Vector2 array into a native float2 array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyManagedToNative (Vector2[] managed, NativeArray<float2> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					source: managedArrayPointer,
					size: managed.Length * (long)UnsafeUtility.SizeOf<Vector2> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed Vector3 array into a native float3 array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyManagedToNative (Vector3[] managed, NativeArray<float3> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy 
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native), 
					source: managedArrayPointer, 
					size: managed.Length * (long)UnsafeUtility.SizeOf<Vector3> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed Vector4 array into a native float4 array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyManagedToNative (Vector4[] managed, NativeArray<float4> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					source: managedArrayPointer,
					size: managed.Length * (long)UnsafeUtility.SizeOf<Vector4> ()
				);
			}
		}
		/// <summary>
		/// Copies a managed Color array into a native float4 array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyManagedToNative (Color[] managed, NativeArray<float4> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					source: managedArrayPointer,
					size: managed.Length * (long)UnsafeUtility.SizeOf<Color> ()
				);
			}
		}

		// Native -> Managed

		/// <summary>
		/// Copies a native int array into a managed int array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyNativeToManaged (int[] managed, NativeArray<int> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					size: managed.Length * (long)UnsafeUtility.SizeOf<int> ()
				);
			}
		}
		/// <summary>
		/// Copies a native float2 array into a managed Vector2 array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyNativeToManaged (Vector2[] managed, NativeArray<float2> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					size: managed.Length * (long)UnsafeUtility.SizeOf<Vector2> ()
				);
			}
		}
		/// <summary>
		/// Copies a native float3 array into a managed Vector3 array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyNativeToManaged (Vector3[] managed, NativeArray<float3> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					size: managed.Length * (long)UnsafeUtility.SizeOf<Vector3> ()
				);
			}
		}
		/// <summary>
		/// Copies a native float4 array into a managed Vector4 array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyNativeToManaged (Vector4[] managed, NativeArray<float4> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					size: managed.Length * (long)UnsafeUtility.SizeOf<Vector4> ()
				);
			}
		}
		/// <summary>
		/// Copies a native float4 array into a managed Color array. Array lengths MUST be the same.
		/// </summary>
		public unsafe static void CopyNativeToManaged (Color[] managed, NativeArray<float4> native)
		{
			fixed (void* managedArrayPointer = managed)
			{
				UnsafeUtility.MemCpy
				(
					destination: managedArrayPointer,
					source: NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks (native),
					size: managed.Length * (long)UnsafeUtility.SizeOf<Color> ()
				);
			}
		}
	}
}