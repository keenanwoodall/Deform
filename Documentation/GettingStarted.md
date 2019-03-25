### Video
[![](https://i.imgur.com/W6nZmkK.png)](https://youtu.be/8HnPkK_cZG8)
### Text
After installing, the first thing you'll probably want to do is deform a mesh. Every mesh that you want to deform must have a `Deformable` component added to it. This component is like a little manager for the mesh. Once your mesh has a `Deformable` component you can add `Deformer` components to the `Deformable`'s list of deformers to modify the mesh.

You can create these components like any other component; from the "Add Component" button in the Inspector, but an alternative way is to use the *Creator* window. This window lets you create deformable meshes and add deformers much more efficiently.

You can open the *Creator* window from either the `Tools/Deform/Creator` or `Window/Deform/Creator` menu item.

Remember, almost every deformer operates in worldspace. Most deformers have a Transform property called `Axis`. If left blank, the deformer will use it's transform as the axis. This axis is used as the position, rotation and scale of the deformer i.e. all deformation will be relative to it. For example, the *Magnet* deformer will push/pull vertices from it's axis' position.
