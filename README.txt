DOCUMENTATION
----------------
More extensive documentation and written/video tutorials can be found on Deform's wiki: https://github.com/keenanwoodall/Deform/wiki


INSTALLATION
----------------
Deform has dependencies on the Burst and Mathematics packages. These packages are referenced by Deform via asmdef files. 
If you install Deform from the Asset Store before installing its dependencies it may lose references to the packages.

If you installed Deform before installing Burst and Mathematics install Burst and Mathematics from the Package Manager. 
The references to the packages should be fixed, but if they aren't go to the asmdef file under Deform/Code/Runtime and manually assign them.


GETTING STARTED
----------------
After installing, the first thing you'll probably want to do is deform a mesh. Every mesh that you want to deform must have a Deformable component added to it. 
This component is a little manager for the mesh. Once your mesh has a Deformable component you can add Deformer components to the Deformable's list of deformers to modify the mesh.

You can create these components like any other component (from the "Add Component" button in the Inspector), but an alternative way is to use the Creator window. 
This window lets you create deformable meshes and add deformers much more efficiently.

You can open the Creator window from either the Tools/Deform/Creator or Window/Deform/Creator menu item.

Remember, almost every deformer operates in worldspace. Most deformers have a Transform property called Axis. If left blank, the deformer will use its own transform as the axis. 
This axis is used as the position, rotation and scale of the deformer i.e. all deformation will be relative to it. For example, the Magnet deformer will push/pull vertices from its axis' position.


SUPPORT
----------------
If you run into any issues please submit them here https://github.com/keenanwoodall/Deform/issues/new
If you any questions feel free to email me at keenanwoodall@gmail.com

I'll do my best to respond and solve any issues but keep in mind that this is a free asset and I won't be able to dedicate all my time to supporting it.