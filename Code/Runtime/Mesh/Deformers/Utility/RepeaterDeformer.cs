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
		public Deformer Deformer
		{
			get => deformer;
			set => deformer = value;
		}

		[SerializeField, HideInInspector] private int iterations = 1;
		[SerializeField, HideInInspector] private Deformer deformer;

		private DataFlags dataFlags = DataFlags.None;

		public override DataFlags DataFlags => dataFlags;

		public override JobHandle Process (MeshData data, JobHandle dependency = default (JobHandle))
		{
			dataFlags = DataFlags.None;

			if (Deformer == null || !Deformer.CanProcess ())
				return dependency;

			dataFlags |= Deformer.DataFlags;

			for (int i = 0; i < Iterations; i++)
				dependency = Deformer.Process (data, dependency);

			return dependency;
		}
	}
}