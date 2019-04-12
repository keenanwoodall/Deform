using UnityEngine;
using UnityEditor;
using Beans.Unity.Editor;
using Deform;

namespace DeformEditor
{
	[CustomEditor (typeof (Deformable)), CanEditMultipleObjects]
	public class DeformableEditor : Editor
	{
		private static class Styles
		{
			public static readonly GUIStyle WrappedLabel;

			static Styles ()
			{
				WrappedLabel = new GUIStyle (EditorStyles.miniLabel);
				WrappedLabel.wordWrap = true;
			}
		}

		private static class Content
		{
			public static readonly GUIContent UpdateMode = new GUIContent (text: "Update Mode", tooltip: "Auto: Gets updated by a manager.\nPause: Never updated or reset.\nStop: Mesh is reverted to it's undeformed state until mode is switched.\nCustom: Allows updates, but not from a Deformable Manager.");
			public static readonly GUIContent NormalsRecalculation = new GUIContent (text: "Normals", tooltip: "Auto: Normals are auto calculated after the mesh is deformed; overwriting any changes made by deformers.\nNone: Normals aren't modified by the Deformable.");
			public static readonly GUIContent BoundsRecalculation = new GUIContent (text: "Bounds", tooltip: "Auto: Bounds are recalculated for any deformers that need it, and at the end after all the deformers finish.\nNever: Bounds are never recalculated.\nOnce At The End: Deformers that needs updated bounds are ignored and bounds are only recalculated at the end.");
			public static readonly GUIContent ColliderRecalculation = new GUIContent (text: "Collider", tooltip: "Auto: Collider's mesh is updated when the rendered mesh is updated.\nNone: Collider's mesh isn't updated.");
			public static readonly GUIContent MeshCollider = new GUIContent (text: "Mesh Collider", tooltip: "The Mesh Collider to sync with the deformed mesh. To improve performance, try turning off different cooking options on the Mesh Collider (Especially 'Cook For Faster Simulation').");
			public static readonly GUIContent CustomBounds = new GUIContent (text: "Custom Bounds", tooltip: "The bounds used by the mesh when bounds recalculation is set to 'Custom.'");

			public static readonly string ReadWriteNotEnableAlert = "Read/Write permissions must be enabled on the target mesh.";

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
			public SerializedProperty NormalsRecalculation;
			public SerializedProperty BoundsRecalculation;
			public SerializedProperty ColliderRecalculation;
			public SerializedProperty MeshCollider;
			public SerializedProperty CustomBounds;

			public Properties (SerializedObject obj)
			{
				UpdateMode				= obj.FindProperty ("updateMode");
				NormalsRecalculation	= obj.FindProperty ("normalsRecalculation");
				BoundsRecalculation		= obj.FindProperty ("boundsRecalculation");
				ColliderRecalculation	= obj.FindProperty ("colliderRecalculation");
				MeshCollider			= obj.FindProperty ("meshCollider");
				CustomBounds			= obj.FindProperty ("customBounds");
			}
		}


		private Properties properties;
		private ReorderableComponentElementList<Deformer> deformerList;

		private bool foldoutDebug;

		private void OnEnable ()
		{
			properties = new Properties (serializedObject);

			deformerList = new ReorderableComponentElementList<Deformer> (serializedObject, serializedObject.FindProperty ("deformerElements"));
		}

		private void OnDisable ()
		{
			deformerList.Dispose ();
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.UpdateIfRequiredOrScript ();
			using (var check = new EditorGUI.ChangeCheckScope ())
			{
				EditorGUILayout.PropertyField (properties.UpdateMode, Content.UpdateMode);
				if (check.changed)
				{
					serializedObject.ApplyModifiedProperties ();
					foreach (var t in targets)
						((Deformable)t).UpdateMode = (UpdateMode)properties.UpdateMode.enumValueIndex;
				}
			}

			EditorGUILayout.PropertyField (properties.NormalsRecalculation, Content.NormalsRecalculation);
			EditorGUILayout.PropertyField (properties.BoundsRecalculation, Content.BoundsRecalculation);

			if (properties.BoundsRecalculation.hasMultipleDifferentValues || (BoundsRecalculation)properties.BoundsRecalculation.enumValueIndex == BoundsRecalculation.Custom)
			{
				using (new EditorGUI.IndentLevelScope ())
				{
					EditorGUILayout.PropertyField (properties.CustomBounds, Content.CustomBounds);
				}
			}

			EditorGUILayout.PropertyField (properties.ColliderRecalculation, Content.ColliderRecalculation);
			if (properties.ColliderRecalculation.hasMultipleDifferentValues || (ColliderRecalculation)properties.ColliderRecalculation.enumValueIndex == ColliderRecalculation.Auto)
			{
				using (new EditorGUI.IndentLevelScope ())
					EditorGUILayout.PropertyField (properties.MeshCollider, Content.MeshCollider);
			}

			EditorGUILayout.Space ();

			deformerList.DoLayoutList ();

			var newDeformers = EditorGUILayoutx.DragAndDropArea<Deformer> ();
			if (newDeformers != null && newDeformers.Count > 0)
			{
				Undo.RecordObjects (targets, "Added Deformers");
				foreach (var t in targets)
				{
					var elements = ((Deformable)t).DeformerElements;
					foreach (var newDeformer in newDeformers)
						elements.Add (new DeformerElement (newDeformer));
				}

				// I'd like to give a massive thanks and credit to Thomas Ingram for taking time out of his day to
				// solve an abomination of a bug and find this fix. He truly is an editor scripting legend.
				// Changing fields directly with multiple objects selected doesn't dirty the serialized object for some reason.
				// To force it to be dirty you have to call this method.
				serializedObject.SetIsDifferentCacheDirty ();
				serializedObject.Update ();
			}

			EditorGUILayout.Space ();

			using (new EditorGUILayout.HorizontalScope ())
			{
				var selectedIndex = GUILayout.Toolbar (-1, Content.UtilityToolbar, EditorStyles.miniButton, GUILayout.MinWidth (0));
				switch (selectedIndex)
				{
					default:
						throw new System.ArgumentException ($"No valid action for toolbar index {selectedIndex}.");
					case -1:
						break;
					case 0:
						Undo.RecordObjects (targets, "Cleared Deformers");
						foreach (var t in targets)
							((Deformable)t).DeformerElements.Clear ();
						break;
					case 1:
						Undo.RecordObjects (targets, "Cleaned Deformers");
						foreach (var t in targets)
							((Deformable)t).DeformerElements.RemoveAll (d => d.Component == null);
						break;
					case 2:
						foreach (var t in targets)
						{
							var deformable = t as Deformable;

							// C:/...<ProjectName>/Assets/
							var projectPath = Application.dataPath + "/";
							// We have to generate the full asset path starting from the Assets folder for GeneratorUniqueAssetPath to work,
							var assetPath = EditorUtility.SaveFilePanelInProject ("Save Obj", $"{deformable.name}.obj", "obj", "");
							if (string.IsNullOrEmpty (assetPath))
								break;
							// Now that we have a unique asset path we can remove the "Assets/" and ".obj" to get the unique name.
							var fileName = assetPath;
							// It's pretty gross, but it works and this code doesn't need to be performant.
							fileName = fileName.Remove (0, 7);
							fileName = fileName.Remove (fileName.Length - 4, 4);

							ObjExporter.SaveMesh (deformable.GetMesh (), deformable.GetRenderer (), projectPath, fileName);
							AssetDatabase.Refresh (ImportAssetOptions.ForceSynchronousImport);
						}
						break;
					case 3:
						foreach (var t in targets)
						{
							var deformable = t as Deformable;

							var assetPath = EditorUtility.SaveFilePanelInProject ("Save Mesh Asset", $"{deformable.name}.asset", "asset", "");
							if (string.IsNullOrEmpty (assetPath))
								break;

							AssetDatabase.CreateAsset (Instantiate (deformable.GetMesh ()), assetPath);
							AssetDatabase.SaveAssets ();
						}
						break;
				}
			}

			EditorGUILayout.Space ();

			if (foldoutDebug = EditorGUILayoutx.FoldoutHeader ("Debug Info", foldoutDebug))
			{
				var vertexCount = 0;
				var modifiedData = DataFlags.None;
				var bounds = (target as Deformable).GetMesh ().bounds;
				foreach (var t in targets)
				{
					var deformable = t as Deformable;
					var mesh = deformable.GetMesh ();

					if (mesh != null)
						vertexCount += deformable.GetMesh ().vertexCount;
					modifiedData |= deformable.ModifiedDataFlags;
				}

				EditorGUILayout.LabelField ($"Vertex Count: {vertexCount}", Styles.WrappedLabel);
				EditorGUILayout.LabelField ($"Modified Data: {modifiedData.ToString ()}", Styles.WrappedLabel);
				EditorGUILayout.LabelField ($"Bounds: {bounds.ToString ()}", Styles.WrappedLabel);
			}

			serializedObject.ApplyModifiedProperties ();

			foreach (var t in targets)
			{
				var deformable = t as Deformable;

				var originalMesh = deformable.GetOriginalMesh ();
				if (originalMesh != null && !originalMesh.isReadable)
					EditorGUILayout.HelpBox (Content.ReadWriteNotEnableAlert, MessageType.Error);
			}

			EditorApplication.QueuePlayerLoopUpdate ();
		}

		private void OnSceneGUI ()
		{
			if (foldoutDebug)
			{
				var deformable = target as Deformable;

				DeformHandles.Bounds (deformable.GetMesh ().bounds, deformable.transform.localToWorldMatrix, DeformHandles.LineMode.LightDotted);
			}
		}

		[MenuItem ("CONTEXT/Deformable/Strip")]
		private static void Strip (MenuCommand command)
		{
			var deformable = (Deformable)command.context;

			Undo.SetCurrentGroupName ("Strip Selected Deformables");
			Undo.RecordObject (deformable, "Changed Assign Original Mesh On Disable");
			deformable.assignOriginalMeshOnDisable = false;
			Undo.DestroyObjectImmediate (deformable);
		}
	}
}
