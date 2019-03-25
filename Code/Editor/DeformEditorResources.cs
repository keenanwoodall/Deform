using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Object = UnityEngine.Object;

namespace DeformEditor
{
	/// <summary>
	/// A class to assist in loading editor resources.
	/// </summary>
	public static class DeformEditorResources
	{
		public enum SearchFilter
		{
			All,
			Assets,
			Packages
		}

		private static GUISkin ProSkin, PersonalSkin;

		public static T LoadAssetOfType<T> (string contains = null, SearchFilter searchAssets = SearchFilter.All, Action error = null, Action success = null) where T : Object
		{
			bool allowScriptAssets = typeof (T) == typeof (MonoScript);

			T t = null;
			string[] assetGUIDs = AssetDatabase.FindAssets ($"t:{typeof (T).Name}", GetSearchDirectories (searchAssets));
			foreach (var assetGUID in assetGUIDs)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath (assetGUID);
				if (string.IsNullOrEmpty (assetPath) || !allowScriptAssets && assetPath.EndsWith (".cs") || contains != null && !Path.GetFileName (assetPath).Contains (contains))
					continue;
				t = AssetDatabase.LoadAssetAtPath<T> (assetPath);
				break;
			}

			if (t == null)
				error?.Invoke ();
			else
				success?.Invoke ();

			return t;
		}

		/// <summary>
		/// Creates asset relative to the Assets folder.
		/// </summary>
		public static void CreateAsset (Object asset, string relativePath)
		{
			EnsurePath (relativePath);
			AssetDatabase.CreateAsset (asset, $"Assets/{relativePath}");
		}

		private static void EnsurePath (string relativePath)
		{
			if (!AssetDatabase.IsValidFolder ($"Assets/{relativePath}"))
			{
				var paths = relativePath.Split ('/');
				var workingPath = "Assets";
				for (int i = 0; i < paths.Length - 1; i++)
				{
					if (!AssetDatabase.IsValidFolder ($"{workingPath}/{paths[i]}"))
						AssetDatabase.CreateFolder (workingPath, paths[i]);

					workingPath += $"/{paths[i]}";
				}
				AssetDatabase.Refresh (ImportAssetOptions.Default);
			}
		}

		private static string[] GetSearchDirectories (SearchFilter searchAssets)
		{
			string[] searchDirs;
			switch (searchAssets)
			{
				case SearchFilter.All:
					searchDirs = new[] { "Assets", "Packages" };
					break;
				case SearchFilter.Assets:
					searchDirs = new[] { "Assets" };
					break;
				case SearchFilter.Packages:
					searchDirs = new[] { "Packages" };
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (searchAssets), searchAssets, null);
			}

			return searchDirs;
		}

		public static GUISkin GetSkin ()
		{
			if (EditorGUIUtility.isProSkin)
			{
				if (ProSkin == null)
					ProSkin = LoadAssetOfType<GUISkin> (contains: "DeformProfessional");
				return ProSkin;
			}
			else
			{
				if (PersonalSkin == null)
					PersonalSkin = LoadAssetOfType<GUISkin> (contains: "DeformPersonal");
				return PersonalSkin;
			}
		}

		public static GUIStyle GetStyle (string name)
		{
			return GetSkin ().GetStyle (name);
		}

		public static Texture2D GetTexture (string name, bool appendDarkOrLightBasedOnSkin = true)
		{
			if (appendDarkOrLightBasedOnSkin)
				name += EditorGUIUtility.isProSkin ? "Light" : "Dark";

			return LoadAssetOfType<Texture2D> (name);
		}
	}
}