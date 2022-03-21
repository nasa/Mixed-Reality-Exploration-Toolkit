noah/CableConversion

Locations:
All relevant files in Assets\Framework\InProgress

Scripts:
STPReader.cs is the main cable converter script, if finished it would be used to read an STP file and spawn cables in the scene
CableDebug.cs is a debug script used to render certain features of a STP file to a point cloud to better understand the structure of the models
TempEditor.cs (located in editor folder) is used to create buttons on CableDebug components to make it easier to use

Features/functionality:
STPReader can render cables to point clouds (create a separate game object in the same scene, attach a PointCloudViewerDX11 component to it, then reference that component
in the STPReader component, same process for CableDebug components as well)

If in a MRET scene, reference the DrawLineManager in the STPReader component attached to a GameObject to spawn a cable (right now the cable will just spawn at the point
positions, so it will likely be very large compared to the size of a player

In the "Feature" section of CableDebug, put in the beginning of the name of a STP file feature to render it to a point cloud ("I usually used "b spline" or "cartesian")

Bugs:
Sometimes segments of the same cable are disconnected, so I wrote code to attach these separate segments together to make them a single cable, however this does not work
properly and in the case of the RST model it spawns cable segments seemingly at random in places where cables do not exist in the actual model (enabled by toggling the
Attach Segments bool)

