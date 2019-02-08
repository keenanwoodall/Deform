using System;
using Unity.Collections;
using Unity.Mathematics;
using Beans.Unity.Mathematics;

namespace Deform
{
	/// <summary>
	/// Stores mesh data in NativeArrays for fast processing and multithreading.
	/// </summary>
	public class NativeMeshData : IDisposable
	{
		public NativeArray<float3> VertexBuffer;
		public NativeArray<float3> NormalBuffer;
		public NativeArray<float4> TangentBuffer;
		public NativeArray<float2> UVBuffer;
		public NativeArray<float4> ColorBuffer;
		public NativeArray<int> IndexBuffer;
		public NativeArray<float3> MaskVertexBuffer;
		public NativeArray<bounds> Bounds;

		public NativeMeshData (ManagedMeshData data, Allocator allocator = Allocator.Persistent)
		{
			var length = data.Vertices.Length;

			VertexBuffer		= new NativeArray<float3>		(data.Vertices.Length,	allocator, NativeArrayOptions.UninitializedMemory);
			NormalBuffer		= new NativeArray<float3>		(data.Normals.Length,	allocator, NativeArrayOptions.UninitializedMemory);
			TangentBuffer		= new NativeArray<float4>		(data.Tangents.Length,	allocator, NativeArrayOptions.UninitializedMemory);
			UVBuffer			= new NativeArray<float2>		(data.UVs.Length,		allocator, NativeArrayOptions.UninitializedMemory);
			ColorBuffer			= new NativeArray<float4>		(data.Colors.Length,	allocator, NativeArrayOptions.UninitializedMemory);
			IndexBuffer			= new NativeArray<int>			(data.Triangles.Length, allocator, NativeArrayOptions.UninitializedMemory);
			MaskVertexBuffer	= new NativeArray<float3>		(data.Vertices.Length,	allocator, NativeArrayOptions.UninitializedMemory);
			Bounds				= new NativeArray<bounds>		(1,						allocator, NativeArrayOptions.UninitializedMemory);

			DataUtils.CopyManagedToNativeMeshData (data, this, DataFlags.All);
		}

		/// <summary>
		/// Disposes of all native arrays.
		/// </summary>
		public void Dispose ()
		{
			if (VertexBuffer.IsCreated)
				VertexBuffer.Dispose ();
			if (NormalBuffer.IsCreated)
				NormalBuffer.Dispose ();
			if (TangentBuffer.IsCreated)
				TangentBuffer.Dispose ();
			if (UVBuffer.IsCreated)
				UVBuffer.Dispose ();
			if (ColorBuffer.IsCreated)
				ColorBuffer.Dispose ();
			if (IndexBuffer.IsCreated)
				IndexBuffer.Dispose ();
			if (MaskVertexBuffer.IsCreated)
				MaskVertexBuffer.Dispose ();
			if (Bounds.IsCreated)
				Bounds.Dispose ();
		}

		/// <summary>
		/// Returns true if all the arrays are created.
		/// </summary>
		/// <returns></returns>
		public bool HasValidData () =>
			VertexBuffer.IsCreated
			&& NormalBuffer.IsCreated
			&& TangentBuffer.IsCreated
			&& UVBuffer.IsCreated
			&& ColorBuffer.IsCreated
			&& IndexBuffer.IsCreated
			&& MaskVertexBuffer.IsCreated
			&& Bounds.IsCreated;
	}
}