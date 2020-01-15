using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Deform;
using Beans.Unity.Editor;

namespace DeformEditor
{
	public class CreatorWindow : EditorWindow
	{
		private static List<DeformerAttribute> DeformerAttributes;
		private List<DeformerAttribute> filteredDeformerAttributes;

		private static class Styles
		{
			public const int MARGIN_X = 2;
			public const int MARGIN_Y = 2;

			public static readonly GUIStyle Button;

			static Styles ()
			{
				Button = new GUIStyle (EditorStyles.miniButton);
				Button.margin = new RectOffset (MARGIN_X, MARGIN_X, MARGIN_Y, MARGIN_Y);
			}
		}

		private static class Content
		{
			public static GUIContent CreateDeformable = new GUIContent (text: "Deformable", tooltip: "Create a deformable");
			public static GUIContent CreateElasticDeformable = new GUIContent (text: "Elastic Deformable (WIP)", tooltip: "Create an elastic deformable");
		}

		[SerializeField]
		private Vector2 scrollPosition;
		[SerializeField]
		private SearchField searchField;
		[SerializeField]
		private string searchQuery;
		[SerializeField]
		private Dictionary<Category, bool> categoryFoldouts = new Dictionary<Category, bool>
		{
			{ Category.Normal, true },
			{ Category.Noise, true },
			{ Category.Mask, true },
			{ Category.Utility, true },
			{ Category.WIP, true }
		};

		[MenuItem ("Window/Deform/Creator", priority = 10000)]
		[MenuItem ("Tools/Deform/Creator Window", priority = 10000)]
		public static void ShowWindow ()
		{
			GetWindow<CreatorWindow> ("Creator", true);
		}

		[UnityEditor.Callbacks.DidReloadScripts]
		private static void UpdateDeformerAttributes ()
		{
			DeformerAttributes = GetAllDeformerAttributes ().OrderBy (x => x.Name).OrderBy (x => (int)x.Category).ToList ();
		}

		private void OnEnable ()
		{
			searchField = new SearchField ();

			UpdateDeformerAttributes ();

			Undo.undoRedoPerformed += Repaint;
		}

		private void OnDisable ()
		{
			Undo.undoRedoPerformed -= Repaint;
		}

		private void OnGUI ()
		{
			EditorGUILayout.Space ();

			if (GUILayout.Button(Content.CreateDeformable, Styles.Button))
				AddOrCreateDeformable<Deformable>();
			if (GUILayout.Button(Content.CreateElasticDeformable, Styles.Button))
				AddOrCreateDeformable<ElasticDeformable>();

			using (new EditorGUILayout.HorizontalScope ())
			{
				using (var check = new EditorGUI.ChangeCheckScope ())
				{
					var rect = GUILayoutUtility.GetRect (1, 1, 18, 18, GUILayout.ExpandWidth (true));
					rect.width -= Styles.MARGIN_X * 2;
					rect.x += Styles.MARGIN_X;
					rect.y += Styles.MARGIN_Y * 2;

					var newSearchQuery = searchField.OnToolbarGUI (rect, searchQuery);
					if (check.changed)
					{
						Undo.RecordObject (this, "Changed Search Query");
						searchQuery = newSearchQuery;
					}
				}
			}

			EditorGUILayout.Space ();

			using (var scroll = new EditorGUILayout.ScrollViewScope (scrollPosition))
			{
				if (DeformerAttributes == null || DeformerAttributes.Count == 0)
					EditorGUILayout.LabelField ("No deformers found.", GUILayout.MinWidth (0));
				else
				{
					filteredDeformerAttributes =
					(
						from d in DeformerAttributes
						where string.IsNullOrEmpty (searchQuery) || d.Name.ToLower ().Contains (searchQuery.ToLower ())
						select d
					).ToList ();

					var drawnCount = 0;
					for (int i = 0; i < filteredDeformerAttributes.Count; i++)
					{
						var current = filteredDeformerAttributes[i];

						if (drawnCount == 0)
						{
							var countInCategory = filteredDeformerAttributes.Count (t => t.Category == current.Category);
							categoryFoldouts[current.Category] = EditorGUILayoutx.FoldoutHeader ($"{current.Category.ToString ()} ({countInCategory})", categoryFoldouts[current.Category], EditorStyles.label);
						}

						if (categoryFoldouts[current.Category])
							if (GUILayout.Button (new GUIContent (current.Name, current.Description), Styles.Button))
								CreateDeformerFromAttribute (current, Event.current.modifiers != EventModifiers.Alt);

						drawnCount++;

						if (i + 1 < filteredDeformerAttributes.Count)
						{
							var next = filteredDeformerAttributes[i + 1];
							if (next.Category != current.Category)
							{
								var countInCategory = filteredDeformerAttributes.Count (t => t.Category == next.Category);
								categoryFoldouts[next.Category] = EditorGUILayoutx.FoldoutHeader ($"{next.Category.ToString ()} ({countInCategory})", categoryFoldouts[next.Category], EditorStyles.label);
							}
						}
					}

					EditorGUILayout.Space ();
				}
				scrollPosition = scroll.scrollPosition;
			}
		}

		public static void AddOrCreateDeformable<T> () where T : Deformable
		{
			var targets = Selection.gameObjects;

			// If we don't have any objects selected, create a new Deformable.
			if (targets == null || targets.Length == 0)
				CreateDeformable<T> ();
			else
			{
				// Keep track of whether or not we've actually been able to add a Deformable component.
				var addedComponent = false;
				foreach (var target in Selection.gameObjects)
				{
					// Check if there's already a Deformable/
					var deformable = target.GetComponent<T> ();
					// If there isn't, we can add one
					if (!PrefabUtility.IsPartOfPrefabAsset (target) && deformable == null && MeshTarget.IsValid (target))
					{
						Undo.AddComponent<T> (target);
						addedComponent = true;
					}
				}

				// If we never ended up adding a Deformable component, we should create new one.
				if (!addedComponent)
					CreateDeformable<T> ();
			}
		}

		private static T CreateDeformable<T> () where T : Deformable
		{
			var newObject = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			newObject.name = $"{typeof(T).Name} Object";

			newObject.transform.position = SceneView.lastActiveSceneView.pivot;

			var deformable = newObject.AddComponent<T> ();
			deformable.ChangeMesh (DeformEditorResources.LoadAssetOfType<Mesh> ("DeformDefaultMesh"));

			newObject.GetComponent<Renderer> ().material = DeformEditorResources.LoadAssetOfType<Material> ("DeformDefaultMaterial");

			DestroyImmediate (newObject.GetComponent<Collider> ());

			Selection.activeGameObject = newObject;

			Undo.RegisterCreatedObjectUndo
			(
				newObject,
				$"Created {typeof(T).Name} GameObject"
			);

			return deformable;
		}

		public static void CreateDeformerFromAttribute (DeformerAttribute attribute, bool autoAdd)
		{
			var selectedGameObjects = Selection.gameObjects;
			if (selectedGameObjects == null || selectedGameObjects.Length == 0)
			{
				var newGameObject = new GameObject (attribute.Name);

				Undo.RegisterCreatedObjectUndo (newGameObject, "Created Deformer");

				newGameObject.AddComponent (attribute.Type);

				newGameObject.transform.localRotation = Quaternion.Euler (attribute.XRotation, attribute.YRotation, attribute.ZRotation);

				Selection.activeGameObject = newGameObject;
			}
			else
			{
				Undo.SetCurrentGroupName ("Created Deformer");

				var newGameObject = new GameObject (attribute.Name);
				Undo.RegisterCreatedObjectUndo (newGameObject, "Created Deformer");

				EditorGUIUtility.PingObject (newGameObject);

				var newDeformer = newGameObject.AddComponent (attribute.Type) as Deformer;

				if (autoAdd)
				{
					if (selectedGameObjects.Length == 1)
					{
						if (!PrefabUtility.IsPartOfPrefabAsset (Selection.gameObjects[0]))
						{
							var parent = selectedGameObjects[0].transform;
							newGameObject.transform.SetParent (parent, true);
							newGameObject.transform.position = parent.position;
							newGameObject.transform.rotation = parent.rotation * Quaternion.Euler (attribute.XRotation, attribute.YRotation, attribute.ZRotation);
						}
					}
					else
					{
						var center = GetAverageGameObjectPosition (selectedGameObjects);
						var rotation = Quaternion.Euler (attribute.XRotation, attribute.YRotation, attribute.ZRotation);
						newGameObject.transform.SetPositionAndRotation (center, rotation);
					}

					var deformables = GetComponents<Deformable> (selectedGameObjects);
					var groups = GetComponents<GroupDeformer> (selectedGameObjects);
					var repeaters = GetComponents<RepeaterDeformer> (selectedGameObjects);

					foreach (var deformable in deformables)
					{
						if (deformable != null && !PrefabUtility.IsPartOfPrefabAsset (deformable))
						{
							Undo.RecordObject (deformable, "Added Deformer");
							deformable.DeformerElements.Add (new DeformerElement (newDeformer));
						}
					}

					foreach (var group in groups)
					{
						if (group != null && !PrefabUtility.IsPartOfPrefabAsset (group))
						{
							Undo.RecordObject (group, "Added Deformer");
							group.DeformerElements.Add (new DeformerElement (newDeformer));
						}
					}

					foreach (var repeater in repeaters)
					{
						if (repeater != null && !PrefabUtility.IsPartOfPrefabAsset (repeater))
						{
							Undo.RecordObject (repeater, "Set Deformer");
							repeater.DeformerElement.Component = newDeformer;
						}
					}

				}
				else
					Selection.activeGameObject = newGameObject;

				Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());
			}
		}

		private static IEnumerable<T> GetComponents<T> (GameObject[] objects) where T : Component
		{
			for (int i = 0; i < objects.Length; i++)
			{
				var component = objects[i].GetComponent<T> ();
				if (component != null)
					yield return component;
			}
		}

		private static Vector3 GetAverageGameObjectPosition (GameObject[] gameObjects)
		{
			if (gameObjects == null || gameObjects.Length == 0)
				return Vector3.zero;

			var sum = Vector3.zero;
			foreach (var gameObject in gameObjects)
				sum += gameObject.transform.position;

			return sum / gameObjects.Length;
		}

		public static IEnumerable<DeformerAttribute> GetAllDeformerAttributes()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				foreach (var type in GetLoadableTypes(assembly))
				{
					if (type.IsSubclassOf(typeof(Deformer)))
					{
						var attribute = type.GetCustomAttribute<DeformerAttribute>(false);
						if (attribute != null)
							yield return attribute;
					}
				}
			}
		}

		public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(t => t != null);
			}
		}
	}
}