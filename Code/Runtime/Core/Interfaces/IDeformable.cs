using Unity.Jobs;

namespace Deform
{
	/// <summary>
	/// Manages deformers and the data they process.
	/// </summary>
	public interface IDeformable
	{
		/// <summary>
		/// Called before all other Deformables with the same DeformableManager have Schedule called.
		/// </summary>
		void PreSchedule ();

		/// <summary>
		/// Creates a job chain linking each deformer.
		/// </summary>
		JobHandle Schedule (JobHandle dependency = default);

		/// <summary>
		/// Force the scheduled work to finish.
		/// </summary>
		void Complete ();

		/// <summary>
		/// Applies current data to target.
		/// </summary>
		void ApplyData ();

		/// <summary>
		/// Returns true if this deformable should be processed.
		/// </summary>
		bool CanUpdate ();
	}
}