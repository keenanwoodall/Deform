using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor(typeof(Deformable), editorForChildClasses: true), CanEditMultipleObjects]
	public class DeformableEditor : Editor
	{
		private static class Styles
		{
			public static readonly GUIStyle WrappedLabel;

			static Styles()
			{
				WrappedLabel = new GUIStyle(EditorStyles.miniLabel);
				WrappedLabel.wordWrap = true;
			}
		}

		private static class Content
		{
			public static readonly GUIContent UpdateMode = new GUIContent(text: "Update Mode", tooltip: "Auto: Gets updated by a manager.\nPause: Never updated or reset.\nStop: Mesh is reverted to it's undeformed state until mode is switched.\nCustom: Allows updates, but not from a Deformable Manager.");
			public static readonly GUIContent CullingMode = new GUIContent(text: "Culling Mode", tooltip: "Always Update: Update everything regardless of renderer visibility.\n\nDon't Update: Do not update unless renderer is visible. When the deformers aren't recalculated, bounds cannot be updated which may result in animated deformables not reappearing on screen.");
			public static readonly GUIContent StripMode = new GUIContent(text: "Strip Mode", tooltip: "Strip:\nThis deformable will only exist in edit mode.\n\nDon't Strip:\nThis deformable will continue at runtime.");
			public static readonly GUIContent NormalsRecalculation = new GUIContent(text: "Normals", tooltip: "Auto: Normals are auto calculated after the mesh is deformed; overwriting any changes made by deformers.\nNone: Normals aren't modified by the Deformable.");
			public static readonly GUIContent BoundsRecalculation = new GUIContent(text: "Bounds", tooltip: "Auto: Bounds are recalculated for any deformers that need it, and at the end after all the deformers finish.\nNever: Bounds are never recalculated.\nOnce At The End: Deformers that needs updated bounds are ignored and bounds are only recalculated at the end.");
			public static readonly GUIContent ColliderRecalculation = new GUIContent(text: "Collider", tooltip: "Auto: Collider's mesh is updated when the rendered mesh is updated.\nNone: Collider's mesh isn't updated.");
			public static readonly GUIContent MeshCollider = new GUIContent(text: "Mesh Collider", tooltip: "The Mesh Collider to sync with the deformed mesh. To improve performance, try turning off different cooking options on the Mesh Collider (Especially 'Cook For Faster Simulation').");
			public static readonly GUIContent CustomBounds = new GUIContent(text: "Custom Bounds");
			public static readonly GUIContent ApplyBounds = new GUIContent(text: "Apply Bounds", tooltip: "Applies the currently recorded bounds.");

			public static readonly string ReadWriteNotEnableAlert = "Read/Write permissions must be enabled on the target mesh.";
			public static readonly string StaticBatchingAlert = "Deformable will be stripped at runtime when static batching is enabled.";
			public static readonly string FixReadWriteNotEnabled = "Fix It!";

			public static readonly GUIContent[] UtilityToolbar =
			{
				new GUIContent (text: "Clear", tooltip: "Remove all deformers from the deformer list."),
				new GUIContent (text: "Clean", tooltip: "Remove all null deformers from the deformer list."),
				new GUIContent (text: "Save Obj", tooltip: "Save the current mesh as a .obj file in the project. (Doesn't support vertex colors)"),
				new GUIContent (text: "Save Asset", tooltip: "Save the current mesh as a mesh asset file in the project.")
			};
		}

		private class Properties
		{
			public SerializedProperty UpdateMode;
			public SerializedProperty CullingMode;
			public SerializedProperty StripMode;
			public SerializedProperty NormalsRecalculation;
			public SerializedProperty BoundsRecalculation;
			public SerializedProperty ColliderRecalculation;
			public SerializedProperty MeshCollider;
			public SerializedProperty CustomBounds;

			public Properties(SerializedObject obj)
			{
				UpdateMode = obj.FindProperty("updateMode");
				CullingMode = obj.FindProperty("cullingMode");
				StripMode = obj.FindProperty("stripMode");
				NormalsRecalculation = obj.FindProperty("normalsRecalculation");
				BoundsRecalculation = obj.FindProperty("boundsRecalculation");
				ColliderRecalculation = obj.FindProperty("colliderRecalculation");
				MeshCollider = obj.FindProperty("meshCollider");
				CustomBounds = obj.FindProperty("customBounds");
			}
		}

		protected Action overrideCullingModeGUI;
		protected Action overrideStripModeGUI;

		private Properties properties;
		private ReorderableComponentElementList<Deformer> deformerList;

		private static bool foldoutDebug;
		private GUIContent record;
		private GUIStyle redBox;
		private bool recording;

		protected virtual void OnEnable()
		{
			record = new GUIContent(DeformEditorResources.GetTexture("Record", false), "Record current bounds");
			properties = new Properties(serializedObject);

			deformerList = new ReorderableComponentElementList<Deformer>(serializedObject, serializedObject.FindProperty("deformerElements"));
		}

		protected virtual void OnDisable()
		{
			deformerList.Dispose();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();

			DrawMainSettings();
			EditorGUILayout.Space();
			DrawDeformersList();
			EditorGUILayout.Space();
			DrawUtilityToolbar();
			EditorGUILayout.Space();
			DrawDebugInfo();
			DrawHelpBoxes();

			if (serializedObject.ApplyModifiedProperties())
				EditorApplication.QueuePlayerLoopUpdate();
		}

		protected virtual void DrawMainSettings()
		{
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				EditorGUILayout.PropertyField(properties.UpdateMode, Content.UpdateMode);
				if (check.changed)
				{
					serializedObject.ApplyModifiedProperties();
					foreach (var t in targets)
						((Deformable)t).UpdateMode = (UpdateMode)properties.UpdateMode.enumValueIndex;
				}
			}

			if (overrideCullingModeGUI != null)
				overrideCullingModeGUI.Invoke();
			else
				EditorGUILayout.PropertyField(properties.CullingMode, Content.CullingMode);

			if (overrideStripModeGUI != null)
				overrideStripModeGUI.Invoke();
			else
			{
				var batchingStatic = targets.Select(t => ((Deformable)t).gameObject).Any(go =>
					GameObjectUtility.AreStaticEditorFlagsSet(go, StaticEditorFlags.BatchingStatic));
				if (!batchingStatic)
				{
					using (new EditorGUI.DisabledScope(Application.isPlaying))
						EditorGUILayout.PropertyField(properties.StripMode, Content.StripMode);
				}
				else
					using (new EditorGUI.DisabledScope(true))
					{
						EditorGUILayout.EnumPopup(Content.StripMode, StripMode.Strip);
						EditorGUILayout.HelpBox(new GUIContent(Content.StaticBatchingAlert));
					}
			}

			EditorGUILayout.PropertyField(properties.NormalsRecalculation, Content.NormalsRecalculation);
			EditorGUILayout.PropertyField(properties.BoundsRecalculation, Content.BoundsRecalculation);

			if (properties.BoundsRecalculation.hasMultipleDifferentValues || (BoundsRecalculation)properties.BoundsRecalculation.enumValueIndex == BoundsRecalculation.Custom)
			{
				if (target is Deformable deformable)
				{
					var originalBackgroundColor = GUI.backgroundColor;

					var mesh = deformable.GetMesh();
					using (new EditorGUI.IndentLevelScope())
					using (new EditorGUILayout.HorizontalScope())
					{
						if (recording)
						{
							GUI.backgroundColor = Color.red;
							mesh.RecalculateBounds();
							var bounds = deformable.GetMesh().bounds;
							EditorGUILayout.BoundsField(properties.CustomBounds.displayName, bounds);
							GUI.backgroundColor = originalBackgroundColor;
						}
						else
						{
							EditorGUILayout.PropertyField(properties.CustomBounds, Content.CustomBounds);
						}

						var buttonStyle = ((GUIStyle) "button");

						var boundsLabelSize = EditorStyles.label.CalcSize(Content.CustomBounds);
						var recordRect = GUILayoutUtility.GetLastRect();
						recordRect.xMin += boundsLabelSize.x + 6;
						recordRect = EditorGUI.IndentedRect(recordRect);
						var recordSize = buttonStyle.CalcSize(record);
						recordRect.width = recordSize.x;
						recordRect.height = recordSize.y;

						var wasRecording = recording;
						if (recording = GUI.Toggle(recordRect, recording, record, buttonStyle))
						{
							var applyRect = recordRect;
							var applySize = buttonStyle.CalcSize(Content.ApplyBounds);
							applyRect.width = applySize.x;
							applyRect.height = applySize.y;
							applyRect.position += Vector2.right * (recordRect.width + 6);

							if (GUI.Button(applyRect, Content.ApplyBounds))
							{
								Undo.RecordObjects(targets, "Apply Recorded Bounds");
								recording = false;
								foreach (var t in targets)
								{
									if (!(t is Deformable d)) continue;
									var m = d.GetMesh();
									m.RecalculateBounds();
									d.CustomBounds = m.bounds;
								}
							}
						}

						GUI.backgroundColor = originalBackgroundColor;
						
						if (wasRecording != recording)
							SceneView.RepaintAll();
					}
				}
			}

			EditorGUILayout.PropertyField(properties.ColliderRecalculation, Content.ColliderRecalculation);
			if (properties.ColliderRecalculation.hasMultipleDifferentValues || (ColliderRecalculation)properties.ColliderRecalculation.enumValueIndex == ColliderRecalculation.Auto)
			{
				using (new EditorGUI.IndentLevelScope())
					EditorGUILayout.PropertyField(properties.MeshCollider, Content.MeshCollider);
			}
		}

		protected virtual void DrawDeformersList()
		{
			deformerList.DoLayoutList();

			var newDeformers = EditorGUILayoutx.DragAndDropArea<Deformer>();
			if (newDeformers != null && newDeformers.Count > 0)
			{
				Undo.RecordObjects(targets, "Added Deformers");
				foreach (var t in targets)
				{
					var elements = ((Deformable)t).DeformerElements;
					foreach (var newDeformer in newDeformers)
						elements.Add(new DeformerElement(newDeformer));
				}

				// I'd like to give a massive thanks and credit to Thomas Ingram for taking time out of his day to
				// solve an abomination of a bug and find this fix. He truly is an editor scripting legend.
				// Changing fields directly with multiple objects selected doesn't dirty the serialized object for some reason.
				// To force it to be dirty you have to call this method.
				serializedObject.SetIsDifferentCacheDirty();
				serializedObject.Update();
			}
		}

		protected virtual void DrawUtilityToolbar()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				var selectedIndex = GUILayout.Toolbar(-1, Content.UtilityToolbar, EditorStyles.miniButton, GUILayout.MinWidth(0));
				switch (selectedIndex)
				{
					default:
						throw new System.ArgumentException($"No valid action for toolbar index {selectedIndex}.");
					case -1:
						break;
					case 0:
						Undo.RecordObjects(targets, "Cleared Deformers");
						foreach (var t in targets)
							((Deformable)t).DeformerElements.Clear();
						break;
					case 1:
						Undo.RecordObjects(targets, "Cleaned Deformers");
						foreach (var t in targets)
							((Deformable)t).DeformerElements.RemoveAll(d => d.Component == null);
						break;
					case 2:
						foreach (var t in targets)
						{
							var deformable = t as Deformable;

							// C:/...<ProjectName>/Assets/
							var projectPath = Application.dataPath + "/";
							var assetPath = EditorUtility.SaveFilePanelInProject("Save Obj", $"{deformable.name}.obj", "obj", "");
							if (string.IsNullOrEmpty(assetPath))
								break;
							var fileName = assetPath;
							// Now that we have a unique asset path we can remove the "Assets/" and ".obj" to get the unique name.
							// It's pretty gross, but it works and this code doesn't need to be performant.
							fileName = fileName.Remove(0, 7);
							fileName = fileName.Remove(fileName.Length - 4, 4);

							ObjExporter.SaveMesh(deformable.GetMesh(), deformable.GetRenderer(), projectPath, fileName);
							AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
						}
						break;
					case 3:
						foreach (var t in targets)
						{
							var deformable = t as Deformable;

							var assetPath = EditorUtility.SaveFilePanelInProject("Save Mesh Asset", $"{deformable.name}.asset", "asset", "");
							if (string.IsNullOrEmpty(assetPath))
								break;

							AssetDatabase.CreateAsset(Instantiate(deformable.GetMesh()), assetPath);
							AssetDatabase.SaveAssets();
						}
						break;
				}
			}
		}

		protected virtual void DrawDebugInfo()
		{
			if (foldoutDebug = EditorGUILayoutx.FoldoutHeader("Debug Info", foldoutDebug))
			{
				var vertexCount = 0;
				var modifiedData = DataFlags.None;
				var bounds = (target as Deformable).GetCurrentMesh().bounds;
				foreach (var t in targets)
				{
					var deformable = t as Deformable;
					var mesh = deformable.GetMesh();

					if (mesh != null)
						vertexCount += deformable.GetMesh().vertexCount;
					modifiedData |= deformable.ModifiedDataFlags;
				}

				EditorGUILayout.LabelField($"Vertex Count: {vertexCount}", Styles.WrappedLabel);
				EditorGUILayout.LabelField($"Modified Data: {modifiedData.ToString()}", Styles.WrappedLabel);
				EditorGUILayout.LabelField($"Bounds: {bounds.ToString()}", Styles.WrappedLabel);
			}
		}

		protected virtual void DrawHelpBoxes()
		{
			foreach (var t in targets)
			{
				var deformable = t as Deformable;

				var originalMesh = deformable.GetOriginalMesh();
				if (originalMesh != null && !originalMesh.isReadable)
				{
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.HelpBox(Content.ReadWriteNotEnableAlert, MessageType.Error);
						if (GUILayout.Button(Content.FixReadWriteNotEnabled, GUILayout.Width(50f), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing)))
						{
							if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(originalMesh)) is ModelImporter importer)
							{
								importer.isReadable = true;
								importer.SaveAndReimport();
							}
						}
					}
				}
			}
		}

		protected virtual void OnSceneGUI()
		{
			if (!(target is Deformable deformable))
				return;
			if (recording)
			{
				deformable.GetCurrentMesh().RecalculateBounds();
				DeformHandles.Bounds(deformable.GetCurrentMesh().bounds, deformable.transform.localToWorldMatrix, DeformHandles.LineMode.SolidDotted, DeformEditorSettings.RecordingHandleColor);
			}
			else if (foldoutDebug || deformable.BoundsRecalculation == BoundsRecalculation.Custom)
			{
				DeformHandles.Bounds(deformable.GetCurrentMesh().bounds, deformable.transform.localToWorldMatrix, DeformHandles.LineMode.LightDotted);
			}
		}

		[MenuItem("CONTEXT/Deformable/Strip")]
		private static void Strip(MenuCommand command)
		{
			var deformable = (Deformable)command.context;

			Undo.SetCurrentGroupName("Strip Selected Deformables");
			Undo.RecordObject(deformable, "Changed Assign Original Mesh On Disable");
			deformable.assignOriginalMeshOnDisable = false;
			Undo.DestroyObjectImmediate(deformable);
		}
	}
}