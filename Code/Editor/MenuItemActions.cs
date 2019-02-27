using UnityEngine;
using UnityEditor;
using Deform;

namespace DeformEditor
{
	public static class MenuItemActions
	{
		[MenuItem ("Tools/Deform/Actions/Clean All Deformables In Scene", priority = 10100)]
		public static void CleanDeformablesInScene ()
		{
			var deformables = GameObject.FindObjectsOfType<Deformable> ();

			Undo.RecordObjects (deformables, "Cleaned All Deformables In Scene");
			foreach (var deformable in deformables)
				deformable.DeformerElements.RemoveAll (t => t.Component == null);
		}

		[MenuItem ("Tools/Deform/Actions/Strip Selected Deformables", priority = 10101)]
		public static void StripDeformableFromMesh ()
		{
			var selections = Selection.gameObjects;
			Undo.SetCurrentGroupName ("Stripped Deformables");
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
	}
}