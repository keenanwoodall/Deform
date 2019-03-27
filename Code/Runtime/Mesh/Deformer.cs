using UnityEngine;
using Unity.Jobs;

namespace Deform
{
	/// <summary>
	/// The base class for mesh deformation.
	/// </summary>
	public abstract class Deformer : MonoBehaviour, IDeformer<MeshData>
	{
		public const bool COMPILE_SYNCHRONOUSLY = true;
		public const int DEFAULT_BATCH_COUNT = 64;

		/// <summary>
		/// If false, this deformer will be ignored by Deformables.
		/// </summary>
		public bool update = true;
		/// <summary>
		/// If true, bounds will be recalculated before this deformer is sent data to process.
		/// </summary>
		public virtual bool RequiresUpdatedBounds { get; } = false;

		/// <summary>
		/// Override this to tell the Deformable what data you are changing so that it knows to copy it over to the mesh.
		/// </summary>
		public abstract DataFlags DataFlags { get; }

		/// <summary>
		/// Is called before Process is called on any deformers used by the same deformable.
		/// </summary>
		public virtual void PreProcess () { }
		/// <summary>
		/// Schedules a job to process the data which should be depend on dependency and returns its handle.
		/// </summary>
		/// <param name="data">The data to process.</param>
		/// <param name="dependency">The end of the dependency chain that this job should be appended to.</param>
		/// <returns>The new end of the dependency chain.</returns>
		public abstract JobHandle Process (MeshData data, JobHandle dependency = default);

		public virtual bool CanProcess ()
		{
			return update && isActiveAndEnabled;
		}
	}
}