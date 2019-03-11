using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;

namespace DeformEditor
{
	public class DeformSettingsWindow : EditorWindow
	{
		private class Content
		{
			public GUIContent SolidHandleColor = new GUIContent (text: "Solid Color");
			public GUIContent LightHandleColor = new GUIContent (text: "Light Color");
			public GUIContent DottedLineSize = new GUIContent (text: "Dotted Line Size");
			public GUIContent ScreenspaceSliderHandleCapSize= new GUIContent (text: "Handle Size");
			public GUIContent ScreenspaceAngleHandleSize = new GUIContent (text: "Angle Handle Size");
		}

		private Content content = new Content ();
		private SerializedObject serializedAsset;

		[MenuItem ("Window/Deform/Settings", priority = 10000)]
		[MenuItem ("Tools/Deform/Settings", priority = 10000)]
		public static void ShowWindow ()
		{
			GetWindow<DeformSettingsWindow> ("Deform Settings", true);
		}

		private void OnEnable ()
		{
			serializedAsset = new SerializedObject (DeformEditorSettings.SettingsAsset);
			Undo.undoRedoPerformed += Repaint;
		}

		private void OnDisable ()
		{
			serializedAsset.Dispose ();
			Undo.undoRedoPerformed -= Repaint;
		}

		private void OnGUI ()
		{
			var foldoutProperty = serializedAsset.FindProperty ("sceneFoldoutOpen");
			if (foldoutProperty.boolValue = EditorGUILayoutx.DrawFoldoutHeader ("Scene", foldoutProperty.boolValue))
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var solidColor = EditorGUILayout.ColorField (content.SolidHandleColor, DeformEditorSettings.SolidHandleColor);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Solid Color Settings");
						DeformEditorSettings.SolidHandleColor = solidColor;
					}
				}
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var lightColor = EditorGUILayout.ColorField (content.LightHandleColor, DeformEditorSettings.LightHandleColor);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Light Color Settings");
						DeformEditorSettings.LightHandleColor = lightColor;
					}
				}
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var dottedLineSize = EditorGUILayout.FloatField (content.DottedLineSize, DeformEditorSettings.DottedLineSize);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Dotted Line Size Settings");
						DeformEditorSettings.DottedLineSize = dottedLineSize;
					}
				}
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var handleSize = EditorGUILayout.FloatField (content.ScreenspaceSliderHandleCapSize, DeformEditorSettings.ScreenspaceSliderHandleCapSize);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Handle Size Settings");
						DeformEditorSettings.ScreenspaceSliderHandleCapSize = handleSize;
					}
				}
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var angleHandleSize = EditorGUILayout.FloatField (content.ScreenspaceAngleHandleSize, DeformEditorSettings.ScreenspaceAngleHandleSize);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Angle Handle Size Settings");
						DeformEditorSettings.ScreenspaceAngleHandleSize = angleHandleSize;
					}
				}
			}
		}
	}
}