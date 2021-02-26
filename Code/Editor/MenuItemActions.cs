using UnityEngine;
using UnityEditor;
using Deform;
using System.Collections.Generic;
using System.Linq;

namespace DeformEditor
{
	public static class MenuItemActions
	{
		[MenuItem ("Tools/Deform/Actions/Clean All Deformables", priority = 10100)]
		public static void CleanAllDeformables ()
		{
			var deformables = GameObject.FindObjectsOfType<Deformable> ();

			Undo.RecordObjects (deformables, "Cleaned All Deformables");
			foreach (var deformable in deformables)
				deformable.DeformerElements.RemoveAll (t => t.Component == null);
			
			var groupDeformers = GameObject.FindObjectsOfType<GroupDeformer> ();

			Undo.RecordObjects (groupDeformers, "Cleaned All Deformables");
			foreach (var groupDeformer in groupDeformers)
				groupDeformer.DeformerElements.RemoveAll (t => t.Component == null);

			EditorUtility.DisplayDialog ("Cleaned All Deformables", $"{deformables.Length + groupDeformers.Length} found and cleaned.", "OK");
		}

		private static void StripDeformable(Deformable deformable)
		{
			Undo.RecordObject (deformable, "Changed Assign Original Mesh On Disable");
			// Make sure the meshes are up to date before stripping (we don't want it being culled when stripped stopping preventing the correct mesh being baked out)
			deformable.ForceImmediateUpdate();
			deformable.assignOriginalMeshOnDisable = false;
			Undo.DestroyObjectImmediate (deformable);
		}

		[MenuItem ("Tools/Deform/Actions/Strip All Deformables", priority = 10101)]
		public static void StripAllDeformablesFromMeshes ()
		{
			var deformables = GameObject.FindObjectsOfType<Deformable> ();
			Undo.SetCurrentGroupName ("Stripped All Deformables");
			foreach (var deformable in deformables)
			{
				StripDeformable(deformable);
			}

			EditorUtility.DisplayDialog ("Stripped All Deformables", $"{deformables.Length} found and stripped.", "OK");
		}

		[MenuItem ("Tools/Deform/Actions/Strip Selected Deformables", priority = 10102)]
		public static void StripSelectedDeformablesFromMeshes ()
		{
			var selections = Selection.gameObjects;
			Undo.SetCurrentGroupName ("Stripped Selected Deformables");
			foreach (var selection in selections)
			{
				var deformable = selection.GetComponent<Deformable> ();
				if (deformable != null)
				{
					StripDeformable(deformable);
				}
			}
		}
		[MenuItem ("Tools/Deform/Actions/Strip Selected Deformables", validate = true)]
		private static bool CanStripDeformableFromMesh ()
		{
			var selections = Selection.gameObjects;
			foreach (var selection in selections)
				if (selection.GetComponent<Deformable> () != null)
					return true;
			return false;
		}

		[MenuItem ("Tools/Deform/Actions/Make Immediate Children Deformable", priority = 10103)]
		public static void MakeImmediateChildrenDeformables ()
		{
			var newSelection = new HashSet<GameObject> ();

			var selections = Selection.gameObjects;
			Undo.SetCurrentGroupName ("Made Immediate Children Deformable");
			foreach (var selection in selections)
			{
				foreach (Transform child in selection.transform)
				{
					if (child.GetComponent<Deformable> ())
						continue;
					if (MeshTarget.IsValid (child.gameObject))
						newSelection.Add (Undo.AddComponent<Deformable> (child.gameObject).gameObject);
				}
			}

			Selection.objects = newSelection.ToArray ();
		}
		
		[MenuItem ("Tools/Deform/Actions/Make All Children Deformable", priority = 10104)]
		public static void MakeAllChildrenDeformables ()
		{
			var newSelection = new HashSet<GameObject> ();

			var selections = Selection.gameObjects;
			Undo.SetCurrentGroupName ("Made All Children Deformable");
			foreach (var selection in selections)
			{
				var allChildren = selection.transform.GetComponentsInChildren<Transform>();
				foreach (Transform child in allChildren)
				{
					if (child.GetComponent<Deformable> ())
						continue;
					if (MeshTarget.IsValid (child.gameObject))
						newSelection.Add (Undo.AddComponent<Deformable> (child.gameObject).gameObject);
				}
			}

			Selection.objects = newSelection.ToArray ();
		}

		[MenuItem ("Tools/Deform/Report Bug", priority = 10203)]
		public static void ReportBug ()
		{
			Application.OpenURL ("https://github.com/keenanwoodall/Deform/issues/new");
		}
	}
}