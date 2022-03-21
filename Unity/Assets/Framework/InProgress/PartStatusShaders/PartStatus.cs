using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartStatus : MonoBehaviour
{

    public MeshRenderer thisMeshRend;

    public Material[] matArray;
    public Material originalMat;
    public Material partStatusMat;
    public Material wireframeMat;

    public bool partStatusOn = false;
    public bool passiveMonitor = false;

    public bool isYellow;
    public bool isRed;

    void Start()
    {
        thisMeshRend = this.GetComponent<MeshRenderer>();
        matArray = thisMeshRend.materials;
        originalMat = matArray[0];
        matArray[1] = partStatusMat;
        thisMeshRend.materials = matArray;
    }

    private void Update()
    {
        if (passiveMonitor == true)
        {
            SetPassiveMonitorMat();
        }

        if (partStatusOn == true)
        {
            SetPartStatusMat();
            DisplayPartStatus();
        }

        if (!passiveMonitor && !partStatusOn)
        {
            ResetMaterial();
        }

    }
    public void SetPartStatusMat()
    {
            passiveMonitor = false;
            matArray[0] = partStatusMat;
            matArray[1] = partStatusMat;
            thisMeshRend.materials = matArray;
    }

    public void SetPassiveMonitorMat()
    {
            partStatusOn = false;
            matArray[0] = wireframeMat;
            matArray[1] = wireframeMat; 
            thisMeshRend.materials = matArray;
    }
    
    public void ResetMaterial()
    {
        matArray[0] = originalMat;
        matArray[1] = originalMat;
        thisMeshRend.materials = matArray;
    }
    public void DisplayPartStatus()
    {
        if (partStatusOn == true)
        {
            if (!isYellow && !isRed)
            {
                Nominal();
            }
            if (isRed == true)
            {
                RedLimitsExceeded();
            }
            if (isYellow == true)
            {
                YellowLimitsExceeded();
            }
        }
        else
        {
            PartStatusNull();
        }
    }
    public void Nominal()
    {
        partStatusMat.SetInt("_noLimitsExceeded", 1);
        partStatusMat.SetInt("_yellowLimitsExceeded", 0);
        partStatusMat.SetInt("_redLimitsExceeded", 0);

        partStatusMat.SetFloat("_saturation", 6.5f);
        partStatusMat.SetFloat("_rimPower", 0.5f);
        partStatusMat.SetFloat("_frequency", 2f);
    }
    public void YellowLimitsExceeded()
    {
        partStatusMat.SetInt("_noLimitsExceeded", 0);
        partStatusMat.SetInt("_yellowLimitsExceeded", 1);
        partStatusMat.SetInt("_redLimitsExceeded", 0);

        partStatusMat.SetFloat("_saturation", 6f);
        partStatusMat.SetFloat("_rimPower", 0.6f);
        partStatusMat.SetFloat("_frequency", 5f);

        isRed = false;
    }
    public void RedLimitsExceeded()
    {
        partStatusMat.SetInt("_noLimitsExceeded", 0);
        partStatusMat.SetInt("_yellowLimitsExceeded", 0);
        partStatusMat.SetInt("_redLimitsExceeded", 1);

        partStatusMat.SetFloat("_saturation", 7.5f);
        partStatusMat.SetFloat("_rimPower", 0.7f);
        partStatusMat.SetFloat("_frequency", 10f);

        isYellow = false;
    }
    public void PartStatusNull()
    {
        partStatusMat.SetInt("_noLimitsExceeded", 0);
        partStatusMat.SetInt("_yellowLimitsExceeded", 0);
        partStatusMat.SetInt("_redLimitsExceeded", 0);

        partStatusMat.SetFloat("_saturation", 0f);
        partStatusMat.SetFloat("_rimPower", 0f);
        partStatusMat.SetFloat("_frequency", 0f);
    }
}
