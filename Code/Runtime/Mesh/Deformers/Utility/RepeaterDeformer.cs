using UnityEngine;
using Unity.Jobs;

namespace Deform
{
	[Deformer (Name = "Repeater", Description = "Applies the same deformer multiple times", Type = typeof (RepeaterDeformer), Category = Category.Utility)]
	public class RepeaterDeformer : Deformer
	{
		public int Iterations
		{
			get => iterations;
			set => iterations = Mathf.Max (0, value);
		}
		public DeformerElement DeformerElement
		{
			get => deformerElement;
			set => deformerElement = value;
		}

		[SerializeField, HideInInspector] private int iterations = 1;
		[SerializeField, HideInInspector] private DeformerElement deformerElement = new DeformerElement (null);

		private DataFlags dataFlags = DataFlags.None;

		public override DataFlags DataFlags => dataFlags;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			dataFlags = DataFlags.None;

			var deformer = DeformerElement.Component;

#if UNITY_EDITOR
			if (deformer == this)
			{
				Debug.LogError ("Repeater Deformer cannot reference itself as this will create an infinite loop. Please fix this now as it will not be checked at runtime.");
				return dependency;
			}
#endif

			if (deformer == null || !DeformerElement.CanProcess ())
				return dependency;

			dataFlags |= deformer.DataFlags;

			for (int i = 0; i < Iterations; i++)
				dependency = deformer.Process (data, dependency);

			return dependency;
		}
	}
}