# Terrains in MRET
## _Terrain Loading and Streaming from a DEM File_

This feature will allow MRET to load, generate, and stream from raw Digital Elevation Model (DEM) files at runtime. The user must provide the DEM file and other necessary information dependent on the mode of terrain generation.
The different modes are:
- Generate one terrain tile from a DEM file
- Generate one terrain tile by cropping a DEM file (two modes)
    - Use pixel coordinates (x,y) for the center point
    - Use latitude/longitude coordinates for the center point
- Stream a large DEM file by cropping it into smaller DEM files and generating them as tiles

## Set Up
### GDAL Dependency
To use this feature in Unity, GDAL must be installed on the host machine.
1. Install GDAL (x64) on Windows machine from an installer on [GISInternals Support Site](https://gisinternals.com/release.php):
    - release-1916-x64-gdal-3-3-1-mapserver-7-6-4.zip 
    - gdal-303-1916-x64-core.msi (installer) 

2. Add environment variables:
Follow steps 2-4 here: [Installing GDAL for Windows – IDRE Sandbox](https://sandbox.idre.ucla.edu/sandbox/tutorials/installing-gdal-for-windows#:~:text=Step%204%3A%20Testing%20the%20GDAL%20install%201%20Open,result%2C%20then%20congratulations%20your%20GDAL%20installation%20worked%20smoothly%21)

## Usage
### Four Modes of Terrain Generation
The TerrainLoadingManager has four modes of operation. It is set in the manager as one of several enum options. In the code the state enum is defined as so:
```
        public enum State
        {
            GenerateOneTile = 0,
            CropAndGenerateOneTilePixelCenter = 1, 
            CropAndGenerateOneTileLatLonCenter = 2, 
            StreamFile = 3 
        }
        public State state = State.GenerateOneTile; 
```
One way to set the `state` variable is through the Unity Editor, however more practically for MRET it can be set as so:
```
    TerrainLoadingManager.state = State.StreamFile;
```
The filepath is a static variable in TerrainLoadingManager and may be set as so:

    TerrainLoadingManager.filepath = @"C:\path\to\dem\file.raw";
_All modes of operation require a path to a DEM file._

Additionally, modes 2-4 generate new DEM files and save them at the same directory of the inputed DEM file. The TerrainLoadingManager has an Overwrite flag that indicates whether or not these files should be overwritten if they already exist. The default of this flag is true. It can be set as so:
    
    TerrainLoadingManager.Overwrite = true;
Here are the descriptions and additional parameter requirements for the modes of operation:
##### 1. Generate Terrain from DEM File
This is the base mode of operation for the terrain load capability. A Unity terrain will be generated from the height data in the file provided. It is recommended to be in the RAW format (file extension ".raw"), however the TerrainLoadingManager will convert the file to ".raw" if it is not in that format.
The DEM file provided must have one of the following dimensions:
- 32 x 32
- 64 x 64
- 512 x 512
- 1024 x 1024
- 2048 x 2048
- 4096 x 4096

If the DEM file does not have the dimensions from the list above, Unity will not be able to properly generate the terrain.

##### 2. Crop DEM File and Generate Terrain (Pixel Coordinate Center)
This mode allows the user to create a terrain from a subsection of a larger DEM file. The user must provide the desired size of the cropped DEM file (choosing from one of the options in the list of dimensions under the first mode of operation). The user must also provide the point on the larger DEM file that will be the center point on the new cropped file. For this mode, the user provides the center point as a pixel coordinate with x and y components.
###### Use:
 An example of how to set these parameters:
```
    TerrainLoadingManager.cropSizeX = 512;
    TerrainLoadingManager.cropSizeY = 512;
    TerrainLoadingManager.centerX = 120;
    TerrainLoadingManager.centerY = 347;
```
##### 3. Crop DEM File and Generate Terrain (Lat/Lon Coordinate Center)
The description is the same as the previous mode, however the center coordinate type is in latitude/longitude instead of pixels. The user also needs to provide the following four pieces of information for this mode to work:
- Westernmost Longitude
- Easternmost Longitude
- Minimum Latitude
- Maximimum Latitude

Since the center point is known in lat/lon, these four points should also be known by the user.
###### Use:
 An example of how to set these parameters:
```
    TerrainLoadingManager.cropSizeX = 1024;
    TerrainLoadingManager.cropSizeY = 1024;
    
    TerrainLoadingManager.westernmostLongitude = 3.29;
    TerrainLoadingManager.easternmostLongitude = 3.84;
    TerrainLoadingManager.minimumLatitude = 25.11;
    TerrainLoadingManager.maximumLatitude = 26.87;
    
    TerrainLoadingManager.centerX = 3.654;
    TerrainLoadingManager.centerY = 25.436;
```
##### 4. Stream Terrain from DEM File
This mode allows for Unity to stream the terrain from the given DEM file. This is done by separating the file into smaller tiles to stream, so the user must provide the desired size of each tile.
###### Use:
 An example of how to set these parameters:
```
    TerrainLoadingManager.tileSizeX = 512;
    TerrainLoadingManager.tileSizeY = 512;
```
## Testing
There is already a scene set up to test each of the four modes: GenerateAndStream scene tests the functionality of this new feature separately from MRET. All that needs to be done is to select the mode of operation from the TerrainLoadingManager component on the TerrainLoadingManager GameObject. Then the requirements described in the previous section must be entered.

The LunarStreamTest tests only the streaming (not terrain loading/generation) but may need to be updated.

Here is an example DEM file I used in my testing: [http://wms.lroc.asu.edu/lroc/view_rdr/NAC_DTM_APOLLO15_2];

## Future Development Tasks
### Linking DynamicTerrainStreaming to TerrainLoadingManager
The DynamicTerrainStreaming namespace was the second approach to terrain streaming. This method involves creating the terrain tile DEM files and objects on the fly as needed and causes a bit of lag during runtime.
It is similar to the original terrain streaming classes, but there are some differences such as the data structures used in this implementation. It is currently not linked with the TerrainLoadingManager but this process should not be difficult.
### Use in MRET
This capability has been developed outside of the MRET interface and needs to be added via the new schema of MRET. When a user wants to use a terrain in the scene, they should be able to do so by indicating the DEM file, the desired mode of operation, and the parameters needed for that mode. They may also set the overwrite flag as well.
Notes: The GameObject with the character controller must have the TerrainCollision component attached to it.
### High Resolution Terrain Tiles
One ticket assigned to me that I did not accomplish during my time working on the terrain loading and streaming was HiRes terrain tiles. The description says: "Ability to substitute higher resolution textures for a terrain pixel". This ability must be tackled in the future.
### Script to install GDAL
Instead of adding a task for the user to do, we should make a script that installs GDAL automatically upon running MRET.

### Other Future Tasks
- Mixing source resolutions (closer high resolution, further away is lower) 
- Layering (different types) 
- Geology maps 
- Slope maps 
- Orbital images 
- Spot texturing (foot steps, geology map) 
- GeoJSON can populate information (like rocks) on terrain 
- Path over terrain to follow contours of terrain (like GPS route) 
- Specify lat/long for current location or teleporting 
- Texturing 
- Randomly rotate repeatable pattern to reduce the repeating pattern (we want it to look like a whole surface) 
- Display in project overview map 

# Authors
- Molly Goldstein (molly.goldstein@nasa.gov)