﻿using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Deform.Masking;

namespace Deform
{
	/// <summary>
	/// The implementation of IDeformable meant for deforming a MeshFilter or SkinnedMeshRenderer's mesh.
	/// </summary>
	[ExecuteAlways, DisallowMultipleComponent]
	public class Deformable : MonoBehaviour, IDeformable
	{
		public UpdateMode UpdateMode
		{
			get => updateMode;
			set
			{
				if (value == UpdateMode.Stop)
				{
					data.ResetData (DataFlags.All);
					ResetMesh ();
				}
				updateMode = value;
			}
		}
		public NormalsRecalculation NormalsRecalculation
		{
			get => normalsRecalculation;
			set => normalsRecalculation = value;
		}
		public BoundsRecalculation BoundsRecalculation
		{
			get => boundsRecalculation;
			set => boundsRecalculation = value;
		}
		public ColliderRecalculation ColliderRecalculation
		{
			get => colliderRecalculation;
			set => colliderRecalculation = value;
		}
		public MeshCollider MeshCollider
		{
			get => meshCollider;
			set => meshCollider = value;
		}
		public DeformableManager Manager
		{
			get => manager;
			set
			{
				if (manager != null)
					manager.RemoveDeformable (this);
				manager = value;
				if (manager != null)
					manager.AddDeformable (this);
			}
		}
		public List<DeformerElement> DeformerElements
		{
			get => deformerElements;
			set => deformerElements = value;
		}

		public DataFlags ModifiedDataFlags => lastModifiedDataFlags;

		[SerializeField, HideInInspector] private UpdateMode updateMode = UpdateMode.Auto;
		[SerializeField, HideInInspector] private NormalsRecalculation normalsRecalculation = NormalsRecalculation.Auto;
		[SerializeField, HideInInspector] private BoundsRecalculation boundsRecalculation = BoundsRecalculation.Auto;
		[SerializeField, HideInInspector] private ColliderRecalculation colliderRecalculation = ColliderRecalculation.None;
		[SerializeField, HideInInspector] private MeshCollider meshCollider;
		[SerializeField, HideInInspector] private DeformableManager manager;

		[SerializeField, HideInInspector] private MeshData data;
		[SerializeField, HideInInspector] private List<DeformerElement> deformerElements = new List<DeformerElement> ();

		private JobHandle handle;

		private DataFlags currentModifiedDataFlags = DataFlags.None;
		private DataFlags lastModifiedDataFlags = DataFlags.None;

		private void OnEnable ()
		{
			if (Application.isPlaying)
			{
				if (Manager == null)
					Manager = DeformableManager.GetDefaultManager (true);
				if (Manager != null)
					Manager.AddDeformable (this);
			}

			InitializeData ();

#if UNITY_EDITOR
			if (!Application.isPlaying && handle.IsCompleted)
			{
				Schedule ().Complete ();
				ApplyData ();
			}
#endif
		}

		private void OnDisable ()
		{
			Complete ();
			data.Dispose ();
			if (Manager != null)
				Manager.RemoveDeformable (this);
		}

		/// <summary>
		/// Initializes mesh data.
		/// </summary>
		public void InitializeData ()
		{
			// Don't create a new instance if one already exists because it'll will lose any serialized data from the previous instance.
			if (data == null)
				data = new MeshData ();
			data.Initialize (gameObject);
		}

		#if UNITY_EDITOR
		private void Update ()
		{
			if (!Application.isPlaying)
			{
				PreSchedule ();
				Schedule ().Complete ();
				ApplyData ();
				if (MeshCollider == null)
					MeshCollider = GetComponent<MeshCollider> ();
			}
		}
		#endif

		/// <summary>
		/// Called before Schedule.
		/// </summary>
		public void PreSchedule ()
		{
			foreach (var element in DeformerElements)
			{
				var deformer = element.Deformer;
				if (deformer != null && deformer.CanProcess ())
					deformer.PreProcess ();
			}
		}

		/// <summary>
		/// Creates a chain of work to deform the native mesh data.
		/// </summary>
		public JobHandle Schedule (JobHandle dependency = default)
		{
			if (data.Target.GetGameObject () == null)
				if (!data.Initialize (gameObject))
					return dependency;

			// Don't try to process any data if we're disabled or our data is broken.
			if (!CanUpdate ())
				return dependency;

			// We need to dispose of this objects data if it is destroyed.
			// We can't destroy the data if there's a job currently using it,
			// so we need to cache a reference to this objects part of the chain.
			// That will let us force this objects portion of the work to complete
			// which will let us dispose of its data and avoid a leak.
			handle = dependency;

			// Create a chain of job handles to process the data.
			for (int i = 0; i < deformerElements.Count; i++)
			{
				var element = deformerElements[i];
				var deformer = element.Deformer;

				// Only add the current deformer to the dependency chain if it wants to update.
				if (element.CanProcess ())
				{
					// If this deformer need updated bounds, add bounds recalculation
					// to the end of the chain.
					if (deformer.RequiresUpdatedBounds && BoundsRecalculation == BoundsRecalculation.Auto)
					{
						handle = MeshUtils.RecalculateBounds (data.DynamicNative, handle);
						currentModifiedDataFlags |= DataFlags.Bounds;
					}

					// Add the current deformer to the end of the dependency chain.
					handle = deformer.Process (data, handle);
					currentModifiedDataFlags |= deformer.DataFlags;
				}
			}

			if (normalsRecalculation == NormalsRecalculation.Auto)
			{
				// Add normal recalculation to the end of the deformation chain.
				handle = MeshUtils.RecalculateNormals (data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Normals;
			}
			if (boundsRecalculation != BoundsRecalculation.Never)
			{
				// Add bounds recalculation to the end as well.
				handle = MeshUtils.RecalculateBounds (data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Bounds;
			}

			// Return the new end of the dependency chain.
			return handle;
		}

		/// <summary>
		/// Copies the original mesh data to the mesh.
		/// </summary>
		public void ResetMesh ()
		{
			data.ApplyOriginalData ();
		}
		
		/// <summary>
		/// Completes any scheduled work.
		/// </summary>
		public void Complete ()
		{
			handle.Complete ();
		}

		/// <summary>
		/// Sends native mesh data to the mesh, updates the mesh collider if required and then resets the native mesh data.
		/// </summary>
		public void ApplyData ()
		{
			if (!CanUpdate ())
				return;

			data.ApplyData (currentModifiedDataFlags | lastModifiedDataFlags);

			if (ColliderRecalculation == ColliderRecalculation.Auto)
				RecalculateMeshCollider ();

			ResetDynamicData ();
		}

		/// <summary>
		/// Sends the original native mesh data to the dynamic mesh data.
		/// </summary>
		private void ResetDynamicData ()
		{
			data.ResetData (currentModifiedDataFlags);

			lastModifiedDataFlags = currentModifiedDataFlags;
			currentModifiedDataFlags = DataFlags.None;
		}

		/// <summary>
		/// Updates the mesh collider, if it exists.
		/// </summary>
		public void RecalculateMeshCollider ()
		{
			if (MeshCollider == null)
				MeshCollider = GetComponent<MeshCollider> ();
			if (MeshCollider != null)
				MeshCollider.sharedMesh = GetMesh ();
		}

		/// <summary>
		/// Returns true if this deformable can update.
		/// </summary>
		public bool CanUpdate ()
		{
			return handle.IsCompleted && (UpdateMode == UpdateMode.Auto || UpdateMode == UpdateMode.Custom) && isActiveAndEnabled && data.EnsureData ();
		}

		/// <summary>
		/// Returns the dynamic mesh.
		/// </summary>
		/// <returns></returns>
		public Mesh GetMesh ()
		{
			return data.DynamicMesh;
		}

		/// <summary>
		/// Returns the target's renderer.
		/// </summary>
		/// <returns></returns>
		public Renderer GetRenderer ()
		{
			return data.Target.GetRenderer ();
		}

		/// <summary>
		/// Returns true if the target exists.
		/// </summary>
		public bool HasTarget ()
		{
			return data.Target.Exists ();
		}

		public void ChangeMesh (Mesh mesh)
		{
			data.ChangeMesh (mesh);
		}
	}
}