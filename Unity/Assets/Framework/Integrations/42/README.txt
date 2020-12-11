42 Menu Documentation 
Written by Kyra Lee

Opening 42 Menu
1) Open Windows Powershell (C:\Users\yourUser\Desktop\GMSEC_API\bin> .\MBServer.exe)
2) Open msys, cd to 42, run 42 TxGMSEC/
3) Open Unity, play Main Scene
4) Go to Forty Two Demo project
5) Open 'Tools' submenu in Menu
6) Scroll down, press 42 Button
7) Instruments will appear on IonCruiser

FortyTwoMenu Prefab (not FortyTwoMenuItem)
- uses FortyTwoMenuController.cs, WorldSpaceMenuManager.cs
- Buttons call 
	a) methods that set instruments active using int array position
		* ex; Geomagnetic Field Vector button OnClick() method calls setVectorActive(1)
	b) colorButton(int pick) (toggle feature, keeps button green when pressed)
- Buttons contain HoverInfo.cs
	* use Text to describe button feature when user hovers
- Filling arrays
	a) Truth Vectors, FSW Vectors, Astro Labels
	***** use "... Base" when adding to array to ensure correct positioning***** 
	b) Must add any new button to Button array (for toggle feature)
		+ there's definitely a more effective way to toggle the button (sorry)
- Coloring instruments (I am colorblind, tried to replicate 42 component colors as accurate as possible)
	a) if in the inertial frame -> red
	b) if in the LVLH frame -> pink 
	c) if in the formation frame -> purple
	d) if in the body frame -> green
	e) if in the galactic frame -> light blue
	f) vectors
		sun -> yellow
		geomagnetic field -> pink
		angular momentum -> light purple
	g) astro labels
		** talk to Eric Stoneking for specification **

Menu Contents 
- 3 Truth Vectors with attached labels (vectors[i])
- 2 FSW Vectors with attached labels (fsw[i])
- 5 Grids (grids[i])
- 4 Axes with attached labels (axes[i])
- Prox Ops (proxOps)
- 10 Astro Labels with attached labels (astro[i])

Truth Vectors 
1) Sun Vector (vectors[0])
	+ Label: "S"
2) Geomagnetic Field Vector (vectors[1])
	+ Label: "B"
3) Angular Momentum Vector (vectors[2])
	+ Label: "H"

FSW Vectors
1) Sun Vector (fsw[0])
2) Geomagnetic Field Vector (vectors[1])

Grids
1) Inertial (grids[0])
2) Local Vertical-Local Horizontal (grids[1])
3) Formation (grids[2])
4) Body (grids[3])
5) Galactic (grids[4])

Axes
1) Inertial (axes[0])
2) Local Vertical-Local Horizontal (axes[1])
3) Formation (axes[2])
4) Body (axes[3])

Astro Labels (** talk to Eric Stoneking for specificationon correct labels **
1) Mercury (astro[0])
2) Venus (astro[1])
3) Earth (astro[2])
4) Mars (astro[3])
5) Jupiter (astro[4])
6) Saturn (astro[5])
7) Uranus (astro[6])
8) Neptune (astro[7])
9) Sun (astro[8])
10) Moon (astro[9])

Positioning Instruments
1) Vectors
	- posTruthVector(int arrayChoice), posFSWVector(int arrayChoice)
	- vectorPrefix contains prefixes that correspond with the vectors they position 
		* ie; vectorPrefix[0] positions vectors[0] and fsw[0]
	- "(type of vector) Base" allows positioning of vectors at endpoint rather than the vector's origin
2) Grids
	- Use Grid.cs to position grids (Assets > 42 > Scripts > Grids > Grid.cs)
	- Talk to Eric Stoneking for more info pertaining to consideration of reference frames
3) Axes
	- posNAxis(), posLAxis(), posFAxis(), posBAxis()
	- N(1) = x; N(2) = y; N(3) = z; NC = center
		* same for all axes
	- Right Handed Coordinate System
	- x(1), x(2), x(3) childed to xC, when x is N, L, F, or B 
	- Can position the vectors individually
	- "(type of axis vector) Base" allows positioning of vectors at endpoint 
		* starts with vectors attached to center
	- N(1) = First Point of Aries; N(3) = 'True' North; N(2) = Cross(N(1), N(3))
		* temporarily rotated N axis 23.4 degrees from 
	- Formation Axis needs multiple spacecraft flying in formation to position; position upon acquisition of data indicating multiple spacecrafts exist in scene

4) Astro Labels
	- posAstroLabels(string prefix, int arrayChoice)
	- All active when Astro Label Button pressed
	- Need planetary positions to position

5) ProxOps
	- Need something in proximity of spacecraft to orient














