using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;

namespace DeformEditor
{
	public class DeformSettingsWindow : EditorWindow
	{
		private static class Content
		{
			public static GUIContent SolidHandleColor = new GUIContent (text: "Solid Color");
			public static GUIContent LightHandleColor = new GUIContent (text: "Light Color");
			public static GUIContent DottedLineSize = new GUIContent (text: "Dotted Line Size");
			public static GUIContent ScreenspaceSliderHandleCapSize= new GUIContent (text: "Handle Size");
			public static GUIContent ScreenspaceAngleHandleSize = new GUIContent (text: "Angle Handle Size");
			public static GUIContent ModelsReadableByDefault = new GUIContent (text: "Models Readable By Default", tooltip: "When true, any newly imported models will be marked as readable.");
		}

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
			var sceneExpandedProperty = serializedAsset.FindProperty (nameof(DeformEditorSettingsAsset.dottedLineSize));
			if (sceneExpandedProperty.isExpanded = EditorGUILayoutx.FoldoutHeader ("Scene", sceneExpandedProperty.isExpanded))
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var solidColor = EditorGUILayout.ColorField (Content.SolidHandleColor, DeformEditorSettings.SolidHandleColor);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Solid Color Settings");
						DeformEditorSettings.SolidHandleColor = solidColor;
					}
				}
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var lightColor = EditorGUILayout.ColorField (Content.LightHandleColor, DeformEditorSettings.LightHandleColor);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Light Color Settings");
						DeformEditorSettings.LightHandleColor = lightColor;
					}
				}
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var dottedLineSize = EditorGUILayout.FloatField (Content.DottedLineSize, DeformEditorSettings.DottedLineSize);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Dotted Line Size Settings");
						DeformEditorSettings.DottedLineSize = dottedLineSize;
					}
				}
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var handleSize = EditorGUILayout.FloatField (Content.ScreenspaceSliderHandleCapSize, DeformEditorSettings.ScreenspaceSliderHandleCapSize);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Handle Size Settings");
						DeformEditorSettings.ScreenspaceSliderHandleCapSize = handleSize;
					}
				}
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var angleHandleSize = EditorGUILayout.FloatField (Content.ScreenspaceAngleHandleSize, DeformEditorSettings.ScreenspaceAngleHandleSize);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Angle Handle Size Settings");
						DeformEditorSettings.ScreenspaceAngleHandleSize = angleHandleSize;
					}
				}
			}
			
			var importerExpandedProperty = serializedAsset.FindProperty (nameof(DeformEditorSettingsAsset.modelsReadableByDefault));
			if (importerExpandedProperty.isExpanded = EditorGUILayoutx.FoldoutHeader("Importer", importerExpandedProperty.isExpanded))
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var modelsReadableByDefault = EditorGUILayout.Toggle(Content.ModelsReadableByDefault, DeformEditorSettings.ModelsReadableByDefault);
					if (check.changed)
					{
						Undo.RecordObject (DeformEditorSettings.SettingsAsset, "Changed Models Readable By Default");
						DeformEditorSettings.ModelsReadableByDefault = modelsReadableByDefault;
					}
				}
			}
		}
	}
}