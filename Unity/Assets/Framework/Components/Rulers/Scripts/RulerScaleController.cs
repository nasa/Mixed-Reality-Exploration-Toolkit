using UnityEngine;
using GSFC.ARVR.MRET.Common.Schemas;

public class RulerScaleController : MonoBehaviour
{
    public GameObject siRuler, imperialRuler;
    public UnityProject unityProject;

    private int updateDivider = 0;
	void Update ()
    {
		if (updateDivider++ > 16)
        {
            siRuler.transform.localScale = new Vector3(unityProject.scaleMultiplier, unityProject.scaleMultiplier, unityProject.scaleMultiplier);
            imperialRuler.transform.localScale = new Vector3(unityProject.scaleMultiplier, unityProject.scaleMultiplier, unityProject.scaleMultiplier);
            updateDivider = 0;
        }
	}
}