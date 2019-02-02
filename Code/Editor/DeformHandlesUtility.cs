using UnityEngine;
using Deform;

namespace DeformEditor
{
	public static class DeformHandlesUtility
	{
		/// <summary>
		/// Returns the distance between two points along a primary transform axis.
		/// </summary>
		public static float DistanceAlongAxis (Transform transform, Vector3 a, Vector3 b, Axis axis)
		{
			var delta = (Quaternion.Inverse (transform.rotation) * (b - a));
			switch (axis)
			{
				default:
					return delta.x;
				case Axis.Y:
					return delta.y;
				case Axis.Z:
					return delta.z;
			}
		}
	}
}