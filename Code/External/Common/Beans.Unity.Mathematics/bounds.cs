using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Beans.Unity.Mathematics
{
	public struct bounds
	{
		public float3 center;
		public float3 extents;

		public float3 size
		{
			get => extents * 2f;
			set => extents = value * 0.5f;
		}
		public float3 min
		{
			get => center - extents;
			set => setminmax (value, max);
		}
		public float3 max
		{
			get => center + extents;
			set => setminmax (min, value);
		}

		public bounds (float3 center, float3 size)
		{
			this.center = center;
			this.extents = size * 0.5f;
		}

		public void setminmax (float3 min, float3 max)
		{
			extents = (max - min) * 0.5f;
			center = min + extents;
		}

		public void encapsulate (float3 point)
		{
			setminmax (min (min, point), max (max, point));
		}
		public void encapsulate (bounds b)
		{
			encapsulate (b.center - b.extents);
			encapsulate (b.center + b.extents);
		}

		public void expand (float amount)
		{
			extents += float3 (amount * 0.5f);
		}
		public void expand (float3 amount)
		{
			extents += amount * 0.5f;
		}

		public bool intersects (bounds b)
		{
			return 
				(min.x <= b.max.x) && (max.x >= b.min.x) &&
				(min.y <= b.max.y) && (max.y >= b.min.y) &&
				(min.z <= b.max.z) && (max.z >= b.min.z);
		}

		/// <summary>
		/// Returns true if the point is within the bounds
		/// </summary>
		public bool contains (float3 p)
		{
			var insideX = center.x - extents.x < p.x && p.x < center.x + extents.x;
			var insideY = center.y - extents.y < p.y && p.y < center.y + extents.y;
			var insideZ = center.z - extents.z < p.z && p.z < center.z + extents.z;
			return insideX && insideY && insideZ;
		}

		/// <summary>
		/// Returns the closest point on the surface of the bounds to p.
		/// </summary>
		public float3 closestsurfacepoint (float3 p)
		{
			var cp = p;

			if (contains (p))
			{
				var ap = abs (p);
				if (ap.x > ap.y)
				{
					if (ap.x > ap.z)
						cp.x = (extents.x * sign (p.x)) + center.x;
					else
						cp.z = (extents.z * sign (p.z)) + center.z;
				}
				else if (ap.y > ap.z)
					cp.y = (extents.y * sign (p.y)) + center.y;
				else
					cp.z = (extents.z * sign (p.z)) + center.z;
			}
			else
			{
				var he = extents * 0.5f;

				if (p.x > extents.x + center.x)
					cp.x = extents.x + center.x;
				else if (p.x < -extents.x + center.x)
					cp.x = -extents.x + center.x;

				if (p.y > extents.y + center.y)
					cp.y = extents.y + center.y;
				else if (p.y < -extents.y + center.y)
					cp.y = -extents.y + center.y;

				if (p.z > extents.z + center.z)
					cp.z = extents.z + center.z;
				else if (p.z < -extents.z + center.z)
					cp.z = -extents.z + center.z;
			}

			return cp;
		}

		public override int GetHashCode ()
		{
			return center.GetHashCode () ^ (extents.GetHashCode () << 2);
		}

		public override bool Equals (object other)
		{
			if (!(other is bounds || other is Bounds))
				return false;

			return Equals ((bounds)other);
		}

		public bool Equals (Bounds other)
		{
			return center.Equals (other.center) && extents.Equals (other.extents);
		}

		public static bool operator == (bounds a, bounds b) => all (a.center == b.center) && all (a.extents == b.extents);
		public static bool operator != (bounds a, bounds b) => any (a.center != b.center) || any (a.extents != b.extents);

		public static implicit operator Bounds (bounds b) => new Bounds (b.center, b.size);
		public static implicit operator bounds (Bounds b) => new bounds (b.center, b.size);
	}
}