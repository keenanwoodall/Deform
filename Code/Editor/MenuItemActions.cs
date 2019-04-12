using UnityEngine;
using UnityEditor;
using Deform;
using System.Collections.Generic;

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

			EditorUtility.DisplayDialog ("Cleaned All Deformables", $"{deformables.Length} found and cleaned.", "OK");
		}

		[MenuItem ("Tools/Deform/Actions/Strip All Deformables", priority = 10101)]
		public static void StripAllDeformablesFromMeshes ()
		{
			var deformables = GameObject.FindObjectsOfType<Deformable> ();
			Undo.SetCurrentGroupName ("Stripped All Deformables");
			foreach (var deformable in deformables)
			{
				Undo.RecordObject (deformable, "Changed Assign Original Mesh On Disable");
				deformable.assignOriginalMeshOnDisable = false;
				Undo.DestroyObjectImmediate (deformable);
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
					Undo.RecordObject (deformable, "Changed Assign Original Mesh On Disable");
					deformable.assignOriginalMeshOnDisable = false;
					Undo.DestroyObjectImmediate (deformable);
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

		[MenuItem ("Tools/Deform/Actions/Make Children Deformable", priority = 10103)]
		public static void MakeChildrenDeformables ()
		{
			var newSelection = new List<GameObject> ();

			var selections = Selection.gameObjects;
			Undo.SetCurrentGroupName ("Made Children Deformable");
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

		[MenuItem ("Tools/Deform/Report Bug", priority = 10203)]
		public static void ReportBug ()
		{
			Application.OpenURL ("https://github.com/keenanwoodall/Deform/issues/new");
		}
	}
}