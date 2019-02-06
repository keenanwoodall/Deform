using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace Deform
{
	[Deformer (Name = "Group", Description = "Combines deformers into a group", Type = typeof (GroupDeformer), Category = Category.Utility)]
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
				if (element.CanProcess ())
				{
					var deformer = element.Component;
					dependency = deformer.Process (data, dependency);
					dataFlags |= deformer.DataFlags;
				}
			}

			return dependency;
		}
	}
}