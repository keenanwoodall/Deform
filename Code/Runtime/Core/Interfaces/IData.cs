namespace Deform
{
	/// <summary>
	/// Holds the data that a Deformer will process.
	/// </summary>
	public interface IData
	{
		/// <summary>
		/// Resets data back to it's unmodified state.
		/// </summary>
		void ResetData (DataFlags dataFlags);
		/// <summary>
		/// Sends current data to target. (MeshFilter, SkinnedMeshRenderer etc)
		/// </summary>
		void ApplyData (DataFlags dataFlags);
		/// <summary>
		/// Deletes any data and frees its memory.
		/// </summary>
		void Dispose ();
	}
}