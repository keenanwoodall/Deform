namespace Deform
{
	/// <summary>
	/// Contains a reference to a deformer, and a bool, Active, that determines if the deformer should be allowed to process data.
	/// </summary>
	[System.Serializable]
	public struct DeformerElement
	{
		public Deformer Deformer;
		public bool Active;

		public DeformerElement (Deformer deformer, bool active = true)
		{
			Deformer = deformer;
			Active = active;
		}

		public bool CanProcess ()
		{
			return Active && Deformer != null && Deformer.CanProcess ();
		}
	}
}