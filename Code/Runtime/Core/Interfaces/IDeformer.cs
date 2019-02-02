using Unity.Jobs;

namespace Deform
{
	/// <summary>
	/// Processes data.
	/// </summary>
	/// <typeparam name="TData">The type of data that is processed.</typeparam>
	public interface IDeformer<TData> where TData : IData
	{
		/// <summary>
		/// Called before Process.
		/// </summary>
		void PreProcess ();

		/// <summary>
		/// Returns a scheduled job that operated on the data.
		/// </summary>
		/// <param name="data">The data to be operated on in the job.</param>
		/// <returns>The scheduled job.</returns>
		JobHandle Process (TData data, JobHandle dependency = default (JobHandle));

		/// <summary>
		/// Returns true if this Deformer should process data.
		/// </summary>
		bool CanProcess ();
	}
}