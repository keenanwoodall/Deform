![Unity Version](https://img.shields.io/badge/unity-2018.3%2B-blue.svg)
![GitHub](https://img.shields.io/github/license/keenanwoodall/Deform.svg)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/keenanwoodall/Deform/compare)

[![Twitter](https://img.shields.io/twitter/follow/keenanwoodall.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=keenanwoodall)
[![Discord](https://img.shields.io/discord/503808487520993280.svg?logo=Discord&style=social)](https://discord.gg/NnX5cpr)

# Deform
A fully-featured deformer system for Unity. Deform is multi-threaded with the Job System, compiled with Burst and calculations are done using the Mathematics library.

## Features
:zap: Lightning fast!

:zap: Fully multi-threaded!

:zap: 40+ modular deformers!

:zap: Easily extendable!

:zap: Works in worldspace!

:zap: Custom editors and handles!

## Installing
Deform has been built to be used with UPM (aka Package Manager.) You *can* clone it directly into your project, but it will take a bit of work to set it up.

### Install via UPM
#### From Disk
1. Clone/download the project somewhere on your computer (**not** in your Unity project.)
2. Open the project in which you want to install Deform.
3. Open the Package Manager window.
4. Click the "plus" :heavy_plus_sign: button at the bottom the the packages list and select `Add package from disk...`
5. Navigate to the folder where you installed Deform and open the `package.json` file.
6. The project should be now installed.

#### From Repo
*At the time of typing this, to update your version of Deform you'll have to remove and then re-add the dependency to the GitHub repository.*
1. Open your project's manifest file in a text editor (Located at `<ProjectPath>/Packages/manifest.json`)
2. Add `"com.beans.deform": "https://github.com/keenanwoodall/deform.git"` to the dependencies.
3. The project should be now installed.

### Install Manually
*It's very important you follow these steps in the correct order. If you install Deform before installing it's dependencies, you'll have to assign all of it's assembly definition references manually.*
1. Open Package Manager.
2. Install `Burst` and `Mathematics`.
3. Clone/download the repository directly into a project's `Assets` folder.

**If you have any errors unclearable errors try restarting Unity. If the errors persist open a new issue and I'll do my best to help.**

## Getting Started
After installing, the first thing you'll probably want to do is deform a mesh. Every mesh that you want to deform must have a `Deformable` component added to it. This component is like a little manager for the mesh. Once your mesh has a `Deformable` component you can add `Deformer` components to the `Deformable`'s list of deformers to modify the mesh.

You can create these components like any other component; from the "Add Component" button in the Inspector, but an alternative way is to use the *Creator* window. This window lets you create deformable meshes and add deformers much more efficiently.

You can open the *Creator* window from either the `Tools/Deform/Creator` or `Window/Deform/Creator` menu item.

Remember, almost every deformer operates in worldspace. Most deformers have a Transform property called `Axis`. If left blank, the deformer will use it's transform as the axis. This axis is used as the position, rotation and scale of the deformer i.e. all deformation will be relative to it. For example, the *Magnet* deformer will push/pull vertices from it's axis' position.

## Creating a custom Deformer

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
For a finishing touch, make sure to add `[BurstCompile]` to the `OffsetJob`. This will make you code be compiled by Burst (run waaay faster).

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

If you want your deformer to be added to the Creator window, add a `[Deformer(...)]` attribute to the class.

## Acknowledgments
* Thanks to [Thomas Ingram](https://twitter.com/vertexxyz) for going the extra-mile to help with editor scripting. The amount of knowledge he has of Unity is incredible!
* Thanks to [Alexander Ameye](https://twitter.com/alexanderameye) for sharing some of his snippets to make editor GUI look nicer.

## Author's Note
Thanks so much for checking out Deform! It's been my passion project since 2016 and has undergone 4 rewrites! For a long time I planned to charge money for this tool, but I've decided to release it for free for a few reasons.

1. I'm self-taught and still quite young. I'm take pride in this, but it's made it very hard to "get a foot in the door". I don't have a college to help me get an internship or a degree to vouch for my skill so I need to show my programming prowess by making badass projects. I *could* sell Deform but I see that as a short-term solution. My long-term goal is to make awesome tools for an awesome company. Making my projects free and open-source is the best way to get my code in the hands of a possible employer.
2. I wouldn't have been able to make this tool if the environment I learned in wasn't so supportive. I want to give back to the game development community in some way. The amount of helpful people and free tools/art/learning resources swirling around the game dev world is uncontested. I want to contribute something of my own to further the spirit of sharing.
3. Now that it's open-source other people can contribute to the project! I think Deform is awesome right now, but there's always room for improvement and I'm excited to see what cool stuff people add!

**If you find this tool useful and you work at, or know an awesome studio that is looking for an intern or junior developer, please reach out!**

[email](mailto:keenanwoodall@gmail.com) | [twitter](https://twitter.com/keenanwoodall) | [website](http://keenanwoodall.com)
