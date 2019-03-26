using UnityEngine;
using Unity.Jobs;

namespace Deform
{
	public abstract class NoiseDeformer : Deformer, IFactor
	{
		public float Factor
		{
			get => MagnitudeScalar;
			set => MagnitudeScalar = value;
		}

		public NoiseMode Mode
		{
			get => mode;
			set => mode = value;
		}
		public float MagnitudeScalar
		{
			get => magnitudeScalar;
			set => magnitudeScalar = value;
		}
		public Vector3 MagnitudeVector
		{
			get => magnitudeVector;
			set => magnitudeVector = value;
		}
		public float FrequencyScalar
		{
			get => frequencyScalar;
			set => frequencyScalar = value;
		}
		public Vector3 FrequencyVector
		{
			get => frequencyVector;
			set => frequencyVector = value;
		}
		public Vector4 OffsetVector
		{
			get => offsetVector;
			set => offsetVector = value;
		}
		public float OffsetSpeedScalar
		{
			get => offsetSpeedScalar;
			set => offsetSpeedScalar = value;
		}
		public Vector4 OffsetSpeedVector
		{
			get => offsetSpeedVector;
			set => offsetSpeedVector = value;
		}
		public Transform Axis
		{
			get
			{
				if (axis == null)
					axis = transform;
				return axis;
			}
			set => axis = value;
		}

		[SerializeField, HideInInspector] private NoiseMode mode = NoiseMode._3D;
		[SerializeField, HideInInspector] private float magnitudeScalar = 0f;
		[SerializeField, HideInInspector] private Vector3 magnitudeVector = Vector3.one;
		[SerializeField, HideInInspector] private float frequencyScalar = 2f;
		[SerializeField, HideInInspector] private Vector3 frequencyVector = Vector3.one;
		[SerializeField, HideInInspector] private Vector4 offsetVector;
		[SerializeField, HideInInspector] private float offsetSpeedScalar = 1f;
		[SerializeField, HideInInspector] private Vector4 offsetSpeedVector = new Vector4 (0f, 0f, 0f);
		[SerializeField, HideInInspector] private Transform axis;

		protected Vector4 speedOffset;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public Vector3 GetActualMagnitude () => MagnitudeVector * MagnitudeScalar;
		public Vector3 GetActualFrequency () => FrequencyVector * FrequencyScalar;
		public Vector4 GetActualOffset () => speedOffset + OffsetVector;

		protected virtual void Update ()
		{
			speedOffset += OffsetSpeedVector * (OffsetSpeedScalar * Time.deltaTime);
		}

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			if (GetActualMagnitude () == Vector3.zero)
				return dependency;

			switch (Mode)
			{
				default:
					return Create3DNoiseJob (data, dependency);
				case NoiseMode.Directional:
					return CreateDirectionalNoiseJob (data, dependency);
				case NoiseMode.Normal:
					return CreateNormalNoiseJob (data, dependency);
				case NoiseMode.Spherical:
					return CreateSphericalNoiseJob (data, dependency);
				case NoiseMode.Color:
					return CreateColorNoiseJob (data, dependency);
			}
		}

		protected abstract JobHandle Create3DNoiseJob (MeshData data, JobHandle dependency = default);
		protected abstract JobHandle CreateDirectionalNoiseJob (MeshData data, JobHandle dependency = default);
		protected abstract JobHandle CreateNormalNoiseJob (MeshData data, JobHandle dependency = default);
		protected abstract JobHandle CreateSphericalNoiseJob (MeshData data, JobHandle dependency = default);
		protected abstract JobHandle CreateColorNoiseJob (MeshData data, JobHandle dependency = default);
	}
}