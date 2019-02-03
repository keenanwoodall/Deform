using UnityEngine;
using UnityEditor;

namespace DeformEditor
{
	public static class DeformEditorSettings
	{
		private static DeformEditorSettingsAsset settingsAsset;
		public static DeformEditorSettingsAsset SettingsAsset
		{
			get
			{
				if (settingsAsset == null)
					settingsAsset = DeformEditorResources.LoadAssetOfType <DeformEditorSettingsAsset> ();
				if (settingsAsset == null)
				{
					settingsAsset = ScriptableObject.CreateInstance<DeformEditorSettingsAsset> ();
					DeformEditorResources.CreateAsset (settingsAsset, "Deform/EditorResources/DeformSettings.asset");
				}
				return settingsAsset;
			}
		}

		public static Color SolidHandleColor
		{
			get => SettingsAsset.solidHandleColor;
			set => SettingsAsset.solidHandleColor = value;
		}
		public static Color LightHandleColor
		{
			get => SettingsAsset.lightHandleColor;
			set => SettingsAsset.lightHandleColor = value;
		}

		public static float DottedLineSize
		{
			get => SettingsAsset.dottedLineSize;
			set => SettingsAsset.dottedLineSize = value;
		}
		public static float ScreenspaceSliderHandleCapSize
		{
			get => SettingsAsset.screenspaceHandleCapSize;
			set => SettingsAsset.screenspaceHandleCapSize = value;
		}
		public static float ScreenspaceAngleHandleSize
		{
			get => SettingsAsset.screenspaceAngleHandleSize;
			set => SettingsAsset.screenspaceAngleHandleSize = value;
		}

		public static void SelectSettingsAsset ()
		{
			Selection.activeObject = SettingsAsset;
		}
	}
}