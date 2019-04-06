## Installation
*Requires Unity 2018.3+*
### Install via UPM
1. Navigate to your project's Packages folder and open the manifest.json file.
2. Add this line below the `"dependencies": {` line
```
"com.beans.deform": "https://github.com/keenanwoodall/Deform.git",
```
3. UPM should now install Deform and it's dependencies.

Don't want to use git? Just download and unzip the repository into the Packages folder.

### Install Manually
*It's very important you follow these steps in the correct order. If you install Deform before installing it's dependencies, you'll have to assign all of it's assembly definition references manually.*
1. Open Package Manager.
2. Install `Burst` and `Mathematics`. The correct versions can be seen in [package.json](../../package.json)'s dependency list. You can probably use different versions, but I can't guarantee they'll work.
3. Clone/download the repository directly into a project's `Assets` folder.
