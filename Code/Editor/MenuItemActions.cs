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
	}
}