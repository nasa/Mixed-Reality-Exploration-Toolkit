using UnityEngine;

public class BillboardingScript : MonoBehaviour 
{
	void Update () 
	{
		transform.rotation =  Quaternion.LookRotation(Camera.main.transform.forward);
	}
}