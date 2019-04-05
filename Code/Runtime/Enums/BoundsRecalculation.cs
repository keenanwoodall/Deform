namespace Deform
{
	/// <summary>
	/// Auto: Bounds are recalculated if a deformer requires it, and at the very end.
	/// Never: Bounds are never recalculated.
	/// OnceAtTheEnd: Deformers requests for updated bounds are ignored and the bounds are only updated once all deformers finish.
	/// </summary>
	public enum BoundsRecalculation
	{
		Auto,
		Never,
		OnceAtTheEnd,
		Custom
	}
}