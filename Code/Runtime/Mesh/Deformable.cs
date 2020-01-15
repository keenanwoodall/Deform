using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace Deform
{
	/// <summary>
	/// The implementation of IDeformable meant for deforming a MeshFilter or SkinnedMeshRenderer's mesh.
	/// </summary>
	[ExecuteAlways, DisallowMultipleComponent]
	[HelpURL("https://github.com/keenanwoodall/Deform/wiki/Deformable")]
	public class Deformable : MonoBehaviour, IDeformable
	{
		/// <summary>
		/// Assigning Auto sets Manager to the default manager.
		/// Assigning Stop resets data and mesh and prevents prescheduling, scheduling and applying.
		/// Assigning Custom sets Manager to null.
		/// </summary>
		public UpdateMode UpdateMode
		{
			get => updateMode;
			set
			{
				switch (value)
				{
					case UpdateMode.Auto:
						Manager = DeformableManager.GetDefaultManager(true);
						break;
					case UpdateMode.Stop:
						Complete();
						data.ResetData(DataFlags.All);
						ResetMesh();
						break;
					case UpdateMode.Custom:
						Manager = null;
						Complete();
						break;
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
		public List<DeformerElement> DeformerElements
		{
			get => deformerElements;
			set => deformerElements = value;
		}
		public Bounds CustomBounds
		{
			get => customBounds;
			set => customBounds = value;
		}

		/// <summary>
		/// Setting the Manager property unregisters this deformable from it's current manager and registers it with the new one.
		/// Assign null if you want to unregister.
		/// </summary>
		public DeformableManager Manager
		{
			get => manager;
			set
			{
				if (manager != null)
					manager.RemoveDeformable(this);
				manager = value;
				if (manager != null)
					manager.AddDeformable(this);
			}
		}

		public DataFlags ModifiedDataFlags => lastModifiedDataFlags;

		public bool assignOriginalMeshOnDisable = true;

		[SerializeField, HideInInspector] protected UpdateMode updateMode = UpdateMode.Auto;
		[SerializeField, HideInInspector] protected NormalsRecalculation normalsRecalculation = NormalsRecalculation.Auto;
		[SerializeField, HideInInspector] protected BoundsRecalculation boundsRecalculation = BoundsRecalculation.Auto;
		[SerializeField, HideInInspector] protected ColliderRecalculation colliderRecalculation = ColliderRecalculation.None;
		[SerializeField, HideInInspector] protected MeshCollider meshCollider;

		[SerializeField, HideInInspector] protected MeshData data;
		[SerializeField, HideInInspector] protected List<DeformerElement> deformerElements = new List<DeformerElement>();

		[SerializeField, HideInInspector] protected Bounds customBounds;

		protected DeformableManager manager;
		protected JobHandle handle;

		protected DataFlags currentModifiedDataFlags = DataFlags.None;
		protected DataFlags lastModifiedDataFlags = DataFlags.None;

		private void OnEnable()
		{
			InitializeData();

			if (Application.isPlaying)
				Manager = DeformableManager.GetDefaultManager(true);

#if UNITY_EDITOR
			if (!Application.isPlaying && handle.IsCompleted)
			{
				PreSchedule();
				Schedule().Complete();
				ApplyData();
			}
#endif
		}

		protected virtual void OnDisable()
		{
			Complete();
			data.Dispose(assignOriginalMeshOnDisable);
			if (Manager != null)
				Manager.RemoveDeformable(this);
		}

		/// <summary>
		/// Initializes mesh data.
		/// </summary>
		public virtual void InitializeData()
		{
			// Don't create a new instance if one already exists because it'll will lose any serialized data from the previous instance.
			if (data == null)
				data = new MeshData();
			data.Initialize(gameObject);
		}

#if UNITY_EDITOR
		protected void Update()
		{
			if (!Application.isPlaying)
			{
				PreSchedule();
				Schedule().Complete();
				ApplyData();
			}
		}
#endif

		/// <summary>
		/// Called before Schedule.
		/// </summary>
		public void PreSchedule()
		{
			if (!CanUpdate())
				return;
			foreach (var element in DeformerElements)
			{
				var deformer = element.Component;
				if (deformer != null && deformer.CanProcess())
					deformer.PreProcess();
			}
		}

		/// <summary>
		/// Creates a chain of work to deform the native mesh data.
		/// </summary>
		public virtual JobHandle Schedule(JobHandle dependency = default)
		{
			if (data.Target.GetGameObject() == null)
				if (!data.Initialize(gameObject))
					return dependency;

			// Don't try to process any data if we're disabled or our data is broken.
			if (!CanUpdate())
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
				var deformer = element.Component;

				// Only add the current deformer to the dependency chain if it wants to update.
				if (element.CanProcess())
				{
					// If this deformer need updated bounds, add bounds recalculation
					// to the end of the chain.
					if (deformer.RequiresUpdatedBounds && BoundsRecalculation == BoundsRecalculation.Auto)
					{
						handle = MeshUtils.RecalculateBounds(data.DynamicNative, handle);
						currentModifiedDataFlags |= DataFlags.Bounds;
					}

					// Add the current deformer to the end of the dependency chain.
					handle = deformer.Process(data, handle);
					currentModifiedDataFlags |= deformer.DataFlags;
				}
			}

			if (NormalsRecalculation == NormalsRecalculation.Auto)
			{
				// Add normal recalculation to the end of the deformation chain.
				handle = MeshUtils.RecalculateNormals(data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Normals;
			}
			if (BoundsRecalculation == BoundsRecalculation.Auto || BoundsRecalculation == BoundsRecalculation.OnceAtTheEnd)
			{
				// Add bounds recalculation to the end as well.
				handle = MeshUtils.RecalculateBounds(data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Bounds;
			}

			// Return the new end of the dependency chain.
			return handle;
		}

		/// <summary>
		/// Copies the original mesh data to the mesh.
		/// </summary>
		public void ResetMesh()
		{
			data.ApplyOriginalData();
		}

		/// <summary>
		/// Completes any scheduled work.
		/// </summary>
		public void Complete()
		{
			handle.Complete();
		}

		/// <summary>
		/// Sends native mesh data to the mesh, updates the mesh collider if required and then resets the native mesh data.
		/// </summary>
		public virtual void ApplyData()
		{
			if (!CanUpdate())
				return;

			data.ApplyData(currentModifiedDataFlags | lastModifiedDataFlags);

			if (BoundsRecalculation == BoundsRecalculation.Custom)
				data.DynamicMesh.bounds = CustomBounds;

			if (ColliderRecalculation == ColliderRecalculation.Auto)
				RecalculateMeshCollider();

			ResetDynamicData();
		}

		public void ForceImmediateUpdate()
		{
			PreSchedule();
			Schedule().Complete();
			ApplyData();
		}

		/// <summary>
		/// Sends the original native mesh data to the dynamic mesh data.
		/// </summary>
		protected void ResetDynamicData()
		{
			data.ResetData(currentModifiedDataFlags);

			lastModifiedDataFlags = currentModifiedDataFlags;
			currentModifiedDataFlags = DataFlags.None;
		}

		/// <summary>
		/// Updates the mesh collider, if it exists.
		/// </summary>
		public void RecalculateMeshCollider()
		{
			if (MeshCollider != null)
				MeshCollider.sharedMesh = GetMesh();
		}

		/// <summary>
		/// Returns true if this deformable can update.
		/// </summary>
		public bool CanUpdate()
		{
			return handle.IsCompleted && (UpdateMode == UpdateMode.Auto || UpdateMode == UpdateMode.Custom) && isActiveAndEnabled && data.EnsureData();
		}

		public void AddDeformer(Deformer deformer, bool active = true)
		{
			DeformerElements.Add(new DeformerElement(deformer, active));
		}

		public void RemoveDeformer(Deformer deformer)
		{
			for (int i = 0; i < DeformerElements.Count; i++)
			{
				var element = DeformerElements[i];
				if (element.Component == deformer)
				{
					DeformerElements.RemoveAt(i);
					i--;
				}
			}
		}

		public void ChangeMesh(Mesh mesh)
		{
			Complete();
			data.ChangeMesh(mesh);
		}

		/// <summary>
		/// Returns the dynamic mesh.
		/// </summary>
		public Mesh GetMesh()
		{
			return data.DynamicMesh;
		}

		/// <summary>
		/// Returns the original mesh.
		/// </summary>
		public Mesh GetOriginalMesh()
		{
			return data.OriginalMesh;
		}

		public ManagedMeshData GetOriginalManagedMeshData() => data.GetOriginalManagedData();
		public ManagedMeshData GetDynamicManagedMeshData() => data.GetDynamicManagedData();

		/// <summary>
		/// Returns the target's renderer.
		/// </summary>
		/// <returns></returns>
		public Renderer GetRenderer()
		{
			return data.Target.GetRenderer();
		}

		/// <summary>
		/// Returns true if the target exists.
		/// </summary>
		public bool HasTarget()
		{
			return data.Target.Exists();
		}
	}
}