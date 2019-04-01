## Installation
### Install via UPM
1. Navigate to your project's Packages folder using a console or terminal.
```
cd <ProjectPath>/Packages
```
2. Clone the repository
```
git clone https://github.com/keenanwoodall/Deform
```
---
Those of you not using git can just download and unzip the repository into the Packages folder.
```
- <ProjectPath>
  - Assets
  - Packages
    - <extract files here>
```

### Install Manually
*It's very important you follow these steps in the correct order. If you install Deform before installing it's dependencies, you'll have to assign all of it's assembly definition references manually.*
1. Open Package Manager.
2. Install `Burst` and `Mathematics`. The correct versions can be seen in [package.json](../package.json)'s dependency list. You can probably use different versions, but I can't guarantee they'll work.
3. Clone/download the repository directly into a project's `Assets` folder.

**If you have any errors unclearable errors try restarting Unity. If the errors persist open a new issue and I'll do my best to help.**
