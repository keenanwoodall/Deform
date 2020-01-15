using System;
using UnityEngine;
using Beans.Unity.Collections;

namespace Deform
{
	/// <summary>
	/// Manages native and managed mesh data as well as the meshes they represent.
	/// </summary>
	[Serializable]
	public class MeshData : IData, IDisposable
	{
		// Stores the original shared mesh that other objects may be using.
		// Must be serialized so that if the Deformable that encapsulates this class is duplicated the reference won't be broken.
		[SerializeField, HideInInspector]
		public Mesh OriginalMesh;

		/// <summary>
		/// References the copy of the original mesh that processed data is applied to.
		/// Once this class is initialized, this is the mesh that will be used by MeshFilter.
		/// </summary>
		[NonSerialized]
		public Mesh DynamicMesh;

		/// <summary>
		/// Stores either a MeshFilter or SkinnedMeshRenderer and abstracts access to it.
		/// </summary>
		public MeshTarget Target;

		/// <summary>
		/// Stores original mesh data in NativeArrays for fast processing and multithreading.
		/// </summary>
		public NativeMeshData OriginalNative;

		/// <summary>
		/// Stores mesh data in NativeArrays for fast processing and multithreading.
		/// </summary>
		public NativeMeshData DynamicNative;

		// Must be serialized so that if the Deformable that encapsulates this class is duplicated the reference won't be broken.
		[SerializeField, HideInInspector]
		private bool initialized;

		/// Stores the original state of the mesh before any changes were made.
		[SerializeField, HideInInspector]
		private ManagedMeshData originalManaged;

		/// We can't directly transfer from a NativeArray to a mesh, so this is used as a middle-ground. 
		/// The transfer goes like this: Native Data -> Managed Data (this) -> Mesh
		private ManagedMeshData dynamicManaged;

		/// <summary>
		/// Convenient place to get the vertex count.
		/// </summary>
		public int Length { get; private set; }

		public bool Initialize (GameObject targetObject)
		{
			if (Target == null)
				Target = new MeshTarget ();
			if (!Target.Initialize (targetObject))
				return false;

			// Store the original mesh and make a copy (stored in DynamicMesh).
			// Assign the copy back to the filter so that this object has a unique mesh.
			if (!initialized)
			{
				OriginalMesh = Target.GetMesh ();

				if (OriginalMesh == null)
					return false;

				if (!OriginalMesh.isReadable)
				{
					Debug.LogError ($"The mesh '{OriginalMesh.name}' must have read/write permissions enabled.", OriginalMesh);
					return false;
				}

				DynamicMesh = GameObject.Instantiate (Target.GetMesh ());
			}
			// Since this has already been initialized, make a new mesh for the dynamic mesh to reference
			// so that two Deformables aren't displaying and modifying the same mesh.
			else if (OriginalMesh != null)
				DynamicMesh = GameObject.Instantiate (OriginalMesh);
			else if (DynamicMesh != null)
			{
				Debug.Log ($"Original mesh is missing. Attempting to create one from dynamic mesh ({DynamicMesh.name}) and original managed mesh data.", targetObject);
				OriginalMesh = GameObject.Instantiate (DynamicMesh);
				try
				{
					OriginalMesh.vertices = originalManaged.Vertices;
					OriginalMesh.normals = originalManaged.Normals;
					OriginalMesh.tangents = originalManaged.Tangents;
					OriginalMesh.uv = originalManaged.UVs;
					OriginalMesh.colors = originalManaged.Colors;
					OriginalMesh.triangles = originalManaged.Triangles;
					OriginalMesh.bounds = originalManaged.Bounds;
				}
				catch (NullReferenceException)
				{
					Debug.LogError ($"Attempted to recreate original mesh (from {DynamicMesh.name}), but the data was not valid. Please assign a new mesh.", targetObject);
					return false;
				}

				Debug.Log ($"Original mesh was recreated from {DynamicMesh.name}. This is not ideal, but prevents stuff from breaking when an original mesh is deleted. The best solution is to find and reassign the original mesh.", targetObject);
			}
			else
				return false;
			
			// Tell the mesh filter to display the dynamic mesh.
			Target.SetMesh (DynamicMesh);
			// Mark the dynamic mesh as dynamic for a hypothetical performance boost. 
			// (I've heard this method doesn't do anything)
			DynamicMesh.MarkDynamic ();

			Length = DynamicMesh.vertexCount;

			// Store mesh information in managed data.
			originalManaged = new ManagedMeshData (DynamicMesh);
			dynamicManaged = new ManagedMeshData (DynamicMesh);
			// Copy the managed data into native data.
			OriginalNative = new NativeMeshData (originalManaged);
			DynamicNative = new NativeMeshData (dynamicManaged);

			initialized = true;

			return true;
		}

		public ManagedMeshData GetOriginalManagedData () => originalManaged;
		public ManagedMeshData GetDynamicManagedData () => dynamicManaged;

		/// <summary>
		/// Disposes of current data and reinitializes with the targetObject's filter's shared mesh.
		/// </summary>
		/// <param name="filter">The filter whose mesh will be used as the new mesh data.</param>
		public void ChangeMesh (GameObject targetObject)
		{
			Dispose ();
			initialized = false;
			Initialize (targetObject);
		}

		/// <summary>
		/// Disposes of current data and reinitializes with a new mesh.
		/// </summary>
		/// <param name="mesh"></param>
		public void ChangeMesh (Mesh mesh)
		{
			Dispose ();
			Target.SetMesh (mesh);
			initialized = false;
			Initialize (Target.GetGameObject ());
		}

		/// <summary>
		/// Applies the dynamic native data's vertices, normals and bounds to the dynamic mesh.
		/// </summary>
		public void ApplyData(DataFlags dataFlags)
		{
			// Copy the native data into the managed data for efficient transfer into the actual mesh.
			DataUtils.CopyNativeDataToManagedData(dynamicManaged, DynamicNative, dataFlags);

			if (DynamicMesh == null)
				return;
			// Send managed data to mesh.
			if ((dataFlags & DataFlags.Vertices) != 0)
				DynamicMesh.vertices = dynamicManaged.Vertices;
			if ((dataFlags & DataFlags.Normals) != 0)
				DynamicMesh.normals = dynamicManaged.Normals;
			if ((dataFlags & DataFlags.Tangents) != 0)
				DynamicMesh.tangents = dynamicManaged.Tangents;
			if ((dataFlags & DataFlags.UVs) != 0)
				DynamicMesh.uv = dynamicManaged.UVs;
			if ((dataFlags & DataFlags.Colors) != 0)
				DynamicMesh.colors = dynamicManaged.Colors;
			if ((dataFlags & DataFlags.Triangles) != 0)
				DynamicMesh.triangles = dynamicManaged.Triangles;
			if ((dataFlags & DataFlags.Bounds) != 0)
				DynamicMesh.bounds = dynamicManaged.Bounds;
		}

		/// <summary>
		/// Applies all the original data to the dynamic mesh.
		/// </summary>
		public void ApplyOriginalData ()
		{
			DynamicMesh.vertices = originalManaged.Vertices;
			DynamicMesh.normals = originalManaged.Normals;
			DynamicMesh.tangents = originalManaged.Tangents;
			DynamicMesh.uv = originalManaged.UVs;
			DynamicMesh.colors = originalManaged.Colors;
			DynamicMesh.bounds = originalManaged.Bounds;
		}

		/// <summary>
		/// Copies the original native data over the dynamic native data.
		/// </summary>
		public void ResetData (DataFlags dataFlags)
		{
			// Copy the original mesh data into the native data to remove any changes.
			DataUtils.CopyNativeDataToNativeData (OriginalNative, DynamicNative, dataFlags);
		}

		/// <summary>
		/// Tries hard to make sure we're never processing or rendering anything null.
		/// </summary>
		/// <returns>Returns false if anything important is null.</returns>
		public bool EnsureData ()
		{
			// If no mesh is being rendered
			if (!Target.HasMesh ())
			{
				// but we still have the original mesh
				if (OriginalMesh != null)
				{
					Debug.Log ("No mesh being rendered. Sending original mesh to target.");
					// recreate the data from the original mesh.
					ChangeMesh (OriginalMesh);
					return true;
				}
				else
					return false;
			}
			// The target has a mesh
			else
			{
				// but it isn't the same one we're deforming.
				if (!TargetUsesDynamicMesh ())
				{
					// So update the data to reflect the rendered mesh.
					ChangeMesh (Target.GetMesh ());
				}
				return true;
			}
		}

		/// <summary>
		/// Returns true if the target is using dynamic mesh.
		/// If it isn't, that means we're deforming a mesh that isn't being rendered.
		/// </summary>
		public bool TargetUsesDynamicMesh ()
		{
			return Target.GetMesh () == DynamicMesh;
		}

		/// <summary>
		/// Disposes of all native data and sets the target's mesh back to the original one.
		/// </summary>
		public void Dispose () => Dispose (true);
		public void Dispose (bool assignOriginalMesh)
		{
			if (assignOriginalMesh)
			{
				// Meshes are special and don't get garbage collected so we need to destroy it manually.
				GameObject.DestroyImmediate (DynamicMesh);
				Target.SetMesh (OriginalMesh);
			}

			if (DynamicNative != null)
				DynamicNative.Dispose ();
			if (OriginalNative != null)
				OriginalNative.Dispose ();
		}
	}
}