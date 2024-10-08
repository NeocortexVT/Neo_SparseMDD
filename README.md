# Neo_SparseMDD
Code for the Sparse MDD animation plugin for VNyan

## About the Sparse MDD plugin
MDD animations are a useful tool to animate 3D objects without relying on armatures. When an animation on a 3D object is converted to the MDD format, a blend shape is gererated for each frame of the animation, and to recreate the animation, these blend shapes are cycled through in order. This way, physics-based animations in Blender can be exported into Unity, for example.

The problem with MDD animations is that for each frame, a blend shape is stored, and thus vector data has to be stored for each frame. A five second animation at 60fps requires three hundred blend shapes. On a mesh with 70k polygons (generally the upper limit for VRChat if these polys are tris), the resulting FBX can easily exceed 1GB in size. Converting a model of such size to the .VRM format quickly becomes unworkable, as memory requirements exceed limits not uncommon in consumer-level systems. Subsequently, such animations provide a challenge when tried to be implemented on VTuber models.

The Sparse MDD plugin is a plugin for VNyan that works around this limitation by pruning the number of blend shapes, and iterating over the remaining blend shapes in such a way as to approximate the original animation. The underlying assumption is that moving from one blend shape to the next is roughly linear, and going between blend shapes in a manner proportional to the number of blend shapes (i.e. frames) that were pruned gives a desirable result. The plugin is configured in such a way that the density of the pruning does not have to be homogenous across the full animation, so non-linear effects can be maintained. 
<!--Insert example of explosion with indication of non-linear relation between frames at start and linear at the end-->

## Adding the plugin to VNyan

## How to set up your model and MDD blend shapes
### Modifying the blend shapes for Sparse MDD in Blender
For a tutorial on how to convert animations to MDD, see [this video](https://www.youtube.com/watch?v=sdl-jpZ0NR0&).
Once you have converted your animation to mdd and reimported it, your blender should look something like this:
![](/images/MDD_tutorial1.png)

### Configuring the MDD blend shapes in Unity

### Configuring the plugin and VNyan for use

### Setting up multiple animations on one/different meshes
