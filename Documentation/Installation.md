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
