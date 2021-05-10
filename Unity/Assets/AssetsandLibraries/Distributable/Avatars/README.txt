Astronaut model from https://cubebrush.co/domenicodalisa/products/vsuspw-ObSi/discovery-pay-what-you-want-2017
For official release may need to buy from artist if this model is used. 

-To use Astronaut prefab, ensure at the very minimum, in the VRIK for the model there is a reference to the
right hand, left hand, and headset targets are and linked to the RightController/rh tracker, LeftController/lh tracker, and HeadsetFollower/head_tracker respectively 


-To use Desktop Prefab just drag and drop into scene and ensure the RidigdbodyFirstPersonController, is linked to the animation script on the prefab, and the
animation link is linked to the RidigdbodyFirstPersonController.

-To change out models, make sure rig is set to humanoid and then put appropriate scripts onto it, for VR it's VRIK and for desktop its the animation_link script

NOTE: if model for VR is clipping halfway into the ground, after placing all scripts where necessary, duplicate your model fbx make sure Rig animation type is Generic, 
and then go to the VR prefab and set the avatar in the animator is set to the avatar for the duplicated generic model.

NOTE2: if model for Desktop isn't movind ensure the animator Rig animation type is humanoid and the animator controller is set as third person controller. 

If you have any questions contact Jonathan Reynolds jonathan.reynolds@nasa.gov or reynoldsjt24@gmail.com