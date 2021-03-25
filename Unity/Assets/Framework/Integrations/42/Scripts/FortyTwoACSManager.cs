// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortyTwoACSObject
{
    public GameObject controlledObject;
    public string objectID;
}

public class FortyTwoACSManager : MonoBehaviour
{
    public List<FortyTwoACSObject> trackedObjects;
    public string configurationDirectory;
    
	void Start()
    {
		
	}
}