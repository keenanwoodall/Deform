using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

namespace Deform
{
	/// <summary>
	/// The implementation of IDeformable meant for deforming a MeshFilter or SkinnedMeshRenderer's mesh.
	/// </summary>
	[ExecuteAlways, DisallowMultipleComponent]
	[HelpURL("https://github.com/keenanwoodall/Deform/wiki/Deformable")]
	public class ElasticDeformable : Deformable
	{
		public float ElasticStrength
		{
			get => elasticStrength;
			set => elasticStrength = value;
		}
		public float ElasticDampening
		{
			get => elasticDampening;
			set => elasticDampening = value;
		}

		[SerializeField, HideInInspector] private float elasticStrength = 5f;
		[SerializeField, HideInInspector] private float elasticDampening = 0.9f;

		private NativeArray<float3> velocityBuffer;
		private NativeArray<float3> currentVertexBuffer;

		public override void InitializeData()
		{
			base.InitializeData();
			velocityBuffer = new NativeArray<float3>(data.Length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			currentVertexBuffer = new NativeArray<float3>(data.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			data.OriginalNative.VertexBuffer.CopyTo(currentVertexBuffer);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (velocityBuffer != null)
				velocityBuffer.Dispose();
			if (currentVertexBuffer != null)
				currentVertexBuffer.Dispose();
		}

		/// <summary>
		/// Creates a chain of work to deform the native mesh data.
		/// </summary>
		public override JobHandle Schedule(JobHandle dependency = default)
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

			if (Application.isPlaying)
			{
				handle = new ElasticVertexUpdateJob
				{
					strength = ElasticStrength,
					dampening = ElasticDampening,
					deltaTime = Time.deltaTime,
					localToWorld = transform.localToWorldMatrix,
					worldToLocal = transform.worldToLocalMatrix,
					velocities = velocityBuffer,
					currentVertices = currentVertexBuffer,
					targetVertices = data.DynamicNative.VertexBuffer
				}.Schedule(data.Length, Deformer.DEFAULT_BATCH_COUNT, handle);
			}

			if (NormalsRecalculation == NormalsRecalculation.Auto)
			{
				// Add normal recalculation to the end of the deformation chain.
				if (Application.isPlaying)
					handle = MeshUtils.RecalculateNormals(data.DynamicNative, handle);
				else
					handle = MeshUtils.RecalculateNormals(data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Normals;
			}
			if (BoundsRecalculation == BoundsRecalculation.Auto || BoundsRecalculation == BoundsRecalculation.OnceAtTheEnd)
			{
				// Add bounds recalculation to the end as well.
				if (Application.isPlaying)
					handle = MeshUtils.RecalculateBounds(data.DynamicNative, handle);
				else
					handle = MeshUtils.RecalculateBounds(data.DynamicNative, handle);
				currentModifiedDataFlags |= DataFlags.Bounds;
			}

			// Return the new end of the dependency chain.
			return handle;
		}

		/// <summary>
		/// Sends native mesh data to the mesh, updates the mesh collider if required and then resets the native mesh data.
		/// </summary>
		public override void ApplyData()
		{
			if (!CanUpdate())
				return;

			var flags = currentModifiedDataFlags | lastModifiedDataFlags;

			// If in play-mode, always apply vertices since it's an elastic effect
			if (Application.isPlaying)
				flags |= DataFlags.Vertices;

			var originalVertexBuffer = data.DynamicNative.VertexBuffer;
			if (Application.isPlaying)
			{
				// Swap out the final vertices with the ones moved by spring forces before applying...
				data.DynamicNative.VertexBuffer = currentVertexBuffer;
			}
			data.ApplyData(flags);
			// Then reassign the final vertices
			data.DynamicNative.VertexBuffer = originalVertexBuffer;

			if (BoundsRecalculation == BoundsRecalculation.Custom)
				data.DynamicMesh.bounds = CustomBounds;

			if (ColliderRecalculation == ColliderRecalculation.Auto)
				RecalculateMeshCollider();

			ResetDynamicData();
		}
	}
}