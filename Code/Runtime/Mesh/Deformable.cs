using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Jobs;
using System;

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
						if (Application.isPlaying)
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

		public CullingMode CullingMode
		{
			get => cullingMode;
			set => cullingMode = value;
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

		public virtual StripMode StripMode
		{
			get
			{
				#if UNITY_EDITOR
				if (UnityEditor.GameObjectUtility.AreStaticEditorFlagsSet(gameObject, UnityEditor.StaticEditorFlags.BatchingStatic))
					return StripMode.Strip;
				#endif
				return stripMode;
			}
			set => stripMode = value;
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
		public virtual UpdateFrequency UpdateFrequency => UpdateFrequency.Default;

		public bool assignOriginalMeshOnDisable = true;
		public Action<MeshData> DynamicMeshUpdated;

		[SerializeField, HideInInspector] protected UpdateMode updateMode = UpdateMode.Auto;
		[SerializeField, HideInInspector] protected CullingMode cullingMode = CullingMode.DontUpdate;
		[SerializeField, HideInInspector] protected StripMode stripMode = StripMode.DontStrip;
		[SerializeField, HideInInspector] protected NormalsRecalculation normalsRecalculation = NormalsRecalculation.Auto;
		[SerializeField, HideInInspector] protected BoundsRecalculation boundsRecalculation = BoundsRecalculation.Auto;
		[SerializeField, HideInInspector] protected ColliderRecalculation colliderRecalculation = ColliderRecalculation.None;
		[SerializeField, HideInInspector] protected MeshCollider meshCollider;

		[SerializeField, HideInInspector] protected MeshData data;
		[SerializeField, HideInInspector] protected List<DeformerElement> deformerElements = new List<DeformerElement>();

		[SerializeField, HideInInspector] protected Bounds customBounds = new Bounds(Vector3.zero, Vector3.one * 0.5f);

		protected DeformableManager manager;
		protected JobHandle handle;

		protected DataFlags currentModifiedDataFlags = DataFlags.None;
		protected DataFlags lastModifiedDataFlags = DataFlags.None;

		protected virtual void OnEnable()
		{
			AllocateData();

			if (Application.isPlaying && UpdateMode == UpdateMode.Auto)
				Manager = DeformableManager.GetDefaultManager(true);
			
#if UNITY_EDITOR
			UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += OnSceneSaving;
#endif
			
			InitializeData();
		}

		protected virtual void OnDisable()
		{
			Complete();
			DisposeData();
			if (Manager != null)
				Manager.RemoveDeformable(this);
			
#if UNITY_EDITOR
			UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= OnSceneSaving;
#endif
		}
		
#if UNITY_EDITOR
		private void OnSceneSaving(Scene scene, string path)
		{
			data?.Target?.SetMesh(data.OriginalMesh);
		}
#endif

		private void OnBecameVisible()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				ForceImmediateUpdate();
				return;
			}
#endif
			
			// If the update mode is set to auto and the frequency is not immediate
			// the deformable needs to be updated immediately so there isn't a single
			// frame where the mesh is not deformed
			// Unfortunately, if the Deformable Manager is updating the deformable, we'll
			// mess stuff up by calling ForceImmediateUpdate();
			// The easiest way to force an immediate update in as DeformableManager-friendly way
			// is to just unregister and re-register this deformable with the manager
			if (UpdateMode == UpdateMode.Auto && UpdateFrequency != UpdateFrequency.Immediate)
			{
				// Store the current manager
				var m = manager;
				// If a manager is currently being used, re-register this
				if (m)
				{
					Manager = null;
					Manager = m;
				}
				// Otherwise register with the default manager
				else
					Manager = DeformableManager.GetDefaultManager(true);
			}
		}

		/// <summary>
		/// Initializes mesh data.
		/// </summary>
		public virtual void AllocateData()
		{
			// Don't create a new instance if one already exists because it'll will lose any serialized data from the previous instance.
			if (data == null)
				data = new MeshData();
			data.Initialize(gameObject);
		}
		
		public virtual void InitializeData()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying && handle.IsCompleted)
				ForceImmediateUpdate();
#endif
		}

		/// <summary>
		/// Disposed mesh data.
		/// </summary>
		public virtual void DisposeData()
		{
			data.Dispose(assignOriginalMeshOnDisable);
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
		protected bool IsVisible()
		{
			bool isVisible = data.Target.GetRenderer().isVisible;
			return isVisible;
		}

		protected bool ShouldCull(bool ignoreCullingMode)
		{
			return !IsVisible() && !ignoreCullingMode && cullingMode == CullingMode.DontUpdate;
		}

		/// <summary>
		/// Called before Schedule.
		/// </summary>
		public virtual void PreSchedule(bool ignoreCullingMode)
		{
			if (!CanUpdate() || ShouldCull(ignoreCullingMode))
				return;
			foreach (var element in DeformerElements)
			{
				var deformer = element.Component;
				if (deformer != null && deformer.CanProcess())
					deformer.PreProcess();
			}
		}
		public void PreSchedule() => PreSchedule(false);

		/// <summary>
		/// Creates a chain of work to deform the native mesh data.
		/// </summary>
		public virtual JobHandle Schedule(bool ignoreCullingMode, JobHandle dependency = default)
		{
			if (ShouldCull(ignoreCullingMode))
				return dependency;
			
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
			
			// If the mesh data has been modified, reset it so we don't deform it twice
			if (currentModifiedDataFlags != DataFlags.None)
				ResetDynamicData();

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

			// Store if the vertices have been modified. If not, we don't need to update normals/bounds
			var dirtyVertices = (currentModifiedDataFlags | DataFlags.Vertices) > 0;
			
			if (dirtyVertices && NormalsRecalculation == NormalsRecalculation.Auto)
			{
				// Add normal recalculation to the end of the deformation chain.
				handle = MeshUtils.RecalculateNormals(data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Normals;
			}
			if (dirtyVertices && BoundsRecalculation == BoundsRecalculation.Auto || BoundsRecalculation == BoundsRecalculation.OnceAtTheEnd)
			{
				// Add bounds recalculation to the end as well.
				handle = MeshUtils.RecalculateBounds(data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Bounds;
			}

			// Return the new end of the dependency chain.
			return handle;
		}

		public JobHandle Schedule(JobHandle dependency = default) => Schedule(false, dependency);
		
		/// <summary>
		/// Sends native mesh data to the mesh, updates the mesh collider if required and then resets the native mesh data.
		/// </summary>
		public virtual void ApplyData(bool ignoreCullingMode)
		{
			if (ShouldCull(ignoreCullingMode) || !CanUpdate())
				return;

			data.ApplyData(currentModifiedDataFlags | lastModifiedDataFlags);

			if (BoundsRecalculation == BoundsRecalculation.Custom)
				data.DynamicMesh.bounds = CustomBounds;

			if (ColliderRecalculation == ColliderRecalculation.Auto)
				RecalculateMeshCollider();

			DynamicMeshUpdated?.Invoke(data);

			ResetDynamicData();
		}

		public void ApplyData() => ApplyData(false);

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

		public void ForceImmediateUpdate()
		{
			Complete();
			PreSchedule(true);
			Schedule(true).Complete();
			ApplyData(true);
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
			{
				MeshCollider.sharedMesh = null;
				MeshCollider.sharedMesh = GetMesh();
			}
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

		public Mesh GetCurrentMesh()
		{
			return data.Target.GetMesh();
		}

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