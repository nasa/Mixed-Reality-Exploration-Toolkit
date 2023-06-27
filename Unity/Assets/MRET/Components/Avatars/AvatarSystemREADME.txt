# Avatar System with Tafi

Allows for avatars to be customized in terms of appearance, clothes, hair, weight, etc..

## Features

-Somewhat rigged base character meshes (6 of them)
-Framework for hair options (drag into HairController script)
-Framework for outfit options (drag into OutfitController script)
-Framework for Blendshapes, only weight can be used due to the rigging issue

## Requirements

-CharacterMeshes GameObject must contain all of the base character prefabs (so far we have Darcy, Millarose, Sukai, Tristan, Silas Sanjay). Their index numbers in the ModelController component go from 0-5 in that order.
-ModelController component must contain references to all of the base character prefabs. Keep indexing the same, or else script must be changed
-Each character prefab parent has the and OutfitController component and a HairController component
-The CharacterCustomizationMenu prefab must be put under WorldSpaceMenus

## How it works

Each controller script is referenced by its respective listener script (BlendshapeSlider, HairSelection, ClothesSelection, ModelSelection). These will take the UI input and call a method from its "controller" script.
To add a new base mesh, new morph, new hair, or new clothes:
	-Under Assets/Framework/Components/AvatarSystem/CharacterBuilder, if you need to add a new character, Create->Character->New Character
		-Drag in desired morphs, ALL clothing and ALL hair options you want available. Match order of other models of the same gender for clothes, hair and morphs.
		-IT IS IMPORTANT THE ORDER IS KEPT THE SAME BECAUSE THE INDEX VALUE IS USED IN THE SCRIPTS
	-To add new morph, hair, or clothes, add them in AFTER the existing ones so as not to change index numbers.
To rig the models, references should already be auto filled in:
	-Under the VR IK Component Drag in the following transforms (Can be found under the Camera Offset GameObject from MainScene)
		-TafiHeadTarget is the Head Target
		-TafiLeftVRIKTarget (1) is the Left Arm Target
		-TafiRightVRIKTarget (1) is the Right Arm Target

## Limitations and Solutions

-OutfitController is not working. Need to find a way to access the interior elements of a Nested List.
-Blendshapes only change the SkinnedMeshRenderer. This means a script needs to be written to also change the rig proportions if we want height, armlength, and leglength morphs.
-VR IK is not solving correctly. This will take a lot of testing and small changes before it can work correctly. Ensure that the character prefab is being changed. Once working parameters are found for one of the model, these parameters can be copied over, and rigging should work.
	-Hand orientation is good, everything else should be tweaked
-Clothing options need to be expanded.
-CharacterPreviewCamera doesn't seem to be masking the correct layers.
-All sliders are included in the UI, but only the weight one works. 

## Integration into MainScene

-Create a CharacterMeshes empty GameObject as a child of the ViveRig
-Drag the 6 character prefabs from AvatarSystem/prefabs under CharacterMeshes
-Under the VR IK Component Drag in the following transforms (Can be found under the Camera Offset GameObject from MainScene)
		-Head Target: TafiHeadTarget
		-Left Arm Target: TafiLeftVRIKTarget (1)
		-Right Arm Target: TafiRightVRIKTarget (1)
-Under the CharacterMeshes GameObject, add the BlendController script and ModelController script
	-Fill in "Characters" reference for the ModelController component by dragging in each of the 6 character models.
	-Ensure that the "Parent" reference is to itself (CharacterMeshes)
-VOILA!