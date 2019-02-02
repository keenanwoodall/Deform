using UnityEngine;

namespace Deform
{
	/// <summary>
	/// Contains utility methods for calculations commonly done by deformers.
	/// </summary>
	public static class DeformerUtils
	{
		/// <summary>
		/// Returns a matrix that transform a mesh from local space to a space relative to the axis transform.
		/// </summary>
		public static Matrix4x4 GetMeshToAxisSpace (Transform axis, Transform mesh)
		{
			return axis.worldToLocalMatrix * mesh.transform.localToWorldMatrix;
		}

		/// <summary>
		/// Get an axis transform's position relative to a mesh transform.
		/// </summary>
		public static Vector3 GetAxisPositionRelativeToMesh (Transform axis, Transform mesh)
		{
			return mesh.worldToLocalMatrix.MultiplyPoint3x4 (axis.position);
		}
	}
}