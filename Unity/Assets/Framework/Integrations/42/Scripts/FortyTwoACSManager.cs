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