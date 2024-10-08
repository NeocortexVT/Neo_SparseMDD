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
![A screenshot of the animation timeline in Blender with key frames set up for every frame.](/images/MDD_tutorial1.png)
![A screenshot of the shape keys in the data tab of an object in Blender. There is a shape key for each frame, named "frame_xxxx" where xxxx is a number between 0000 and 9999, here ranging from 71-98.](/images/MDD_tutorial2.png)

Next up, it is time to prune the shape keys. By doing this, the size of the model will remain manageable. See the image below for the example of the explosion animation. In this image two out of every three shape keys are removed from frames 19 to 34, four out of five shape keys are removed for frames 34 to 99, and nine out of ten shape keys are removed for frames 99 to 179. The assumption is that the frames between, for example, frames 19 and 22 can be approximated as a linear transition from frame 19 to frame 22. In the case of the explosion animation, we assume that there is more (non-linear) information in the early frames, where there is a lot of movement and many interactions with the collider, and less information in the later frames, where the movement of each polygon is much slower and no longer strongly influenced by the colliders, thus more linear. For the sake of convenience, it is advaced to keep the number of kept frames as consistent as possible for as many frames as possible (e.g. in the example below, 2/3 frames are removed for 15 frames, 4/5 frames are removed for 65 frames, and 9/10 frames are removed for 80 frames, rather than removing a random number for every step).
![An image of the same shape keys as the previous image, but shape keys have been removed as described above.](/images/MDD_tutorial3.png)

Then, set the "Relative to" value for each shape key to the previous remaining shape key in the animation (e.g. frame_0017 should be set relative to frame_0015, and frame_0019 should be set relative to frame_0017). This ensures that the shape key's transformation is set in relation to the one it is set relative to, and iterating over the "Value" value entails a smooth shift from the previous shape key to the selected one.
![An image of the same shape keys as the previous image, but zoomed in on the settings below. The shape key list goes from "frame_0013" to "frame_0019", with the even numbers having been removed. Shape key "frame_0015" is selected, and its "Relative to" value has been set to "frame_0013".](/images/MDD_tutorial4.png)

Finally, make sure that all the key shapes are listed in the correct order and uninterrupted by other shape keys. Shape keys should have completely unique names unless different animations on different meshes are part of the same final animation; in this case the remaining shape keys should match exactly in terms on names and shape key pruning. If all this is done, you can export the model as .fbx as you would normally.

### Configuring the MDD blend shapes in Unity
First, import the .fbx into Unity and convert it into a .vrm.

### Configuring the plugin and VNyan for use

### Setting up multiple animations on one/different meshes
