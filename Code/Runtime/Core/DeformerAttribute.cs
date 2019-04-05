using System;

namespace Deform
{
	public enum Category
	{
		Normal,
		Noise,
		Mask,
		Utility
	}

	/// <summary>
	/// Add this attribute to deformers to have them recognised by the deformer creation window.
	/// </summary>
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class DeformerAttribute : Attribute
	{
		public string Name;
		public string Description;
		public Category Category;
		public Type Type;
		public float XRotation;
		public float YRotation;
		public float ZRotation;
	}
}