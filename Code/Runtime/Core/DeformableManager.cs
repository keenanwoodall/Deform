using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace Deform
{
	/// <summary>
	/// Manages scheduling deformables.
	/// </summary>
	public class DeformableManager : MonoBehaviour
	{
		private static readonly string DEF_MANAGER_NAME = "DefaultDeformableManager";

		private static DeformableManager defaultInstance;
		/// <summary>
		/// Returns the default manager.
		/// </summary>
		/// <param name="createIfMissing">If true, a manager will be created if one doesn't exist.</param>
		/// <returns></returns>
		public static DeformableManager GetDefaultManager (bool createIfMissing)
		{
			if (defaultInstance == null)
				defaultInstance = new GameObject (DEF_MANAGER_NAME).AddComponent<DeformableManager> ();
			return defaultInstance;
		}

		/// <summary>
		/// Should the manager update?
		/// </summary>
		public bool update = true;

		private HashSet<IDeformable> deformables = new HashSet<IDeformable> ();

		private void Start ()
		{
			if (update)
			{
				ScheduleDeformables ();
				CompleteDeformables ();
				ScheduleDeformables ();
			}
		}

		private void Update ()
		{
			if (update)
			{
				CompleteDeformables ();
				ScheduleDeformables ();
			}
		}

		private void OnDisable ()
		{
			CompleteDeformables ();	
		}

		/// <summary>
		/// Creates a chain of work from the deformables and schedules it.
		/// </summary>
		public void ScheduleDeformables ()
		{
			foreach (var deformable in deformables)
				deformable.PreSchedule ();
			foreach (var deformable in deformables)
			{
				// Apply the finished work.
				deformable.ApplyData ();
				deformable.Schedule ();
			}

			// Schedule the chain.
			JobHandle.ScheduleBatchedJobs ();
		}

		/// <summary>
		/// Finishes the schedules work chain.
		/// </summary>
		public void CompleteDeformables ()
		{
			foreach (var deformable in deformables)
				deformable.Complete ();
		}

		/// <summary>
		/// Registers a deformable to be updated by this manager.
		/// </summary>
		public void AddDeformable (IDeformable deformable)
		{
			deformables.Add (deformable);
		}

		/// <summary>
		/// Unregisters a deformable from this manager.
		/// </summary>
		public void RemoveDeformable (IDeformable deformable)
		{
			deformables.Remove (deformable);
		}
	}
}