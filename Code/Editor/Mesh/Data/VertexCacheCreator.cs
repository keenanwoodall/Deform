using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	/// <summary>
	/// Handles creating vertex caches in the Editor.
	/// </summary>
	public static class VertexCacheCreator
	{
		/// <summary>
		/// Returns true if a mesh asset or GameObject is selected that a vertex cache can be created from.
		/// </summary>
		/// <returns></returns>
		[MenuItem ("Assets/Create/Deform/Vertex Cache", true, 10000)]
		private static bool CanValidateVertexCache ()
		{
			var selections = Selection.gameObjects;

			foreach (var selection in selections)
			{
				var meshFilters = selection.GetComponentsInChildren<MeshFilter> ();
				if (meshFilters != null && meshFilters.Length > 0)
					return true;

				var skinnedMeshRenderers = selection.GetComponentsInChildren<SkinnedMeshRenderer> ();
				if (skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Creates a vertex cache based on the currently selected mesh asset or gameobject.
		/// </summary>
		[MenuItem ("Assets/Create/Deform/Vertex Cache")]
		public static void CreateVertexCache ()
		{
			var selections = Selection.gameObjects;

			foreach (var selection in selections)
			{
				var meshFilters = selection.GetComponentsInChildren<MeshFilter> ();
				var skinnedMeshRenderers = selection.GetComponentsInChildren<SkinnedMeshRenderer> ();

				foreach (var mf in meshFilters)
				{
					var cache = ScriptableObject.CreateInstance<VertexCache> ();
					cache.Initialize (mf.sharedMesh);

					AssetDatabase.CreateAsset (cache, $"Assets/{mf.name}.asset");
				}

				foreach (var smr in skinnedMeshRenderers)
				{
					var cache = ScriptableObject.CreateInstance<VertexCache> ();
					cache.Initialize (smr.sharedMesh);

					AssetDatabase.CreateAsset (cache, $"Assets/{smr.name}.asset");
				}
			}
		}
	}
}