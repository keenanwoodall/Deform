using Unity.Jobs;

namespace Deform
{
	/// <summary>
	/// Contains utility methods for common mesh job operations.
	/// </summary>
	public static class MeshUtils
	{
		/// <summary>
		/// Returns a job chain that will recalculate the normals of the native data.
		/// </summary>
		public static JobHandle RecalculateNormals (NativeMeshData data, JobHandle dependency = default)
		{
			var length = data.NormalBuffer.Length;

			dependency = new ResetNormalsJob
			{
				normals = data.NormalBuffer
			}.Schedule (length, 256, dependency);
			dependency = new AddTriangleNormalToNormalsJob
			{
				triangles = data.IndexBuffer,
				vertices = data.VertexBuffer,
				normals = data.NormalBuffer
			}.Schedule (dependency);
			dependency = new NormalizeNormalsJob
			{
				normals = data.NormalBuffer
			}.Schedule (length, 256, dependency);

			return dependency;
		}

		/// <summary>
		/// Returns a job chain that will recalculate the bounds of the native data.
		/// </summary>
		public static JobHandle RecalculateBounds (NativeMeshData data, JobHandle dependency = default)
		{
			return new RecalculateBoundsJob
			{
				bounds = data.Bounds,
				vertices = data.VertexBuffer
			}.Schedule (dependency);
		}
	}
}