![Header](https://imgur.com/NpPsjQj.png)

[![Unity Version](https://img.shields.io/badge/unity-2018.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![GitHub](https://img.shields.io/github/license/keenanwoodall/Deform.svg)](https://github.com/keenanwoodall/Deform/blob/master/LICENSE.md)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/keenanwoodall/Deform/compare)

[![Twitter](https://img.shields.io/twitter/follow/keenanwoodall.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=keenanwoodall)
[![Discord](https://img.shields.io/discord/503808487520993280.svg?logo=Discord&style=social)](https://discord.gg/NnX5cpr)

A fully-featured deformer system for [Unity](https://unity3d.com/). Deform is multi-threaded with the [Job System](https://unity3d.com/learn/tutorials/topics/scripting/implementing-job-system), compiled with [Burst](https://unity3d.com/learn/tutorials/topics/scripting/using-burst-compiler) and calculations are done using the [Mathematics library](https://github.com/Unity-Technologies/Unity.Mathematics/blob/master/readme.md).

## Features
✔️ Lightning fast!

✔️ Fully multi-threaded!

✔️ 40+ deformers!

✔️ Meshes can be saved!

✔️ Easily extendable!

✔️ Works in worldspace!

✔️ Custom editors and handles!


## [Documentation](https://github.com/keenanwoodall/Deform/wiki)
* [Installation](https://github.com/keenanwoodall/Deform/wiki/Installation) (Important)
* [Getting Started](https://github.com/keenanwoodall/Deform/wiki/Getting-Started)
* [Creating a Custom Deformer](https://github.com/keenanwoodall/Deform/wiki/Creating-A-Custom-Deformer)

You can find all the deformers [here,](../Code/Runtime/Mesh/Deformers) and all their editors [here.](../Code/Editor/Mesh/Deformers)

## FAQ
> Does Deform work with the new prefab system?

Yes. Deform works seamlessly with nested prefabs and prefab variants.

> Do deformers have to be on the object they are deforming?

No. Because deformables require deformers be added manually, they can be anywhere in the scene and on any game object.

> Can deformables share deformers?

Yes. You can create a single deformer and add it to multiple deformables.

> How do deformables handle instancing?

Each deformable has it's own unique mesh. Duplicating and instantiating deformables shouldn't cause any issues.

## Limitations
Deform runs on the CPU. While it *is* incredibly fast, you should not expect to get performance comparable to vertex shaders. Because meshes are modified on the CPU each mesh has to be unique. From what I understand, this means each mesh will require a new draw call. Deform is not meant to be used at a massive scale. If you need to deform an entire world, tons of meshes, or an incredibly high poly model use vertex shaders. 

**tldr:** Use shaders if you need speed, use Deform if you need modularity and ease-of-use.

## Acknowledgments
* Thanks to [Thomas Ingram](https://twitter.com/vertexxyz) for going the extra-mile to help with editor scripting.
* Thanks to [Alexander Ameye](https://twitter.com/alexanderameye), [William Besnard](https://twitter.com/BillSansky), [Raphael Herdlicka](https://www.herdlicka.net/) and [David Carney](https://twitter.com/thedavidcarney) for testing Deform and giving helpful feedback.

## Author's Note
Thanks so much for checking out Deform! It's been my passion project since 2016 and has undergone **4 rewrites**! For a long time I planned to charge money for this tool, but I've decided to release it for free for a few reasons:

1. I'm self-taught and still quite young. I take pride in this, but it's made it hard to "get a foot in the door". I don't have a college to help me get an internship or a degree to vouch for my skill so I need to show my programming prowess by making badass projects. I *could* sell Deform but I see that as a short-term solution. My long-term goal is to make awesome tools for an awesome company. Making my projects free and open-source is the best way to get my code in the hands of a possible employer.
2. I wouldn't have been able to make this tool if the game-dev community wasn't so supportive. I want to give back to the community in some way.
3. Now that it's open-source other people can contribute to the project! I think Deform is awesome right now, but there's always room for improvement and I'm excited to see what cool stuff people want to add!

#### *If you work at, or know an awesome studio that is looking for a developer, please reach out!*

[email](mailto:keenanwoodall@gmail.com) | [twitter](https://twitter.com/keenanwoodall) | [website](http://keenanwoodall.com)

#### If you like Deform, please consider donating!

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://paypal.me/KeenanWoodall)
[![Donate](https://img.shields.io/badge/Donate-Kofi-green.svg)](https://ko-fi.com/keenanwoodall)
[![Itch](https://img.shields.io/badge/Buy-Itch.io-green.svg)](https://keenanwoodall.itch.io/deform)

---

## Example GIFs
<table>
  <tr>
    <td><img src="https://i.imgur.com/h3ZcNEC.gif"></td>
    <td><img src="https://i.imgur.com/LAzo6kT.gif"></td>
    <td><img src="https://i.imgur.com/CgxWUod.gif"></td>
  </tr>
  <tr>
    <td><img src="https://i.imgur.com/7BSjdJF.gif"></td>
    <td><img src="https://i.imgur.com/uFzvAlF.gif"></td>
    <td><img src="https://i.imgur.com/UoDy1ZC.gif"></td>
  </tr>
  <tr>
    <td><img src="https://i.imgur.com/h2D0KDV.gif"></td>
    <td><img src="https://i.imgur.com/Il2oGLH.gif"></td>
    <td><img src="https://i.imgur.com/PQrbXrY.gif"></td>
  </tr>
</table>
