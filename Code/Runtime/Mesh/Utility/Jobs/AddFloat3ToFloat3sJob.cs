using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace Deform
{
	public struct AddFloat3ToFloat3sJob : IJobParallelFor
	{
		public float3 value;
		public NativeArray<float3> values;

		public void Execute(int index)
		{
			values[index] += value;
		}
	}
}