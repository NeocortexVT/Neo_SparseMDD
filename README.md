# Neo_SparseMDD
Code for the Sparse MDD animation plugin for VNyan

## About the Sparse MDD plugin
MDD animations are a useful tool to animate 3D objects without relying on armatures. When an animation on a 3D object is converted to the MDD format, a blend shape is gererated for each frame of the animation, and to recreate the animation, these blend shapes are cycled through in order. This way, physics-based animations in Blender can be exported into Unity, for example.

The problem with MDD animations is that for each frame, a blend shape is stored, and thus vector data has to be stored for each frame. A five second animation at 60fps requires three hundred blend shapes. On a mesh with 70k polygons (generally the upper limit for VRChat if these polys are tris), the resulting FBX can easily exceed 1GB in size. Converting a model of such size to the .VRM format quickly becomes unworkable, as memory requirements exceed limits not uncommon in consumer-level systems. Subsequently, such animations provide a challenge when tried to be implemented on VTuber models.

The Sparse MDD plugin is a plugin for VNyan that works around this limitation by pruning the number of blend shapes, and iterating over the remaining blend shapes in such a way as to approximate the original animation. The underlying assumption is that moving from one blend shape to the next is roughly linear, and going between blend shapes in a manner proportional to the number of blend shapes (i.e. frames) that were pruned gives a desirable result. The plugin is configured in such a way that the density of the pruning does not have to be homogenous across the full animation, so non-linear effects can be maintained. 
<!--Insert example of explosion with indication of non-linear relation between frames at start and linear at the end-->

## Adding the plugin to VNyan
<!--Insert link to Itch and instructions to add the contents to the Asssets folder-->

## How to set up your model and MDD blend shapes
### Modifying the blend shapes for Sparse MDD in Blender
For a tutorial on how to convert animations to MDD, see [this video](https://www.youtube.com/watch?v=sdl-jpZ0NR0&). Note that this plugin relies on setting up blend shapes for the object you wish to animate. If the kind of object you are trying to create does not support blend shapes, this plugin is unlikely to work for you. 

Once you have converted your animation to mdd and reimported it, your blender should look something like this:

![A screenshot of the animation timeline in Blender with key frames set up for every frame.](/images/MDD_tutorial1.png)
![A screenshot of the shape keys in the data tab of an object in Blender. There is a shape key for each frame, named "frame_xxxx" where xxxx is a number between 0000 and 9999, here ranging from 71-98.](/images/MDD_tutorial2.png)

Next up, it is time to prune the shape keys. By doing this, the size of the model will remain manageable if you either have a long animation or a high-poly mesh. See the image below for the example of the explosion animation. In this image two out of every three shape keys are removed from frames 19 to 34, four out of five shape keys are removed for frames 34 to 99, and nine out of ten shape keys are removed for frames 99 to 179. The assumption is that the frames between, for example, frames 19 and 22 can be approximated as a linear transition from frame 19 to frame 22. In the case of the explosion animation, we assume that there is more (non-linear) information in the early frames, where there is a lot of movement and many interactions with the collider, and less information in the later frames, where the movement of each polygon is much slower and no longer strongly influenced by the colliders, thus more linear. For the sake of convenience, it is advaced to keep the number of kept frames as evenly spaced as possible for as many frames as possible (e.g. in the example below, 2/3 frames are removed for 15 frames, 4/5 frames are removed for 65 frames, and 9/10 frames are removed for 80 frames, rather than removing a random number for every step; the amount of pruning does __not__ need to be incremental across steps, however).

![An image of the same shape keys as the previous image, but shape keys have been removed as described above.](/images/MDD_tutorial3.png)

Then, set the "Relative to" value for each shape key to the previous remaining shape key in the animation (e.g. frame_0017 should be set relative to frame_0015, and frame_0019 should be set relative to frame_0017). This ensures that the shape key's transformation is set in relation to the one it is set relative to, and iterating over the "Value" value entails a smooth shift from the previous shape key to the selected one.

![An image of the same shape keys as the previous image, but zoomed in on the settings below. The shape key list goes from "frame_0013" to "frame_0019", with the even numbers having been removed. Shape key "frame_0015" is selected, and its "Relative to" value has been set to "frame_0013".](/images/MDD_tutorial4.png)

Finally, make sure that all the key shapes are listed in the correct order and uninterrupted by other shape keys. Shape keys should have completely unique names unless different animations on different meshes are part of the same final animation; in this case the remaining shape keys should match exactly in terms on names and shape key pruning across the objects. Similarly, the mesh that the key shapes are on should also have a unique name for your entire VNyan instance. If all this is done, you can export the object as .fbx or any other format that supports blend shapes as you would normally.

### Configuring the MDD blend shapes in Unity
Set up each of the blend shapes as you normally would for VNyan, but make sure that the names of the blend shapes on the mesh and in VNyan will be identical (e.g. when setting up blend shape clips for a VRM/Vsfavatar model, make sure the blend shape clip names are identical to their respective blend shapes).

### Configuring the plugin and VNyan for use
The settings for the MDD animations can be set in two ways: either though the plugin UI in VNyan, or by setting the parameter values through VNyan's node graphs. The plugin UI allows for more convenient entry of the relevant values. The node graph configuration allows for different animations and configurations to be selected via hotkeys. The names of the parameters in the plugin UI, followed by the related VNyan parameter in parentheses and an explanation of the parameter are as follows:

- Name of Object (mdd_animobject; string value):
  The name of the object/mesh with the blend shapes that make up the animation. This must be an exact match with the name of the object/mesh (e.g. capitalisation). To set this value through the node graph system, make sure to use the Set Text Parameter node.

- Key blend shape indices (mdd_keyframelist; list of ints):
  A comma-separated list of integers containing _indices_ of the key blend shapes of the animation. That is to say, after pruning, which number in the list of shape keys these key blend shapes are. The key blend shapes are the last blend shapes for the different pruning steps outlined above. In the example of the third image above, the key blend shapes are _frame_0019_ (the end of 1/2 shape keys pruned), _frame_0034_ (the end of 2/3 shape keys pruned), _frame_0099_ (the end of 4/5 shape keys pruned), and _frame_0179_ (the end of 9/10 shape keys pruned). _frame_0009_ is off-screen, but corresponds with the last shape key where no shape keys were pruned. In the list of pruned shape keys, these four (or in reality five) shape keys are at the following positions, i.e indices: 9,14,19,32,40 (this shows that after pruning, fourty shape keys are left of the original 180). Note that the _basis_ for the shape keys does not count for this list. To set this value through the node graph system, make sure to use the Set Text Parameter node.

- Frames between blend shapes (mdd_nsteplist; list of ints):
  A comma-separated list of integers containing the number of frames between each blend shape of the animation. In the example of the third image above, these values would be 2 (up to blend shape 14),3 (up to blend shape 19),5 (up to blend shape 32),10 (up to blend shape 40) (and 1,2,3,5,10 with the off-screen blend shapes included). To set this value through the node graph system, make sure to use the Set Text Parameter node.

- \# of frames after animation before reset (mdd_nexitframes; int):

- Offset in blend shape list (mdd_blendshapeoffset; int):

### Setting up multiple animations on one/different meshes
