Overview A 3D wall map of a scene allows a user to see a top-down view of the scene. The panel can be activated via MRET’s menu and can be adjusted via the value of zoom and by grabbing and rotating the panel. 

The “MinimapPanel” prefab is the menu panel prefab which handles the instancing of the minimap.

The “Minimap” prefab can be placed into a scene in a collaborative session. That is, multiple people can view a single minimap.

Both minimap prefabs contain a MinimapController and Minimap Camera. The minimap controller contains the minimap controller script and object pool. The script contains the user’s position, the minimap camera and the object pool. The object pool contains the Imageicon prefab which handles drawn map objects.

The “MinimapCamera” contains the “minimap” script which references the player and allows for customizable options such as, if the minimap should move andor rotate with the player. The minimap camera also contains the minimap height map reader. The way this works is a ray is casted down onto the terrain the minimap is located (or the player for that matter) and the terrain is selected and fed into the minimap heightmap reader. This changes the color of the minimap heightmap to white depending on how close it is to the camera. This is optimized well for larger terrains, such as the lunar south pole. This value can then be converted to a vertex displacement shader. 

The “MinimapDrawnObject” script allows a UI element sprite to be shown on the minimap at the location of an object in 3D space. Simply add a “MinimapDrawnObject” component to an asset in the scene and select an image sprite to be associated with it.

The “MinimapHeightMapReader” script references the minimap camera and allows for the terrain layer to be selected. (This is the layer that the height maps will be read from).

The Minimaps’s plane which shows the 3D minimap material allows the user to see the depth of terrain given the minimap camera.

The “UIMinimapHeightmapDisplay” script displays the 3D minimap material as a UI overlay on the minimap. It makes the parts of the terrain closer to the .

Object Pool and Method Delayers are “helper” classes of sorts which ensure that new the minimap is togglable without losing the UI elements.


Each script is commented on along with ToolTips (hover over) on public variables in the editor.
