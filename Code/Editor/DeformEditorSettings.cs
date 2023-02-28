using UnityEngine;
using UnityEditor;

namespace DeformEditor
{
	public static class DeformEditorSettings
	{
		[InitializeOnLoadMethod]
		private static void EnsureSettingsAsset ()
		{
			if (settingsAsset == null)
				settingsAsset = DeformEditorResources.LoadAssetOfType<DeformEditorSettingsAsset> (searchAssets: DeformEditorResources.SearchFilter.Assets);
			if (settingsAsset == null)
			{
				settingsAsset = ScriptableObject.CreateInstance<DeformEditorSettingsAsset> ();
				DeformEditorResources.CreateAsset (settingsAsset, "Deform/EditorResources/DeformSettings.asset");
			}
		}

		private static DeformEditorSettingsAsset settingsAsset;
		public static DeformEditorSettingsAsset SettingsAsset
		{
			get
			{
				EnsureSettingsAsset ();
				return settingsAsset;
			}
		}

		public static Color SolidHandleColor
		{
			get => SettingsAsset.solidHandleColor;
			set
			{
				SettingsAsset.solidHandleColor = value;
				EditorUtility.SetDirty (SettingsAsset);
			}
		}
		public static Color LightHandleColor
		{
			get => SettingsAsset.lightHandleColor;
			set
			{
				SettingsAsset.lightHandleColor = value;
				EditorUtility.SetDirty (SettingsAsset);
			}
		}
		public static Color RecordingHandleColor
		{
			get => SettingsAsset.recordingHandleColor;
			set
			{
				SettingsAsset.recordingHandleColor = value;
				EditorUtility.SetDirty (SettingsAsset);
			}
		}

		public static float DottedLineSize
		{
			get => SettingsAsset.dottedLineSize;
			set
			{
				SettingsAsset.dottedLineSize = value;
				EditorUtility.SetDirty (SettingsAsset);
			}
		}
		public static float ScreenspaceSliderHandleCapSize
		{
			get => SettingsAsset.screenspaceHandleCapSize;
			set
			{
				SettingsAsset.screenspaceHandleCapSize = value;
				EditorUtility.SetDirty (SettingsAsset);
			}
		}
		public static float ScreenspaceAngleHandleSize
		{
			get => SettingsAsset.screenspaceAngleHandleSize;
			set
			{
				SettingsAsset.screenspaceAngleHandleSize = value;
				EditorUtility.SetDirty (SettingsAsset);
			}
		}
		public static float ScreenspaceLatticeHandleCapSize
		{
			get => SettingsAsset.screenspaceLatticeCapSize;
			set
			{
				SettingsAsset.screenspaceLatticeCapSize = value;
				EditorUtility.SetDirty (SettingsAsset);
			}
		}

		public static bool ModelsReadableByDefault
		{
			get => SettingsAsset.modelsReadableByDefault;
			set
			{
				SettingsAsset.modelsReadableByDefault = value;
				EditorUtility.SetDirty (SettingsAsset);
			}
		}
		
		public static void SelectSettingsAsset ()
		{
			Selection.activeObject = SettingsAsset;
		}
		
		[MenuItem ("Window/Deform/Settings", priority = 10000)]
		[MenuItem ("Tools/Deform/Settings", priority = 10000)]
		public static void ShowWindow ()
		{
			SelectSettingsAsset();
			EditorApplication.ExecuteMenuItem("Window/General/Inspector");
		}
	}
}