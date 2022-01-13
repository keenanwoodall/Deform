namespace Deform
{
	/// <summary>
	/// None: Normals aren't touched.
	/// Fast: Split vertices will have hard edges. For smooth edges, adjacent triangles must share verts.
	/// Quality: Calculate normals with smoothing angle.
	/// </summary>
	public enum NormalsRecalculation
	{
		Fast,
		None,
		Quality
	}
}