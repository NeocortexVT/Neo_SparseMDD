# Neo_SparseMMD
Code for the Sparse MMD animation plugin for VNyan

## About the Sparse MMD plugin
MMD animations are a useful tool to animate 3D objects without relying on armatures. When an animation on a 3D object is converted to the MMD format, a blend shape is gererated for each frame of the animation, and to recreate the animation, these blend shapes are cycled through in order. This way, physics-based animations in Blender can be exported into Unity, for example.

The problem with MMD animations is that for each frame, a blend shape is stored, and thus vector data has to be stored for each frame. A five second animation at 60fps requires three hundred blend shapes. On a mesh with 70k polygons (generally the upper limit for VRChat if these polys are tris), the resulting file-size can easily exceed 1GB in size. Converting a model of such size to the .VRM format quickly becomes unworkable, as memory requirements exceed limits not uncommon in consumer-level systems. Subsequently, such animations provide a challenge when tried to be implemented on VTuber models.

The Sparse MMD plugin is a plugin for VNyan that works around this limitation to allow MMD animations to be created for vtuber models. By pruning the number of blend shapes, and iterating over the remaining blend shapes, the original animation can be approximated. The underlying assumption is that moving from one blend shape to the next is roughly linear, and going between blend shapes in a manner proportional to the number of blend shapes (i.e. frames) that were pruned gives a desirable result. The plugin is configured in such a way that the density of the pruning does not have to be homogenous across the full animation, so non-linear effects can easily be maintained, but also maximally optimise the number of blend shapes. 

See for example the animation below, where an exploding mesh collides with another mesh. Moving from the base state of the mesh to the last frame in a linear fashion (as would be the case when using a single shape key) would mean polygons have to travel through the mesh. Instead, by keeping a lot of the MMD shape keys at the start, the animation is recreated faithfully. Later on, the movement from one frame to another no longer passes through the collider, and so the intermediary shape keys can be pruned.

https://github.com/user-attachments/assets/fcfc5c24-ccec-4cac-9fe2-156ef64469b1

## Adding the plugin to VNyan
To install the plugin, please go to the [Itch page](https://neocortexvt.itch.io/sparse-mmd-plugin-for-vnyan) and download the zip-file from there. Extract the files into the VNyan/Items/Assemblies folder. Make sure 3rd party plugins are enabled in VNyan under the Settings>Misc menu. After that, restart VNyan and it should be ready to go.

## How to set up your model and MMD blend shapes
### Modifying the blend shapes for Sparse MMD in Blender
For a tutorial on how to convert animations to MMD, see [this video](https://www.youtube.com/watch?v=sdl-jpZ0NR0&). Note that this plugin relies on setting up blend shapes for the object you wish to animate. As a result, the plugin can only be used for animations on VRM/Vsfavatar files. Animations for other types of files can be set up through Unity animations instead.

Once you have converted your animation to mmd and applied it to the mesh, your blender should look something like this:

![A screenshot of the animation timeline in Blender with key frames set up for every frame.](/images/MMD_tutorial1.png)
![A screenshot of the shape keys in the data tab of an object in Blender. There is a shape key for each frame, named "frame_xxxx" where xxxx is a number between 0000 and 9999, here ranging from 0071 to 0098.](/images/MMD_tutorial2.png)

Next up, it is time to prune the shape keys. This step is technically optional, but may be required depending on the animation length and poly count of the mesh. The brain explosion animation, for example could not be converted to a VRM with 180 shape keys, so these needed to be pruned; see the image below. In this image two out of every three shape keys are removed from frames 19 to 34, four out of five shape keys are removed for frames 34 to 99, and nine out of ten shape keys are removed for frames 99 to 179. The assumption is that the frames between, for example, frames 19 and 22 can be approximated as a linear interpolation between frame 19 and frame 22. In the case of the explosion animation, we assume that there is more (non-linear) information in the early frames, where there is a lot of movement and many interactions with the collider, and less information in the later frames, where the movement of each polygon is much slower and no longer strongly influenced by the colliders, thus more linear. For the sake of convenience, it is advised to keep the number of kept frames as evenly spaced as possible for as many frames as possible (e.g. in the example below, 2/3 frames are removed for 15 frames, 4/5 frames are removed for 65 frames, and 9/10 frames are removed for 80 frames, rather than removing a random number for every step; the amount of pruning does __not__ need to be incremental across steps, however).

![An image of the same shape keys as the previous image, but shape keys have been removed as described above.](/images/MMD_tutorial3.png)

Then, set the "Relative to" value for each shape key to the previous remaining shape key in the animation (e.g. frame_0017 should be set relative to frame_0015, and frame_0019 should be set relative to frame_0017). This ensures that the shape key's transformation is set in relation to the one it is set relative to, and iterating over the "Value" value entails a smooth shift from the previous shape key to the selected one.

![An image of the same shape keys as the previous image, but zoomed in on the settings below. The shape key list goes from "frame_0013" to "frame_0019", with the even numbers having been removed. Shape key "frame_0015" is selected, and its "Relative to" value has been set to "frame_0013".](/images/MMD_tutorial4.png)

Finally, make sure that all the key shapes are listed in the correct order and uninterrupted by other shape keys. Shape keys should have completely unique names unless different animations on different meshes are part of the same final animation; in this case the remaining shape keys should match exactly in terms of names and shape key pruning across the objects. Similarly, the mesh that the key shapes are on should also have a unique name for your entire VNyan instance. When all this is done, you can export the avatar as .fbx as you would normally.

### Configuring the MMD blend shapes in Unity
Once the FBX is converted to VRM in Unity, set up blend shape clips for each blend shape as normal. Make sure that the names of the blend shape clips and the blend shapes are identical. Then export the model as normal as a VRM or VSFavatar file.

### Configuring the plugin and VNyan for use
The settings for the MMD animations can be set in two ways: either though the plugin UI in VNyan, or by setting the parameter values through VNyan's node graphs. The plugin UI allows for more convenient entry of the relevant values. The node graph configuration allows for different animations and configurations to be selected via hotkeys. The names of the parameters in the plugin UI, followed by the related VNyan parameter in parentheses and an explanation of the parameter are as follows:

![A screenshot of the plugin UI with all parameters set. The full lsit of parameters are listed below.](/images/MMD_tutorial5.png)

- Name of Object (mmd_animobject; string value):
  The name of the mesh with the blend shapes that make up the animation. This must be an exact match with the name of the object in Unity (e.g. capitalisation). To set the _mmd_animobject_ value through the node graph system, make sure to use the Set Text Parameter node.

- Key blend shape indices (mmd_keyframelist; list of ints):
  A comma-separated list of integers containing _indices_ of the key blend shapes of the animation. That is to say, after pruning, the positions of these key blend shapes in the list of shape keys. The key blend shapes are the last blend shapes for the different pruning steps outlined above. In the example of the third image above, the key blend shapes are _frame_0019_ (the end of 1/2 shape keys pruned), _frame_0034_ (the end of 2/3 shape keys pruned), _frame_0099_ (the end of 4/5 shape keys pruned), and _frame_0179_ (the end of 9/10 shape keys pruned). _frame_0009_ is off-screen, but corresponds with the last shape key where no shape keys were pruned. In the list of pruned shape keys, these four (or in reality five) shape keys are at the following positions, i.e. have the following indices: (9,)14,19,32,40 (after pruning, fourty shape keys are left of the original 180). These values __include__ any blend shapes present on the model that are not related to the animation (e.g. if the object were to have _A_,_E_,_I_,_O_,_U_ blend shapes before the animation, all values would go up by 5). Note that the _basis_ for the shape keys does not count for this list. To set the _mmd_keyframelist_ value through the node graph system, make sure to use the Set Text Parameter node.

- Frames between blend shapes (mmd_nsteplist; list of ints):
  A comma-separated list of integers containing the number of frames between each blend shape of the animation. In the example of the third image above, these values would be 2 (up to blend shape 14),3 (up to blend shape 19),5 (up to blend shape 32),10 (up to blend shape 40) (and 1,2,3,5,10 with the off-screen blend shapes included). To set the _mmd_nsteplist_ value through the node graph system, make sure to use the Set Text Parameter node.

- \# of frames after animation before reset (mmd_nexitframes; int):
  The plugin automatically resets all blend shape values of the animation to 0 once the animation finishes running. Setting this value will postpone the reset with the specified amount of frames. To set the _mmd_nexitframes_ value through the node graph system, make sure to use the Set Parameter node.

- Offset in blend shape list (mmd_blendshapeoffset; int):
  By default, the plugin assumes that the first blend shape on the object is the first blend shape in the animation. If the object has blend shapes unrelated to the animation that precede the blend shapes of the animation (see the _A_,_E_,_I_,_O_,_U_ example above), then this value can be used to tell the plugin to skip those. Similarly, this value can be used to select different animations if an object has blend shapes for multiple aniamtions on it. The value should be equal to the number of blend shapes that precede the animation blend shapes (in the _A_,_E_,_I_,_O_,_U_ example, it should be set to 5). To set the _mmd_blendshapeoffset_ value through the node graph system, make sure to use the Set Parameter node.

When all of these values are set, the animation can be played by activating a Call Trigger note with the Trigger Name set to MMD_runAnimation (make sure Call Trigger is the last to activate in the chain of nodes). When the animation is finished, the plugin will call a trigger called _MMD_exitAnimation_, so that a Trigger node can be activated as soon as the animation ends.

### Setting up multiple animations on one/different meshes
It is possible to make multiple animations for one model, and play them using the node graphs in VNyan. If the animations are on different meshes, then the node chain for each animation should include a Set Text Parameter node that sets _mmd_animobject_ to the specific mesh for the desired animation (as well as nodes setting the other parameter values specific to that animation). If multiple animations are present on one mesh, the first animation can simply be set up by including a node that sets the final value in _mmd_keyframelist_ to the final blend shape in the first animation. For subsequent animations, the _mmd_blendshapeoffset_ should be used in order to start on the correct blend shape (and the final value in _mmd_keyframelist_ should again be the final blend shape for the specific animation).

###Special thanks to [AStrayRae](https://www.twitch.tv/astrayrae) and [Suvidriel](https://www.twitch.tv/suvidriel) for their support in developing this plugin.
