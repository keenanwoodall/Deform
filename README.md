![](https://img.shields.io/badge/unity-2018.3%2B-blue.svg)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/keenanwoodall/Deform/compare)

# Deform
A fully-featured deformer system for Unity.

## Features
- :zap: Lightning fast! (compiled with Burst and calculated using the Mathematics library)
- :trident: Fully multi-threadeded! (using the Job System)
- :globe_with_meridians: Works in worldspace!
- :gear: Custom editors and handles!

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
:warning: *Warning:* At the time of typing this, to update your version of Deform you'll have to remove and then re-add the dependency to the GitHub repository.
1. Open your project's manifest file in a text editor (Located at `<ProjectName>/Packages/manifest.json`)
2. Add `"com.beans.deform": "https://github.com/keenanwoodall/deform.git"` to the dependencies.
3. The project should be now installed.

### Install Manually
:warning: *Warning:* It's very important you follow these steps in the correct order. If you install Deform before installing it's dependencies, you'll have to assign all of it's assembly definition references manually.

1. Open Package Manager.
2. Install `Burst` and `Mathematics`.
3. Clone/download the repository directly into a project's `Assets` folder.

**If you have any errors unclearable errors, try restarting Unity. If the errors persist, open a new issue and I'll do my best to help.**

## License
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Thanks to [Thomas Ingram](https://twitter.com/vertexxyz) for going the extra-mile to help with editor scripting. The amount of knowledge he has of Unity is incredible!
