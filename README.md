<p align="center"><img src=https://github.com/nasa/Mixed-Reality-Exploration-Toolkit/blob/master/Unity/Assets/Media/Images/MRET-Banner.png width=250></p>

# Mixed Reality Exploration Toolkit (MRET)

The Mixed Reality Exploration Toolkit (MRET) is an XR toolkit designed for rapidly building environments for NASA domain problems in science, engineering and exploration. It works with CAD-derived models and assemblies, LiDAR data and scientific models.

## Installation (Built Package)

Download the release package from the [release page](https://github.com/nasa/Mixed-Reality-Exploration-Toolkit/releases/tag/v21.3.0). Simply extract the zip folder and run MRET.exe.

* [VR](https://github.com/nasa/Mixed-Reality-Exploration-Toolkit/releases/download/v21.3.0/MRET21.3VR.zip)
* [Desktop](https://github.com/nasa/Mixed-Reality-Exploration-Toolkit/releases/download/v21.3.0/MRET21.3Desktop.zip)

## Installation (Development Environment)

`git clone https://github.com/nasa/Mixed-Reality-Exploration-Toolkit` into the folder you would like the MRET project to be for your Unity **2020.3.17f1**. Yes, that specific Unity version is important!

In Unity Hub “ADD” the MRET project from the folder where you cloned it to.
Once Unity opens, **DO NOT** change the scene to MRET scene, because you want the necessary assets to be included in the project before. 
Hence import all these assets first into the project under [Unity/Assets/AssetsandLibraries/Non-Distributable](https://github.com/nasa/Mixed-Reality-Exploration-Toolkit/tree/master/Unity/Assets/AssetsandLibraries/Non-Distributable):

### Free assets:

* [MRTK](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/download/v2.7.2/Microsoft.MixedReality.Toolkit.Unity.Foundation.2.7.2.unitypackage) Version 2.7.2
* [VR Capture](https://assetstore.unity.com/packages/tools/video/vr-capture-75654) Version 11.6
* [ROSSharp Unity](https://github.com/siemens/ros-sharp/tree/master/Unity3D/Assets) Version 1.7

### Paid assets:

While MRET is free and [open-source](https://opensource.gsfc.nasa.gov/documents/NASA_Open_Source_Agreement_1.3.pdf), it does rely on third party assets that aren't.

Prices give a ballpark estimate for building MRET as of 2023.03.21. The prices of these assets fluctuate over time and may differ from what is listed.

* [Best HTTP/2](https://assetstore.unity.com/packages/tools/network/best-http-2-155981) Version 2.5.4 - $60.
* [Easy Build System](https://assetstore.unity.com/packages/templates/systems/easy-build-system-45394) Version 5.4.4 - $20.
* [Embedded Browser](https://assetstore.unity.com/packages/tools/gui/embedded-browser-55459) Version 3.1.0 - $75
* [Final IK](https://assetstore.unity.com/packages/tools/animation/final-ik-14290) Version 2.0 - $90
* [Graph & Chart](https://assetstore.unity.com/packages/tools/gui/graph-and-chart-78488) Version 1.95 - $85
* [Non Convex Mesh Collider](https://assetstore.unity.com/packages/tools/physics/non-convex-mesh-collider-84867) Version 1.0 - $9
* [Universal Media Player](https://assetstore.unity.com/packages/tools/video/ump-win-mac-linux-webgl-49625) Version 2.0.3 - $45
* [Universal Sound FX](https://assetstore.unity.com/packages/audio/sound-fx/universal-sound-fx-17256) Version 1.4 - $40 

### Tricky assets:

* [Pointcloud viewer and tools](https://assetstore.unity.com/packages/tools/utilities/point-cloud-viewer-and-tools-16019) Version 2.40 - $100 When importing this one, uncheck the “Editor” folder (otherwise MRET will run in Unity but not compile).
* [Runtime OBJ Importer](https://assetstore.unity.com/packages/tools/modeling/runtime-obj-importer-49547) Version 2.02 - Free
After importing this asset, modify the string in "Shader.Find" on line 160 of MTLLoader.cs to "HDRP/Lit" (i.e. Shader.Find("HDRP/Lit")). If this is not HDRP, it should be set to "Standard".

### Unity Packages
* If the OpenVR Unity XR package doesn't install correctly using the Package Manager, it can also be downloaded from [GitHub](https://github.com/ValveSoftware/unity-xr-plugin). Note, you will need to remove the reference to the OpenVR Unity XR package from the package.manifest if going this route.

Now navigate in Unity to `Project > Assets > Framework > Scenes > MainScene` and open either VR or Desktop scene.

(you will still need the MRET Components to be placed, where they need to be placed to..)

### Additional Configuration
Ensure that Project Settings->Player->Other Settings->Active Input Handling is set to "Both".

## Quick Start - Loading a Project
MRET projects can be opened at any point by following the following steps:
1.Open the controller menu.
2.Navigate to File->Open Project.
3.Select a project from the file browser. Press “Open”. It may take a few minutes for the project to completely load.

## Contributing

Pull requests are welcome. For more information on contributing, please see [Contributing.md](https://github.com/nasa/Mixed-Reality-Exploration-Toolkit/blob/master/CONTRIBUTING.md).

## License

[NOSA](https://opensource.gsfc.nasa.gov/documents/NASA_Open_Source_Agreement_1.3.pdf)
