using UnityEngine;

namespace Deform
{
	/// <summary>
	/// Stores managed mesh data. This the middle-man between NativeMeshData and the actual mesh.
	/// </summary>
	[System.Serializable]
	public class ManagedMeshData
	{
		public Vector3[] Vertices;
		public Vector3[] Normals;
		public Vector4[] Tangents;
		public Vector2[] UVs;
		public Color[] Colors;
		public int[] Triangles;
		public Bounds Bounds;

		public ManagedMeshData ()
		{
			Vertices = new Vector3[0];
			Normals = new Vector3[0];
			Tangents = new Vector4[0];
			UVs = new Vector2[0];
			Colors = new Color[0];
			Triangles = new int[0];
		}

		public ManagedMeshData (Mesh mesh)
		{
			Vertices = mesh.vertices;
			Normals = mesh.normals;
			Tangents = mesh.tangents;
			UVs = mesh.uv;
			Colors = mesh.colors;
			Triangles = mesh.triangles;
			Bounds = mesh.bounds;

			if (Normals == null || Normals.Length != mesh.vertexCount)
				Normals = new Vector3[mesh.vertexCount];
			if (Tangents == null || Tangents.Length != mesh.vertexCount)
				Tangents = new Vector4[mesh.vertexCount];
			if (UVs == null || UVs.Length != mesh.vertexCount)
				UVs = new Vector2[mesh.vertexCount];
			if (Colors == null || Colors.Length != mesh.vertexCount)
				Colors = new Color[mesh.vertexCount];
		}

		/// <summary>
		/// Returns true if all the data arrays aren't null.
		/// </summary>
		public bool HasValidData ()
		{
			return
				Vertices != null && Normals != null && Tangents != null && UVs != null && Colors != null && Triangles != null &&
				Vertices.Length == Normals.Length && Normals.Length == Tangents.Length && Tangents.Length == UVs.Length && UVs.Length == Colors.Length;
		}
	}
}