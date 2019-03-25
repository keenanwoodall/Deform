![](https://img.shields.io/badge/unity-2018.3%2B-blue.svg)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/keenanwoodall/Deform/compare)
![Discord](https://img.shields.io/discord/503808487520993280.svg?logo=Discord)
[![](https://img.shields.io/twitter/follow/keenanwoodall.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=keenanwoodall)

# Deform
A fully-featured deformer system for Unity. Deform is multi-threaded with the Job System, compiled with Burst and calculated using the Mathematics library.

## Features
- :zap: Lightning fast!
- :zap: Fully multi-threaded!
- :zap: 40+ modular deformers!
- :zap: Easily extendable!
- :zap: Works in worldspace!
- :zap: Custom editors and handles!

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
1. Open your project's manifest file in a text editor (Located at `<ProjectName>/Packages/manifest.json`)
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

## License
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments
* Thanks to [Thomas Ingram](https://twitter.com/vertexxyz) for going the extra-mile to help with editor scripting. The amount of knowledge he has of Unity is incredible!
* Thanks to [Alexander Ameye](https://twitter.com/alexanderameye) for sharing some of his snippets to make editor GUI look nicer.

## Author's Note
Thanks so much for checking out Deform! It's been my passion project since 2016 and has undergone 4 rewrites. For a long time I planned to charge money for this tool, but I've decided to release it for free for a few reasons.

1. I'm self-taught and still quite young. I'm proud of this, but it's made it very hard to "get a foot in the door". I don't have a college to help me get an internship or a degree to vouch for my skill so I need to show my programming prowess by making badass projects. I *could* sell Deform but I see that as a short-term solution. My long-term goal is to work for an awesome company making tools. Making my projects free and open-source is the best way to get my code in the hands of a possible employer.
2. I wouldn't have been able to make this tool if the environment I learned in wasn't so supportive. I want to give back to the game development community in some way. The amount of helpful people and free tools/art/learning resources swirling around the game dev world is uncontested. I want to contribute something of my own to further the spirit of sharing.
3. Now that it's open-source other people can contribute to the project! I think Deform is awesome right now, but there's always room for improvement and I'm excited to see what cool stuff people add!

**If you find this tool useful and you work at or know an awesome studio that is looking for an intern or junior developer, please reach out!**

[email](mailto:keenanwoodall@gmail.com) | [twitter](https://twitter.com/keenanwoodall) | [website](http://keenanwoodall.com)
