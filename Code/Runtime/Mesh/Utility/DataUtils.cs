using UnityEngine;
using Unity.Mathematics;
using Beans.Unity.Collections;

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
		public static bool CopyManagedToNativeMeshData (ManagedMeshData managed, NativeMeshData native, DataFlags dataFlags)
		{
			var dataIsValid = true;

			if (!managed.HasValidData ())
			{
				dataIsValid = false;
				Debug.LogError ("Cannot copy data as the managed data is invalid");
			}
			if (!native.HasValidData ())
			{
				Debug.LogError ("Cannot copy data as the native data is invalid");
				dataIsValid = false;
			}

			if (!dataIsValid)
				return false;

			if ((dataFlags & DataFlags.Vertices) != 0)
				managed.Vertices.MemCpy (native.VertexBuffer);
			if ((dataFlags & DataFlags.Normals) != 0)
				managed.Normals.MemCpy (native.NormalBuffer);
			if ((dataFlags & DataFlags.MaskVertices) != 0)
				managed.Vertices.MemCpy (native.MaskVertexBuffer);
			if ((dataFlags & DataFlags.Tangents) != 0)
				managed.Tangents.MemCpy (native.TangentBuffer);
			if ((dataFlags & DataFlags.UVs) != 0)
				managed.UVs.MemCpy (native.UVBuffer);
			if ((dataFlags & DataFlags.Colors) != 0)
				managed.Colors.MemCpy (native.ColorBuffer);
			if ((dataFlags & DataFlags.Triangles) != 0)
				managed.Triangles.MemCpy (native.IndexBuffer);
			if ((dataFlags & DataFlags.Bounds) != 0)
				native.Bounds[0] = managed.Bounds;

			return true;
		}
		public static bool CopyToNativeData(this ManagedMeshData from, NativeMeshData to, DataFlags dataFlags) => CopyManagedToNativeMeshData(from, to, dataFlags);

		/// <summary>
		/// Copies mesh data from native arrays into managed ones.
		/// </summary>
		public static bool CopyNativeDataToManagedData (ManagedMeshData managed, NativeMeshData native, DataFlags dataFlags)
		{
			var dataIsValid = true;

			if (!managed.HasValidData ())
			{
				dataIsValid = false;
				Debug.LogError ("Cannot copy data as the managed data is invalid");
			}
			if (!native.HasValidData ())
			{
				Debug.LogError ("Cannot copy data as the native data is invalid");
				dataIsValid = false;
			}

			if (!dataIsValid)
				return false;

			if ((dataFlags & DataFlags.Vertices) != 0)
				native.VertexBuffer.MemCpy (managed.Vertices);
			if ((dataFlags & DataFlags.Normals) != 0)
				native.NormalBuffer.MemCpy (managed.Normals);
			if ((dataFlags & DataFlags.Tangents) != 0)
				native.TangentBuffer.MemCpy (managed.Tangents);
			if ((dataFlags & DataFlags.UVs) != 0)
				native.UVBuffer.MemCpy (managed.UVs);
			if ((dataFlags & DataFlags.Colors) != 0)
				native.ColorBuffer.MemCpy (managed.Colors);
			if ((dataFlags & DataFlags.Triangles) != 0)
				native.IndexBuffer.CopyTo (managed.Triangles);
			if ((dataFlags & DataFlags.Bounds) != 0)
				managed.Bounds = native.Bounds[0];

			return true;
		}
		public static bool CopyToManagedData(this NativeMeshData from, ManagedMeshData to, DataFlags dataFlags) => CopyNativeDataToManagedData(to, from, dataFlags);
		
		/// <summary>
		/// Copies mesh data from one native array to another
		/// </summary>
		public static bool CopyNativeDataToNativeData (NativeMeshData from, NativeMeshData to, DataFlags dataFlags)
		{
			if (!to.HasValidData () || !from.HasValidData ())
			{
				Debug.LogError ("Cannot copy data as some of it is invalid");
				return false;
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

			return true;
		}
		public static bool CopyToNativeData(this NativeMeshData from, NativeMeshData to, DataFlags dataFlags) => CopyNativeDataToNativeData(from, to, dataFlags);
		
		public static bool CopyManagedDataToMesh(ManagedMeshData from, Mesh to, DataFlags dataFlags)
		{
			if (!from.HasValidData ())
			{
				Debug.LogError ("Cannot copy data as some of it is invalid");
				return false;
			}
			if (to == null)
			{
				Debug.LogError("Cannot copy data to null mesh");
				return false;
			}

			// Send managed data to mesh.
			if ((dataFlags & DataFlags.Vertices) != 0)
				to.vertices = from.Vertices;
			if ((dataFlags & DataFlags.Normals) != 0)
				to.normals = from.Normals;
			if ((dataFlags & DataFlags.Tangents) != 0)
				to.tangents = from.Tangents;
			if ((dataFlags & DataFlags.UVs) != 0)
				to.uv = from.UVs;
			if ((dataFlags & DataFlags.Colors) != 0)
				to.colors = from.Colors;
			if ((dataFlags & DataFlags.Triangles) != 0)
				to.triangles = from.Triangles;
			if ((dataFlags & DataFlags.Bounds) != 0)
				to.bounds = from.Bounds;

			return true;
		}
		public static bool CopyToMesh(ManagedMeshData from, Mesh to, DataFlags dataFlags) => CopyManagedDataToMesh(from, to, dataFlags);
		
#if UNITY_2019_3_OR_NEWER
		/// <summary>
		/// Copies mesh data from one native array to another
		/// </summary>
		public static bool CopyNativeDataToMesh (NativeMeshData from, Mesh to, DataFlags dataFlags)
		{
			if (!from.HasValidData ())
			{
				Debug.LogError ("Cannot copy data as some of it is invalid");
				return false;
			}

			if (to == null)
			{
				Debug.LogError("Cannot copy data to null mesh");
				return false;
			}

			if ((dataFlags & DataFlags.Vertices) != 0)
				to.SetVertices(from.VertexBuffer);
			if ((dataFlags & DataFlags.Normals) != 0)
				to.SetNormals(from.NormalBuffer);
			if ((dataFlags & DataFlags.Tangents) != 0)
				to.SetTangents(from.TangentBuffer);
			if ((dataFlags & DataFlags.UVs) != 0)
				to.SetUVs(0, from.UVBuffer);
			if ((dataFlags & DataFlags.Colors) != 0)
				to.SetColors(from.ColorBuffer);
			if ((dataFlags & DataFlags.Triangles) != 0)
			{
				for (int i = 0; i < to.subMeshCount; i++)
				{
					var submesh = to.GetSubMesh(i);
					to.SetIndices
					(
						indices: 			from.IndexBuffer,
						indicesStart: 		submesh.indexStart, 
						indicesLength: 		submesh.indexCount, 
						topology: 			submesh.topology, 
						submesh: 			i, 
						calculateBounds: 	false, 
						baseVertex: 		submesh.baseVertex
					);
				}
			}

			if ((dataFlags & DataFlags.Bounds) != 0)
				to.bounds = from.Bounds[0];

			return true;
		}
		public static bool CopyToMesh(this NativeMeshData from, Mesh to, DataFlags dataFlags) => CopyNativeDataToMesh(from, to, dataFlags);
#endif
	}
}