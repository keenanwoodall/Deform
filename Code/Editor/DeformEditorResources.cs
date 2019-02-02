using UnityEngine;
using UnityEditor;

namespace DeformEditor
{
	/// <summary>
	/// A class to assist in loading editor resources.
	/// </summary>
	public static class DeformEditorResources
	{
		/// <summary>
		/// A path to Deform's Editor Resources folder, relative to the project.
		/// </summary>
		public static readonly string RESOURCES_PATH = "Assets/Deform/EditorResources/";

		/// <summary>
		/// Loads an asset of type T at a path relative to RESOURCES_PATH.
		/// </summary>
		public static T LoadAsset<T> (string path) where T : Object
		{
			return AssetDatabase.LoadAssetAtPath<T> ($"{RESOURCES_PATH}{path}");
		}

		/// <summary>
		/// Creates an asset of type T at a path relative to RESOURCES_PATH.
		/// </summary>
		public static void CreateAsset (Object asset, string path)
		{
			AssetDatabase.CreateAsset (asset, $"{RESOURCES_PATH}{path}");
		}

		/// <summary>
		/// Loads an mesh at a path relative to RESOURCES_PATH/Meshes.
		/// </summary>
		public static Mesh LoadMesh (string name)
		{
			return LoadAsset<Mesh> ($"Meshes/{name}");
		}

		/// <summary>
		/// Loads a mesh at RESOURCES_PATH/Meshes folder with the name "DefaultMesh".
		/// </summary>
		/// <returns></returns>
		public static Mesh LoadDefaultMesh ()
		{
			var mesh = LoadMesh ("DefaultMesh.fbx");
			if (mesh == null)
				mesh = LoadMesh ("DefaultMesh.obj");
			if (mesh == null)
				mesh = LoadMesh ("DefaultMesh.blend");
			if (mesh == null)
				mesh = LoadMesh ("DefaultMesh.max");
			return mesh;
		}
	}
}