# Motion Constraints in MRET
*Author: Dev M. Chheda*

The traditional interaction method in MRET allows the user to grab and "pick up" objects.
The object becomes attached to the user's hand and the user can then move and rotate that
object freely in all three dimensions. This interaction can be unrealistic and undesired,
especially when seeking to simulate moving large quipment and hardware.

Motion Constraints introdues a new method for interacting with objects in MRET. In Motion
Constraints mode, objects are restricted to "sliding" along the floor. When the user 
interacts with an object, the user "pushes" and "pulls" the object along the floor. 
In the most simple scenario, the user pushes and pulls the object only along a flat floor,
and the result is equivalent to allowing no motion in the vertical direction. Motion 
Constraints also supports angled floors (i.e. ramps) and smooth transitions between floors 
which are angled with respect to each other. As part of the development of Motion Constraints,
object-aligned bounding boxes are also added to MRET.

# Table of Contents
- [User Guide](#user)
- [Architecture and Overview of Changes](#architecture)
- [Core Motion Constraints Computations](#main)
    - [Detecting floor surfaces](#detect)
    - [Aligning to a floor surface](#align)
    - [Transitioning between floor surfaces](#transition)
- [Object-Aligned Bounding Boxes](#oabb)
- [Additional Considerations](#additional)
    - [Height management system](#height)
    - [Floor edge detection](#edge)
    - [Changing "down" direction](#down)
    - [Grabbing and highlighting](#grab)
    - [Defining floors](#floors)
- [Known Issues](#issues)
- [Future Work](#future)

# <a id="user"></a> User Guide

Motion Constraints mode is designed to be easy-to-use for MRET users. To globally toggle
Motion Constraints mode, simply open the MRET menu, and go to the last page. On the last
page is a button to toggle Motion Constraints on and off. 

Once Motion Constraints is enabled, any interactable object in the scene should be constrained
to the defined floors in that scene. As of now, many of the customizable options for Motion
Constraints are not available at the user level, but are available at the code and Unity editor 
level. Future user options are further described in [Future Work](#future).

While great care has been put into preventing glitchy behavior, there are some cases
(listed in [Known Issues](#issues)) that may lead to undesired behavior. If you run into 
any issues while using Motion Constraints (e.g. an object oscillating or clipping into 
the floor), often the easiest fix is simply to turn Motion Constraints off,
move the object to a "safe" location (i.e. one where no glitches were occuring), and 
then turn Motion Constraints on again. 

# <a id="architecture"></a> Architecture and Overview of Changes

Motion Constraints is implemented as part of the `SceneObject` class. It is implemented as
an alternative to the regular attach grabbing interaction. Motion Constraints is toggled
using a menu option, which is reflected by changes to `MenuAdapter` and `ModeNavigator`.
The status of Motion Constraints (i.e. whether it is enabled) is stored in an object
in `DataManager` with the key `SceneObject.motionConstraintsKey`. 

To keep track of how a specific `SceneObject` is being interacted with at any given point
in time, the `AttachGrabMode` enum is introduced, which currently has three members: 
`None`, `Regular`, and `Constrained`, corresponding to no interaction, regular grab 
interaction, and motions constraints interaction, respectively. The value of `attachGrabMode`
is updated when the user grabs and ungrabs the object and depends on whether Motion
Constraints is enabled. 

When the user grabs an object, the `BeginGrab()` method is called, which makes a call to `AttachTo()` if
the `GrabBehavior` is set to `Attach`. Note that regular and constrained motion are sub-options
of attach grabbing. `AttachTo()` was originally a method of `SceneObject`'s parent class, `Interactable`,
but is now overriden by `SceneObject` to allow for motion constraints behavior. The new
`AttachTo()` first checks if Motion Constraints is globally enabled. If not, regular attach
grabbing is performed, and the `attachGrabMode` is set to `Regular`.

However, if Motion Constraints is enabled, the `attachGrabMode` is set to `Constrained`, 
and Motion Constraints are initialized using `InitializeMotionConstraints()`. 
Additionally, a reference to the transform of the grabbing hand is stored in 
`constrainedMotionTarget` for use in Motion Constraints computation. Specifically, the 
object will "follow" the position of `constrainedMotionTarget`. The updated value of 
`attachGrabMode` enables Motion Constraints computation every frame in `Update()` by 
calling the `ConstrainMotion()` method, which contains all core Motion Constraints computations. 

When ungrabbing, the `EndGrab()` method is called, which calls `Detach()`. `Detach()` 
sets `attachGrabMode` to `None`, all relevant references and variables are reset, and
if the object was in Motion Constraints mode, Motion Constraints are deinitialized with
`DeinitializeMotionConstraints()`.

The `FloorHitData` struct is added and used to store the important information from
ray casts that search for floor objects. Specifically, it is the return type of
the `FindFloorSurface()` method, as described in [Detecting floor surfaces](#detect).

An `ObjectAlignedBoundingBox` is used when making Motion Constraints computations,
so during the initialization of Motion Constraints, the `objectAlignedBoundingBox` for this
object is automatically computed if undefined. More information about `ObjectAlignedBoundingBox`
is provided in [Object-Aligned Bounding Boxes](#oabb).

There are also notable changes made to the `HandInteractor` class to ensure compatibility 
with Motion Constraints. These are further expanded on in [Grabbing and highlighting](#grabbing).


# <a id="main"></a> Core Motion Constraints Computations

At its core, Motion Constraints is about restricting objects to slide along floors. 
This requires detecting floor surfaces and choosing a floor as the "current" floor,
aligning the target object to that current floor and restricting the objects motion,
and finally, handling transitions between floors.


## <a id="detect"></a> Detecting floor surfaces

Floor objects are detected by using Unity's `Physics.Raycast()` to find the floor directly
beneath the object. The ray cast points in the "down" direction, as defined by `DownDirection`.
See [Changing "down" direction](#down) for scenarios in which this down direction could be 
variable.

Ray casts are performed by the `FindFloorSurface()` method, which returns a `FloorHitData`
object containing the relevant data from the ray cast. The methods `CenterRayCast()` and 
`BoundingBoxRayCast()` are used to specifically find floors beneath the center of the 
`objectAlignedBoundingBox` and beneath its edges. The center ray cast is used for primary 
determination of the current floor and floor normal, but the bounding box ray casts are
used for advanced transitions and to prevent objects from falling. 


## <a id="align"></a> Aligning to a floor surface

Floor alignment is done by rotating the object to align its `transform.up` with the
floor normal. This is performed by the `AlignWithSurface()` method. `AlignWithSurface()`
computes the rotation and then calls `SafeRotation()`, which ensures that the rotation
will not cause oscilations or other undesired behavior. Note that the floor normal is
obtained from `FindFloorSurface()`, as Unity's ray cast returns the surface normal.

Specifically, `SafeRotation()` rotates the object by the specified rotation, ensures that
this new rotation maintains the results of all ray casts (i.e. that the center and edges 
of the `objectAlignedBoundingBox` remain above the same floors), and if not, returns the 
object to its original rotation. 

`SafeRotation()` is needed due to an issue with oscillating behavior that arose during
testing. When transitioning between floors, the object would rotate to match the 
orientation of the new floor, which changes the orientation of the bounding box, and
thus changes the position of of the bottom center and edges of the bounding box 
(which are used as the origins of ray casts in `CenterRayCast()` and `BoundingBoxRayCast()`).
This leads to the ray casts returning different floor results; specifically, due to the 
rotation, the ray casts no longer detect the new floor, and only detect the old floor. As
a result, the object re-rotates back to the orientation of the old floor, and the cycle
repeats. The object oscillates between the two rotations, leading to a visually 
unpleasant result. `SafeRotation()` prevents this by not allowing rotations that
change the results of `CenterRayCast()` or `BoundingBoxRayCast()`. After testing,
we find that this works quite well, and doesn't introduce any new issues.

It is worth noting that the oscillating behavior also arose with the addition of 
the advanced transitions, so `SafeRotation()` was still needed.


## <a id="transition"></a> Transitioning between floor surfaces

The simplest way to transition between floors is to simply align with the floor surface
beneath the center ray cast. However, this will lead to choppy behavior on transitions
where the object suddenly rotates from being aligned with the previous to being aligned 
with the next floor. So, instead, `AdvancedTransition()` allows for smooth transitions
between floors surfaces. `AdvancedTransition()` first checks if the center and all
edges of the `objectAlignedBoundingBox` are above the same floor. If so, regular floor
alignment computation is performed. Otherwise, an "intermediate" rotation is computed
using the results of the bounding box edge ray casts and cross products to find an
orientation for the box that is perpendicular to the ray cast hit points. Through testing,
this computation seems to work quite well at producing realistic results, although, 
occasionally, the orientation may seem abnormal.


# <a id="oabb"></a> Object-Aligned Bounding Boxes

A key component of all of the core motion constraints computations is that each target 
object requires an object-aligned bounding box. The `ObjectAlignedBoundingBox` class is 
added expressly for this purpose. An object-aligned bounding box is a bounding box on 
an object that is aligned with the object's local axes. Each `ObjectAlignedBoundingBox`
stores a reference to the transform it is bounding, an offset from the transform's 
position, and bounds in the `transform.right`, `transform.up`, and `transform.forward`
directions (i.e. the local $x$, $y$, and $z$ axes, respectively). Once instantiated,
an `ObjectAlignedBoundingBox` can compute different positions in the bounding box
using the `ScaleBoundingVector` method. This method scales vectors with components
between -1 and 1 to the extents of the bounding box.

`ObjectAlignedBoundingBox` contains the static `IsValid` method for determining if an 
object-aligned bounding box is defined and nondegenerate. Additionally, the class contains the 
static method `Encapsulate`, which computes an object-aligned bounding box which 
encapsulates the colliders of a given transform and all of its children. `Encapsulate` is
used for the automatic creation of an object-aligned bounding box for a `SceneObject` 
when none is provided.

While `ObjectAlignedBoundingBox` is currently only used for Motion Constraints 
computations, it is designed to be easily useable by other parts of MRET.


# <a id="additional"></a> Additional Considerations

## <a id="height"></a> Height management system

One of the largest issues that came up during the initial testing of Motion Constraints 
was height inconsistency. When transitioning between angled floors many times, the floor 
distance (i.e. height) often decreases due to the fact that the ray cast detects the new 
floor after the object has already moved over that new floor. This means that the ray 
cast hits a part of the new (angled) floor which is closer to the object than the 
previous floor, thus decreasing the floor distance. This effect is specifically observed 
when transitioning between two floors which meet at a concave angle. The effect is often 
reversed (i.e. increasing floor distance) on convex floor angles, for the same reasons.

The reason this is an issue is because if the object's height continually decreases, it 
may clip into the floor, and give incorrect or indeterminate ray cast results (since the 
ray casts originate from the bottom face of the bounding box). Or, if the object's height
continually increases, it will become apparent to the user, and unrealistic.

The solution to this problem is to introduce a height management system for objects. 
The height management system is implemented directly into the main `ConstrainMotion()`
method, but makes use of the `FindHeight()` method, which finds the minimum distance
from the object to the floor. Choosing between different height management methods is
donw using the `HeightManagementMethod` enum; currently there are two height management 
methods: `Maintain` and `Clamp` (there is also `None`, corresponding to no height 
management).

`Maintain` works by computing the height of the object at the start of the frame and at 
the end of the frame, and then simply moving the object vertically by the difference of
these two heights to maintain the same height.

`Clamp` is more flexible, allowing the height to vary in a range, from `heightMinThreshold`
to `heightMaxThreshold`. If the object's height at the end of the frame is outside of the
specified range, the object is moved vertically to bring it back into range.

In testing, both `Maintain` and `Clamp` work fairly well, although `Maintain` can lead to 
strange behavior on transitions due to its strict height consitency requirement. The 
addition of a `HeightManagementMethod` enum also makes it fairly easy to introduce new 
options for height management systems in the future.

## <a id="edge"></a> Floor edge detection

Floor edge detection is necessary for preventing object from entering "no-floor" zones
(i.e. having no floor underneath) and for preventing objects from falling large vertical
distances from one floor to another. The floor detection system is implemented in the 
`EdgeDetection()` method, which takes as input a proposed position delta for the object,
moves the object by that delta, and then checks if all ray casts (from the edges and 
center of the bottom of the object-aligned bounding box) still hit a floor. If all ray
casts are still determinate, the orignal position delta is returned; otherwise, the zero
vector is returned. 

In order to utilize this sytem for prevent falling between floors which have significant
vertical distance between them, the `maxFloorGap` field is introduced. `maxFloorGap` 
specifies the maximum allowable vertical gaps between floors, and then restricts the
length of the ray casts to only find floors which satisfy the constraint. `maxFloorGap`
can be customized for each object programmatically.


## <a id="down"></a> Changing "down" direction

There are many use cases, especially in the NASA domain, in which the down direction 
(which is specified by each `SceneObject`'s `DownDirection`) is not constant. For example,
consider a rotating space station, in which the down direciton always points away from the
center, or an asteroid in which the down direction always points towards the center.

Currently, the down direction can be modified directly and mid-execution from an external
script without any issues. Although, in the future, one could add methods for common 
patterns of changing down direction (e.g. always pointing towards a point, or always pointing
away from a point).


## <a id="grab"></a> Grabbing and highlighting

Introducing a new method of interacting with and grabbing objects in MRET requires 
consideration of the compatibility of other scripts and systems with the new Motion
Constraints mode. The touching, highlighting, and grabbing system is chief among
these. 

Previously, objects were hightlighed and dehighlighted when an `InputHand` entered or 
exited the collider. Additionally, the highlight state would not be updated if the 
triggering `InputHand` was grabbing the object. In `HandInteractor`, a similar triggering
system was used to update which object the hand is currently interacting with, and, if 
the hand was grabbing an object, the current interactable would not be updated.

The previous systems rely on the fact that in regular attach grabbing, an object is being
grabbed by a hand if the object's transform is a child of the hand's transform, and this
can be used to detect whether a specific object is being grabbed by a specific hand. 
However, in Motion Constraints mode, this property is no longer true; rather, the object 
is being grabbed by a specific `InputHand` if the object is following the hand's 
transform; i.e., if the object's `constrainedMotionTarget` is a reference to the hand's 
transform.  To resolve this difference, the `IsGrabbedBy` method is added to `SceneObject`
and its parent class `Interactable` (since `Interactable`, rather than `SceneObject` is 
used by `HandInteractor` for indicating an interactable object). This method accomodates 
the regular attach grabbing by checking if the object's transform is a child of the
hand's transform, and accomodates the constrained motion grabbing in `SceneObject`'s 
override of the method by checking if the object's `constrainedMotionTarget` points to 
the `Transform` as the hand's transform. This method replaces the old method everywhere
that the check for a hand grabbing an object is performed, incuding in `HandInteractor`
and in `SceneObject`. 

Additionally, the previos highlighting system works well for regular attach grabbing 
because whenever a hand is grabbing an object, the object is attached to the hand, and 
so the hand will continue touching the object throughout the grab. However, this is not
always the case for Motion Constraints grabbing; since the object is constrained to move
along the floor, the grabbing hand will often not be touching the object while moving it.
To account for this change, the field `isTouchedByGrabber` is added to `SceneObject` to 
keep track of whether the grabbing hand is still touching the object, and the field 
`stillTouching` is added to `HandInteractor` for the same purpose. Changes to 
`SceneObject`'s `BeginTouch()`, `EndTouch()`, `BeginGrab()` and `EndGrab()` along with 
changes to `HandInteractor`'s `Grab()`, `Ungrab()`, `OnTriggerEnter()`, and 
`OnTriggerExit()` ensure that a grabbed object remains highlighted while being grabbed, 
and is unhighlighted after being ungrabbed if the grabbing hand is no longer touching the 
object.  

The new system is compatible with the old, regular attach grabbing, and with the new, 
constrained motion grabbing. The `IsGrabbedBy()` method should be used anytime one needs
the check if an object is being grabbed by a hand; modularizing this logic into a single
method also makes it much easier to simply update this single method if new interaction
modes are added in the future.


## <a id="floors"></a> Defining floors

Currently, floor objects are defined by a "Floor" tag (defined by `SceneObject`'s 
`FLOOR_TAG`). While this makes selecting floor objects programmatically easy (as is done 
in `FindFloorSurface()`), this requires manual identificition and tagging of floor objects.

# <a id="issues"></a> Known Issues

- Steep overturns: When moving an object over a steep convex overturn between floors, the 
  object will often not transition smoothly, and will often exhibit oscillating behavior 
  or get stuck on the edge between the two floors. In testing, if the user tries to force
  the object over the turn, it can and will shift to the next floor and realign itself.
  The exact cause of this issu is not known, although it could be a side effect of 
  `SafeRotation()`, since rotating on a steep overturn could cause the object to clip 
  into the floor, making such a rotation invalid.

- Hovering: The height management system was introduced because if a target object gets 
  too close to a floor, it can clip into the floor and break Motion Constraints. However, 
  this requires that the target object always has some height above the floor; i.e., the
  object is always slightly hovering above the floor. This can be undesired, both in
  visual and functional aspects. One idea to fix the visual hovering, as suggested by 
  Tom Grubb, is to use a bounding box slightly shorter than the object, so that the 
  bounding box can be "hovering" while the actual object appears to be touching the floor. 

- Partial transitions: Transition computations in `AdvancedTransition()` assume that the
  entire object is transitioning from one floor to another (of course, this is done 
  gradually). Imagine a simple scenario with a flat floor, a ramp in the up direction, 
  and a singe target object. The user move the object along the ramp such that only half 
  of the object actually moves up the ramp, while the other half remains over the flat 
  floor. That is, the user could attempt to push the object up the edge of the ramp. In
  testing, this causes the object to clip into the ramp, or to move part way up the ramp
  before becoming stuck at a strange orientation.

- Toggling Motion Constraints: When using the `Clamp` height management method, one can
  provide both a minimum and maximum allowable height, ensuring that the gap between the
  object and the floor is not too large. However, this constraint is not applied in 
  regular attach grabbing. So, if a user moves an object too far above the floor in 
  regular interaction, when they try to interact with the object in Motion Constraints, 
  it will snap back towards floor. This is primarily a visual issue, as everything still
  functions properly.
  
  Worse, however, is when a user moves an object below a floor in regular interaction, as
  this will lead to the object being stuck an unmovable in Motion Constraints mode (since
  the object will not detect any floor beneath it). Of course, the user can always go 
  back into regular interaction and move the object out of the floor. Perhaps a warning
  that an object has no floor beneath it could be helpful for informing the user of the
  issue.

- Motion Constraints without floors: In some tests, attempting to use Motion Constraints 
  mode when no floor objects have been defined leads to the target object being 
  highlighted and remaining highlighted even after ungrabbing. The issuse is primarily
  visual, since the object doesn't move in Motion Constraints mode (as intended when 
  there are no floors), and after toggling Motion Constraints off, the object can be 
  interacted with as usual.

# <a id="future"></a> Future Work

Some ideas to expand on this work in the future include:

- Add object-specific toggle for Motion Constraints, and user accessibility to this toggle.
- Add an option for users to mark specific objects as floors.
- Add a method for automatically determining and tagging the floor objects in a scene.
- Add options for users to customize Motion Constraints for each object (or even 
  globally). This includes: 
    - Whether to align with floors (`alignWithFloor`)
    - Whether to use smooth transitions (`advancedTransitionFeatures`)
    - What height management method to use (`heightManagementMethod`)
    - Height thresholds for the `Clamp` management method (`heightMinThreshold`, `heightMaxThreshold`)
    - The object-aligned bounding box (`objectAlignedBoundingBox`)
    - The maximum allowable vertical gap between floors (`maxFloorGap`)
- Add more complex behavior for irregularly shaped objects, since the bounding box may
  not be very accurate for complex objects.
- Add different behavior for concave and convex floor turns (if desired)
- Refactor the height management system as its own method. Currently, height management
  is performed directly in the `ConstrainMotion()` method, and is not modularized like
  other components of Motion Constraints are. Moving height management into its own 
  independent method would make it easier to use outside of the context of Motion 
  Constraints.
- Add more complex transition behavior when transitioning between three or more different 
  floors. The current `AdvancedTransition()` is designed with at most two different 
  floors in mind; although it somewhat works for more than two floors, it doesn't work as
  well.

The biggest item here is user customization. Although there are many customizable 
settings for Motion Constraints, most are currently only accessible at the code and Unity
editor level, and cannot be adjusted by the end user. 