Passive Monitor Shader/Material

	The passive monitor shader takes in a Texture2D and tiles it as a transparent shader over an object's mesh. 
	It works best with seamless (uninterrupted) textures. The tiling variable is a Vector2 that controls the 
	dimensions of the tiled texture as it maps to the mesh. It is recommended that the X and Y values stay the 
	same, otherwise the texture will look "squished". The texture can also be made to move (this is a purely 
	aesthetic effect). The VerticalSpeed and HorizontalSpeed control how fast the tiling moves across the mesh. 
	Lastly, the WireframeToggle turns the texture on and off.
	
Part Status Shader/Material

	The primary purpose of the Part Status shader is to visually indicate whether a MRET part is in nominal, 
	threatened, or critical condition. To do this, it can pulse different colors at different frequencies with 
	different amounts of saturation and glow. All of these parameters can be controlled with the shader's 
	exposed properties. The saturation refers to the intensity of color being rendered and can be referenced 
	in script using

		material.SetInt("_saturation", saturationValue). 
		
	The glow refers to the power of the Fresnel Effect Shader node, commonly known as "rim power" or "edge 
	glow". The power of the glow can be referenced in script using 

		material.SetFloat("_rimPower", rimPowerValue).
	
	The frequency refers to the amount of times per second that the rim glow will bounce between values. 
	The frequency can be referenced in script using 
		material.SetFloat("_frequency", frequencyValue);
		
	The shader also contains exposed booleans that can trigger green, yellow, and red colors that can be 
	referenced in script using	
		material.SetInt("_noLimitsExceeded", 0);
        material.SetInt("_yellowLimitsExceeded", 1);
        material.SetInt("_redLimitsExceeded", 0);
	where 0 indicates a false value and 1 indicates a true value. 
	
Part Status Script

	The part status script can be attached to a game object or referenced to give a user easier control over 
	the exposed properties of the shaders/materials. It contains a reference to the Passive Monitor and Part 
	Status materials. It contains a toggle for the part status material, with additional toggles for preset 
	values of frequency, rim glow, and saturation for the associated part status conditions. These conditions 
	are easily accessible in other scripts through the Nominal(), YellowLimitsExceeded(), and RedLimitsExceeded() 
	methods. There is also a method called PartStatusNull() that sets all values to 0 (which shows a simple white 
	color). This may serve as a visual indicator that something is wrong in the part status decision pipeline. 
	
	The script also contains a toggle for the passive monitor material. The script can currently toggle seamlessly 
	between part status conditions, passive monitoring, and the mesh's original material.
	
	The script currently only works when the material array in the MeshRenderer has two elements. In the future, 
	the script should identify the number of elements in the materials array and adjust them and/or add in the 
	part status and passive monitoring materials in a different way from its current implementation. 