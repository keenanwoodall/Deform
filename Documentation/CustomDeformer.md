## Creating a Custom Deformer

To show how to make your own deformer I'm going to walk you through how to create a simple "Offset" deformer, which will simply add an offset to a Deformable's vertices. Before creating your own deformers your should be slightly familiar with the Job System and Mathematics library.

**Step 1**
Create a new C# script named "OffsetDeformer".

**Step 2**
Open the script and delete everything.

**Step 3**
Make a class called `OffsetDeformer` that inherits from `Deformer` and put it in the `Deform` namespace.
```cs
namespace Deform
{
	public class OffsetDeformer : Deformer
	{
	}
}
```

**Step 4**
Implement the abstract members.
```cs
public override DataFlags DataFlags => throw new System.NotImplementedException ();

public override JobHandle Process (MeshData data, JobHandle dependency = default)
{
	throw new System.NotImplementedException ();
}
```

**Step 5**
The `Deformer.DataFlags` property is used to determine what data the deformer is going to change. This lets deformables figure out what data needs to be copied to the mesh.

Because we only want to change vertices, make `DataFlags` return `DataFlags.Vertices`. 
```cs
public override DataFlags DataFlags => DataFlags.Vertices;
```

However, if you are going to modify multiple types of data you can use the OR operator like so:
```cs
public override DataFlags DataFlags => DataFlags.Vertices | DataFlags.Normals;
```

**Step 6**
The `Process` method is where you will schedule your deformer's work. Deformable's call `Process` on each of their deformers and send their own `MeshData` as the argument for the `data` parameter. You can access native arrays that contain different elements of a mesh. Once a deformable's deformers are done processing, the deformable will copy any modified native mesh data to it's mesh.

In this case, we want to change the vertices, so we can create and schedule a job that changes the native vertices array. The deformable will know to apply changes we make to the array since we override the `DataFlags` enum value with `DataFlags.Vertices`.

Make sure to add `using` statements.

```cs
public override JobHandle Process (MeshData data, JobHandle dependency = default)
{
	return new OffsetJob
	{
		vertices = data.DynamicNative.VertexBuffer
	}.Schedule (data.Length, BatchCount, dependency);
}

...

private struct OffsetJob : IJobParallelFor
{
	public NativeArray<float3> vertices;

	public void Execute (int index)
	{
	}
}
```

Now you've technically got a deformer. It doesn't do anything, but if you add it to a Deformable and open the "Debug Info" foldout you'll see it thinks there are modified vertices. Let's do what it thinks we're doing and actually modify some vertices!

**Step 7**
Create a public `Vector3` field called `offset` in the `OffsetDeformer` class.
```cs
public Vector3 offset;
```
Then create a public `float3` field called `offset` in the `OffsetJob` struct;
```cs
public float3 offset;
```

**Step 8**
When creating the new job, set it's `offset` to the deformer's `offset`.
```cs
return new OffsetJob
{
	offset = offset,
	vertices = data.DynamicNative.VertexBuffer
}.Schedule (data.Length, BatchCount, dependency);
```

**Step 9**
Add the offset to the current index in the offset job.
```cs
public void Execute (int index)
{
	vertices[index] += offset;
}
```

Now it should work. If you add an Offset Deformer to a deformable and change the `offset` field it should offset the mesh.

![Example Deformer](https://i.imgur.com/ShOeUPI.gif)


**Step 10**
For a finishing touch, make sure to add the `[BurstCompile]` attribute to the `OffsetJob`. This will make your code be compiled by Burst (it'll run waaay faster).

If you want your deformer to be added to the Creator window, add a `[Deformer]` attribute to the class.

Here's the final script:
```cs
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Deform
{
	[Deformer (Name = "Offset", Type = typeof (OffsetDeformer))]
	public class OffsetDeformer : Deformer
	{
		public Vector3 offset;

		public override DataFlags DataFlags => DataFlags.Vertices;

		public override JobHandle Process (MeshData data, JobHandle dependency = default)
		{
			return new OffsetJob
			{
				offset = offset,
				vertices = data.DynamicNative.VertexBuffer
			}.Schedule (data.Length, BatchCount, dependency);
		}

		[BurstCompile]
		private struct OffsetJob : IJobParallelFor
		{
			public float3 offset;
			public NativeArray<float3> vertices;

			public void Execute (int index)
			{
				vertices[index] += offset;
			}
		}
	}
}
```
