using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace Deform
{
	[Deformer (Name = "Group", Description = "Combines deformers into a group", Type = typeof (GroupDeformer), Category = Category.Utility)]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/GroupDeformer")]
    public class GroupDeformer : Deformer
	{
		public List<DeformerElement> DeformerElements
		{
			get => deformerElements;
			set => deformerElements = value;
		}

		[SerializeField, HideInInspector]
		private List<DeformerElement> deformerElements = new List<DeformerElement> ();

		private DataFlags dataFlags = DataFlags.None;

		public override DataFlags DataFlags => dataFlags;

		public override void PreProcess ()
		{
			foreach (var element in deformerElements)
				if (element.CanProcess ())
					element.Component.PreProcess ();
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			dataFlags = DataFlags.None;

			foreach (var element in deformerElements)
			{
#if UNITY_EDITOR
				if (element.Component == this)
				{
					Debug.LogError ("Group Deformer cannot reference itself as this will create an infinite loop. Please fix this now as it will not be checked at runtime.");
					continue;
				}
#endif
				if (element.CanProcess ())
				{
					var deformer = element.Component;
					dependency = deformer.Process (data, dependency);
					dataFlags |= deformer.DataFlags;
				}
			}

			return dependency;
		}

		public void AddDeformer (Deformer deformer, bool active = true)
		{
			DeformerElements.Add (new DeformerElement (deformer, active));
		}

		public void RemoveDeformer (Deformer deformer)
		{
			for (int i = 0; i < DeformerElements.Count; i++)
			{
				var element = DeformerElements[i];
				if (element.Component == deformer)
				{
					DeformerElements.RemoveAt (i);
					i--;
				}
			}
		}
	}
}