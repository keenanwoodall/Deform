using UnityEngine;
using Unity.Jobs;

namespace Deform
{
	[Deformer (Name = "Recalculate Bounds", Description = "Recalculates the bounds", Type = typeof (RecalculateBoundsDeformer), Category = Category.Utility)]
    [HelpURL("https://github.com/keenanwoodall/Deform/wiki/RecalculateBoundsDeformer")]
    public class RecalculateBoundsDeformer : Deformer
	{
		public override DataFlags DataFlags => DataFlags.Bounds;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			return MeshUtils.RecalculateBounds (data.DynamicNative, dependency);
		}
	}
}