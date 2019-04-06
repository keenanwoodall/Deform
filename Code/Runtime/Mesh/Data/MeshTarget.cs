using UnityEngine;

namespace Deform
{
	/// <summary>
	/// Stores and abstracts access to a MeshFilter and SkinnedMeshRenderer.
	/// There will never be a MeshFilter AND SkinnedMeshRenderer, this class just makes it
	/// easy to get and set the mesh of whichever one exists without constantly having to check
	/// which one is null.
	/// </summary>
	[System.Serializable]
	public class MeshTarget
	{
		[SerializeField, HideInInspector] private MeshFilter meshFilter;
		[SerializeField, HideInInspector] private SkinnedMeshRenderer skinnedMeshRenderer;

		public static bool IsValid (GameObject target)
		{
			if (target.GetComponent<MeshFilter> () == null)
				if (target.GetComponent<SkinnedMeshRenderer> () == null)
					return false;
			return true;
		}

		/// <summary>
		/// Tries to find a MeshFilter or SkinnedMeshRenderer on the target game object.
		/// </summary>
		/// <param name="target"></param>
		/// <returns>Returns true if a MeshFilter of SkinnedMeshRenderer is found.</returns>
		public bool Initialize (GameObject target)
		{
			if (target == null)
				return false;

			if (Exists ())
			{
				meshFilter = null;
				skinnedMeshRenderer = null;
			}

			meshFilter = target.GetComponent<MeshFilter> ();
			if (meshFilter == null)
			{
				skinnedMeshRenderer = target.GetComponent<SkinnedMeshRenderer> ();
				if (skinnedMeshRenderer == null)
					return false;
				else
					skinnedMeshRenderer.updateWhenOffscreen = true;
			}
			return true;
		}

		/// <summary>
		/// Returns false if the mesh filter AND skinned mesh renderer is null.
		/// </summary>
		public bool Exists ()
		{
			return meshFilter != null || skinnedMeshRenderer != null;
		}

		/// <summary>
		/// Returns true if the target's mesh isn't null.
		/// </summary>
		public bool HasMesh ()
		{
			return GetMesh () != null;
		}

		/// <summary>
		/// Returns the target's mesh.
		/// </summary>
		public Mesh GetMesh ()
		{
			if (meshFilter != null)
				return meshFilter.sharedMesh;
			if (skinnedMeshRenderer != null)
				return skinnedMeshRenderer.sharedMesh;
			return null;
		}

		/// <summary>
		/// Sets the target's mesh.
		/// </summary>
		public void SetMesh (Mesh mesh)
		{
			if (mesh == null)
				return;

			if (meshFilter != null)
			{
				meshFilter.sharedMesh = mesh;
				return;
			}
			if (skinnedMeshRenderer != null)
			{
				skinnedMeshRenderer.sharedMesh = mesh;
				return;
			}

			Debug.LogError ("Deformable doesn't have a target. Mesh cannot be set.");
		}

		/// <summary>
		/// Returns the target's renderer.
		/// </summary>
		public Renderer GetRenderer ()
		{
			if (skinnedMeshRenderer != null)
				return skinnedMeshRenderer;
			else if (meshFilter != null)
				return meshFilter.GetComponent<MeshRenderer> ();
			else
				return null;
		}

		/// <summary>
		/// Returns the target gameobject.
		/// </summary>
		public GameObject GetGameObject ()
		{
			if (meshFilter != null)
				return meshFilter.gameObject;
			if (skinnedMeshRenderer != null)
				return skinnedMeshRenderer.gameObject;
			return null;
		}
		
		/// <summary>
		/// Returns the target's transform.
		/// </summary>
		public Transform GetTransform ()
		{
			if (meshFilter != null)
				return meshFilter.transform;
			if (skinnedMeshRenderer != null)
				return skinnedMeshRenderer.transform;
			return null;
		}
	}
}