Overview: Path visualization allows for an optimized, lightweight line to be rendered between an array of Vector3 points. This is useful for path visualization spacecraft and ground vehicles. Additionally, these points can be casted and interpolated to the ground in order to make a smooth path along the terrain. Breadcrumbing allows for a game object to be tracked in 3D space and have a path drawn along with it.

The “PathVisualization” prefab asset contains the scripts “PathVisualizer” and “PathVisualizationHandler,” along with a “Line Render.” 

The Path Visualizer allows for control of the layer that is to be casted to (Terrain, everything, etc.). This script should reference the Prefab’s line renderer. Also if the path should be casted to the ground.

The Path Visualization Handler contains the PathVisualizationPoints scriptable object. This script should also reference the above Path Visualizer Script. The resolution refers to how many interpolated points are generated between two points.

The Line Render allows for the material to be changed that is shown on the lines. It also allows for the width of the drawn line to be adjusted. Additional parameters of interest may include cast/receive shadows and texture mode.

Breadcrumbing is a Prefab that can be assigned to any GameObject and track its position in 3D space. These can then be used in PathVisualization to draw a line where it has been. This line can be casted to the ground as well.

Each script is commented on along with ToolTips (hover over) on public variables in the editor.