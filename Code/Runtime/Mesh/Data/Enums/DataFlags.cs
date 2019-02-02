using System;

namespace Deform
{
	[Flags]
	public enum DataFlags
	{
		None			= 0,
		Vertices		= 1 << 0,
		Normals			= 1 << 1,
		Tangents		= 1 << 2,
		UVs				= 1 << 3,
		Colors			= 1 << 4,
		Triangles		= 1 << 5,
		MaskVertices	= 1 << 6,
		Bounds			= 1 << 7,
		All =			0xFF
	}
}