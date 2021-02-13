using System;
using UnityEngine;

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
		
#if !UNITY_2019_3_OR_NEWER
		// You cannot copy directly from native arrays to a mesh before 2019.3, so we need to store
		// the mesh data in a managed form.
		[NonSerialized]
		public ManagedMeshData OriginalManaged;
		[NonSerialized]
		public ManagedMeshData DynamicManaged;
#endif

		// Must be serialized so that if the Deformable that encapsulates this class is duplicated the reference won't be broken.
		[SerializeField, HideInInspector]
		private bool initialized;

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
				Debug.Log ($"Original mesh is missing. Recreating one from dynamic mesh (\"{DynamicMesh.name}\"). This is not ideal, but prevents stuff from breaking when an original mesh is deleted. The best solution is to find and reassign the original mesh.", targetObject);
				OriginalMesh = GameObject.Instantiate (DynamicMesh);
				return false;
			}
			else
				return false;
			
			// Tell the mesh filter to display the dynamic mesh.
			Target.SetMesh (DynamicMesh);
			// Mark the dynamic mesh as dynamic for a hypothetical performance boost. 
			// (I've heard this method doesn't do anything)
			DynamicMesh.MarkDynamic ();

			Length = DynamicMesh.vertexCount;

			// Store the native data.
			OriginalNative = new NativeMeshData (DynamicMesh);
			DynamicNative = new NativeMeshData (DynamicMesh);
			
#if !UNITY_2019_3_OR_NEWER
			OriginalManaged = new ManagedMeshData(DynamicMesh);
			DynamicManaged = new ManagedMeshData(DynamicMesh);
#endif

			initialized = true;

			return true;
		}

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
			if (DynamicMesh == null)
				return;

#if UNITY_2019_3_OR_NEWER
			DataUtils.CopyNativeDataToMesh(DynamicNative, DynamicMesh, dataFlags);
#else
			DataUtils.CopyNativeDataToManagedData(DynamicManaged, DynamicNative, dataFlags);
			DataUtils.CopyManagedDataToMesh(DynamicManaged, DynamicMesh, dataFlags);
#endif
		}

		/// <summary>
		/// Applies all the original data to the dynamic mesh.
		/// </summary>
		public void ApplyOriginalData ()
		{
#if UNITY_2019_3_OR_NEWER
			DataUtils.CopyNativeDataToMesh(OriginalNative, DynamicMesh, DataFlags.All);
#else
			DataUtils.CopyManagedDataToMesh(OriginalManaged, DynamicMesh, DataFlags.All);
#endif
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