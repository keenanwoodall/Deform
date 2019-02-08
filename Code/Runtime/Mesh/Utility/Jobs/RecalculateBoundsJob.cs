using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Beans.Unity.Mathematics;

namespace Deform
{
	/// <summary>
	/// Calculates the bounds of an array of float3s.
	/// </summary>
	[BurstCompile (CompileSynchronously = Deformer.COMPILE_SYNCHRONOUSLY)]
	public struct RecalculateBoundsJob : IJob
	{
		public NativeArray<bounds> bounds;
		public NativeArray<float3> vertices;

		public void Execute ()
		{
			var xmin = float.PositiveInfinity;
			var ymin = float.PositiveInfinity;
			var zmin = float.PositiveInfinity;
			var xmax = float.NegativeInfinity;
			var ymax = float.NegativeInfinity;
			var zmax = float.NegativeInfinity;

			var length = vertices.Length;
			for (int i = 0; i < length; i++)
			{
				var p = vertices[i];

				if (p.x < xmin)
					xmin = p.x;
				if (p.y < ymin)
					ymin = p.y;
				if (p.z < zmin)
					zmin = p.z;

				if (p.x > xmax)
					xmax = p.x;
				if (p.y > ymax)
					ymax = p.y;
				if (p.z > zmax)
					zmax = p.z;
			}

			var b = new bounds ();
			b.min = float3 (xmin, ymin, zmin);
			b.max = float3 (xmax, ymax, zmax);

			bounds[0] = b;
		}
	}
}