Procedural Part Generator Documentation

All scripts are commented appropriately.

Overview: This allows for an asset (rock) to be placed at a random location within a certain area with a random rotation along the Y-axis.
Rocks cannot overlap and are raycasted onto terrain, meaning they will always be on the surface.
Each rock's vertices are altered slightly to create a uniqueness to each moon rock.
A scale randomization allows for rocks to be from pebbles to rocks.
Boulders can also be placed.

////////////////////////////////////////////

See ‘Procedural Rocks’ scene.

In hierarchy, see 'Rock Spawner' prefab:

Rock X/Y/Z Spread: This is the area in the scene you want rocks to be procedurally generated. Use Y spread to control how much variant you want when rocks clip into the ground, should be 0 for no clipping.

Num Rocks To Spawn: The number of rocks you want to spawn within the constraints of the X/Y/Z Spread.

///////////////////////////////////////////

Rock Alignment: 

Size: The number of different rock types you would like in your scene.

Raycast Distance: The length of the rays that are casted from “Rock Spawner” prefab

Overlap Box Size: Size of the box that checks for collisions with other rocks

Spawned Layer: Layer that rocks should be spawned upon. Ensures that the only other collisions it detects are other rocks.

///////////////////////////////////////////

Each rock is a prefab varient of Rock_default with scripts applied