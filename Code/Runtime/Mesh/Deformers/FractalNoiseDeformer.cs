using UnityEngine;
using Unity.Jobs;

namespace Deform
{
	[Deformer (Name = "Fractal Noise", Description = "Adds fractal noise to a mesh", Type = typeof (FractalNoiseDeformer))]
	public class FractalNoiseDeformer : NoiseDeformer
	{
		public int Octaves
		{
			get => octaves;
			set => octaves = Mathf.Max (1, value);
		}
		public float Lacunarity
		{
			get => lacunarity;
			set => lacunarity = value;
		}
		public float Persistance
		{
			get => persistance;
			set => persistance = value;
		}

		[SerializeField, HideInInspector] private int octaves = 2;
		[SerializeField, HideInInspector] private float lacunarity = 2f;
		[SerializeField, HideInInspector] private float persistance = 0.5f;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			if (MagnitudeScalar == 0f)
				return dependency;

			var scaledMagnitude = MagnitudeVector * MagnitudeScalar;
			var scaledFrequency = FrequencyVector * FrequencyScalar;
			var actualOffset = speedOffset + OffsetVector;

			var meshToAxis = DeformerUtils.GetMeshToAxisSpace (Axis, data.Target.GetTransform ());

			var handle = dependency;

			// I'm using if statements instead of a switch statement so that I can declare a different actualMagnitude and actualFrequency for each mode.
			if (Mode == NoiseMode.Derivative)
			{
				var actualMagnitude = scaledMagnitude;
				var actualFrequency = scaledFrequency;
				for (int i = 0; i < Octaves; i++)
				{
					handle = new DerivativeNoiseDeformJob
					{
						magnitude = actualMagnitude,
						frequency = actualFrequency,
						offset = actualOffset,
						meshToAxis = meshToAxis,
						vertices = data.DynamicNative.VertexBuffer
					}.Schedule (data.length, BatchCount, handle);

					actualMagnitude *= Persistance;
					actualFrequency *= Lacunarity;
				}
			}
			else if (Mode == NoiseMode.Directional)
			{
				var actualMagnitude = MagnitudeScalar;
				var actualFrequency = scaledFrequency;
				for (int i = 0; i < Octaves; i++)
				{
					handle = new DirectionalNoiseDeformJob
					{
						magnitude = actualMagnitude,
						frequency = actualFrequency,
						offset = actualOffset,
						axisSpace = meshToAxis,
						inverseAxisSpace = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer,
						normals = data.DynamicNative.NormalBuffer
					}.Schedule (data.length, BatchCount, handle);

					actualMagnitude *= Persistance;
					actualFrequency *= Lacunarity;
				}
			}
			else if (Mode == NoiseMode.Normal)
			{
				var actualMagnitude = MagnitudeScalar;
				var actualFrequency = scaledFrequency;
				for (int i = 0; i < Octaves; i++)
				{

					handle = new NormalNoiseDeformJob
					{
						magnitude = actualMagnitude,
						frequency = actualFrequency,
						offset = actualOffset,
						axisSpace = meshToAxis,
						vertices = data.DynamicNative.VertexBuffer,
						normals = data.DynamicNative.NormalBuffer
					}.Schedule (data.length, BatchCount, handle);

					actualMagnitude *= Persistance;
					actualFrequency *= Lacunarity;
				}
			}
			else if (Mode == NoiseMode.Spherical)
			{
				var actualMagnitude = MagnitudeScalar;
				var actualFrequency = scaledFrequency;
				for (int i = 0; i < Octaves; i++)
				{

					handle = new SphericalNoiseDeformJob
					{
						magnitude = actualMagnitude,
						frequency = actualFrequency,
						offset = actualOffset,
						axisSpace = meshToAxis,
						inverseAxisSpace = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer,
						normals = data.DynamicNative.NormalBuffer
					}.Schedule (data.length, BatchCount, handle);

					actualMagnitude *= Persistance;
					actualFrequency *= Lacunarity;
				}
			}
			else if (Mode == NoiseMode.Color)
			{
				var actualMagnitude = MagnitudeScalar;
				var actualFrequency = scaledFrequency;
				for (int i = 0; i < Octaves; i++)
				{
					handle = new ColorNoiseDeformJob
					{
						magnitude = actualMagnitude,
						frequency = actualFrequency,
						offset = actualOffset,
						axisSpace = meshToAxis,
						inverseAxisSpace = meshToAxis.inverse,
						vertices = data.DynamicNative.VertexBuffer,
						colors = data.DynamicNative.ColorBuffer
					}.Schedule (data.length, BatchCount, handle);

					actualMagnitude *= Persistance;
					actualFrequency *= Lacunarity;
				}
			}

			return handle;
		}
	}
}