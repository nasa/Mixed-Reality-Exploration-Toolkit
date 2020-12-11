using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FortyTwoMenuController : MonoBehaviour
{
    public DataManager dataManager;
    public List<GameObject> vectors;
    public List<GameObject> grids;
    public List<GameObject> axes;
    public List<GameObject> fsw;
    public List<GameObject> astro;
    public List<GameObject> buttons;
    public GameObject astroButton;
   public List<string> vectorPrefix = new List<string>()
    {
        "SC.0.SVB",
        "SC.0.BVB",
        "SC.0.HVB"
    };
    public Color origin, pressed;
    public GameObject menuContent, axis, ionCruiser, proxOps, ionCruiserPos;
    public Vector3 pos, tPos, fswPos;

    void Start()
    {
        menuContent.SetActive(false);
        StartCoroutine(hello());

        // set instrument as inactive at start
        for(int v = 0; v < vectors.Count; v++)
        {
            vectors[v].SetActive(false);
        }
        for(int g = 0; g < grids.Count; g++)
        {
            grids[g].SetActive(false);
        }
        for(int a = 0; a < axes.Count; a++)
        {
            axes[a].SetActive(false);
        }
        for(int l = 0; l < astro.Count; l++)
        {
            astro[l].SetActive(false);
        }
        proxOps.SetActive(false);
        origin = buttons[0].GetComponent<Image>().color;
        
        dataManager = FindObjectOfType<DataManager>();

        
    }

   void Update()
    {
        Input.location.Start(); 

        if (dataManager != null) // if DataManager script is active
        {
            if (axes[0].active) // check if instrument is active
                posNAxis(); //positions active instrument

            if (axes[1].active)
                posLAxis();

            if (axes[2].active)
                posFAxis();

            if (axes[3].active)
                posBAxis();

            if(vectors[0].active)
                posTruthVector(0);

            if (vectors[1].active)
                posTruthVector(1);

            if (vectors[2].active)
                posTruthVector(2);

            if (fsw[0].active)
                posFSWVector(0);

            if (fsw[1].active)
                posFSWVector(1);

            if(astroButton.GetComponent<Image>().color == pressed) 
            {
                for (int a = 0; a < astro.Count; a++) // temporary positioning of astro labels
                {
                    if (a == 8)
                    {
                        posAstroLabels("SC.0.SVB", 0);
                    }
                    if (a == 9)
                    {
                        posAstroLabels("SC.0.LUNA_POS_N", 1);
                    }
                    if (a == 2)
                    {
                        posAstroLabels("SC.0.EARTH_POS_H", 2);
                    }
                }
            }
            
        }

     }

    IEnumerator hello()
    {
        yield return new WaitForSeconds(3);
        menuContent.SetActive(true);
    }
    
    
    public void colorButton(int choice)
    {
 
        if (choice < 16)
        {
            if (buttons[choice].GetComponent<Image>().color == pressed)
            {
                buttons[choice].GetComponent<Image>().color = origin;
            }
            else
            {
                buttons[choice].GetComponent<Image>().color = pressed;
            }
        }
    }
   
    public void setVectorActive(int choice)            
    {
        
        if (vectors[choice].active)
            vectors[choice].SetActive(false);
        else
            vectors[choice].SetActive(true);
      

    }
    public void setAstroActive()
    {
        for(int vec = 0; vec < astro.Count; vec++)
        {
            if (astro[vec].active)
                astro[vec].SetActive(false);
            else
                astro[vec].SetActive(true);
        }
    }
    public void setFSWActive(int choice)
    {
        if(fsw[choice].active)
            fsw[choice].SetActive(false);
        else
        {
            fsw[choice].SetActive(true);
            Debug.Log("vector set active");
        }

    }
    public void setGridActive(int choice)
    {
        if (grids[choice].active)
            grids[choice].SetActive(false);
        else
            grids[choice].SetActive(true);
        
    }
    public void setAxesActive(int choice)
    {
        if(axes[choice].active)
            axes[choice].SetActive(false);
        else
            axes[choice].SetActive(true);

    }
    public void setProxOpsActive()
    {
        if (proxOps.active)
            proxOps.SetActive(false);
        else
            proxOps.SetActive(true);
    }
    public void setMenuOff()
    {
        menuContent.SetActive(false);
    }

    public void posTruthVector(int pick)
     {
        Vector3 ionCruiserPos = GameObject.Find("IonCruiser").transform.position;
        vectors[pick].transform.position = ionCruiserPos; // place vector base on spacecraft
       
        var x = dataManager.FindPoint(vectorPrefix[pick] + ".X");
        float finX = Convert.ToSingle(x);

        var y = dataManager.FindPoint(vectorPrefix[pick] + ".Y");
        float finY = Convert.ToSingle(y);

        var z = dataManager.FindPoint(vectorPrefix[pick] + ".Z");
        float finZ = Convert.ToSingle(z);

        vectors[pick].transform.LookAt(new Vector3(finX, finY, finZ)); 
                  
     } 
    public void posFSWVector(int pick)
    {
        Vector3 ionCruiserPos = GameObject.Find("IonCruiser").transform.position;
        fsw[pick].transform.position = ionCruiserPos;
        
        var x = dataManager.FindPoint(vectorPrefix[pick] + ".X");
        float finX = Convert.ToSingle(x);

        var y = dataManager.FindPoint(vectorPrefix[pick] + ".Y");
        float finY = Convert.ToSingle(y);

        var z = dataManager.FindPoint(vectorPrefix[pick] + ".Z");
        float finZ = Convert.ToSingle(z);

        fsw[pick].transform.LookAt(new Vector3(finX, finY, finZ));
        
    }
    
    public void posNAxis()
    {
        GameObject spacecraft = GameObject.Find("IonCruiser");
        GameObject.Find("N1 Base").transform.position = spacecraft.transform.position;
        GameObject.Find("N1 Base").transform.LookAt(GameObject.Find("Aries Point").transform.position); // point X at vernal equinox point

        GameObject.Find("N3 Base").transform.position = spacecraft.transform.position;
        GameObject.Find("N3 Base").transform.LookAt(new Vector3(0, -Input.compass.trueHeading, 0)); // point z North

        Vector3 cross = Vector3.Cross(GameObject.Find("N1 Base").transform.position, GameObject.Find("N3 Base").transform.position); // place y perpendicular
        GameObject.Find("N2 Base").transform.LookAt(cross);

        GameObject.Find("NC").transform.position = spacecraft.transform.position;
        
        
    }

    public void posLAxis()
    {

        GameObject ionCruiser = GameObject.Find("IonCruiser");

        Vector3 rotation = ionCruiser.transform.eulerAngles;

        GameObject.Find("LC").transform.position = ionCruiser.transform.position;
        Debug.Log("ioncruiser position: " + ionCruiser.transform.position);

        GameObject.Find("LC").transform.eulerAngles = rotation;
        Debug.Log("placed L axes on ioncruiser position: " + ionCruiser.transform.position);

        object rPosX = dataManager.FindPoint("SC.0.POS_R.X");
        float posFinX = Convert.ToSingle(rPosX);

        object rPosY = dataManager.FindPoint("SC.0.POS_R.Y");
        float posFinY = Convert.ToSingle(rPosY);

        object rPosZ = dataManager.FindPoint("SC.0.POS_R.Z");
        float posFinZ = Convert.ToSingle(rPosZ);

        Vector3 rPos = new Vector3(posFinX, posFinY, posFinZ);

        object rVelX = dataManager.FindPoint("SC.0.VEL_R.X");
        float velFinX = Convert.ToSingle(rVelX);

        object rVelY = dataManager.FindPoint("SC.0.VEL_R.Y");
        float velFinY = Convert.ToSingle(rVelY);

        object rVelZ = dataManager.FindPoint("SC.0.VEL_R.Z");
        float velFinZ = Convert.ToSingle(rVelZ);

        Vector3 vVel = new Vector3(posFinX, velFinY, velFinZ);

        Vector3 H = Vector3.Cross(rPos, vVel);
        Vector3 z = (-1 * rPos) / rPos.magnitude;
        Vector3 y =  (- 1 * H)/ H.magnitude;
        Vector3 x = Vector3.Cross(y, z);

        GameObject.Find("L1").transform.LookAt(x);
        Debug.Log("L1 looking at " + x);
        GameObject.Find("L2").transform.LookAt(y);
        Debug.Log("L2 looking at " + y);
        GameObject.Find("L3").transform.LookAt(z);
        Debug.Log("L3 looking at " + z);

    }

    public void posFAxis()
    { // temporary until other spacecrafts exist to orient this frame
        GameObject ionCruiser = GameObject.Find("IonCruiser");
        Vector3 rotation = ionCruiser.transform.eulerAngles;

        GameObject.Find("FC").transform.position = GameObject.Find("IonCruiser").transform.position;

        GameObject.Find("FC").transform.eulerAngles = rotation;

    }

    public void posBAxis()
    { 
        GameObject ionCruiser = GameObject.Find("IonCruiser");
        Vector3 rotation = ionCruiser.transform.eulerAngles;

       GameObject.Find("BC").transform.position = GameObject.Find("IonCruiser").transform.position;

        GameObject.Find("BC").transform.eulerAngles = rotation;
    }

    public void posAstroLabels(string prefix, int choice)
    {
        astro[choice].transform.position = GameObject.Find("IonCruiser").transform.position;
        
        object posX = dataManager.FindPoint(prefix + ".X");
            float finX = Convert.ToSingle(posX);

        object posY = dataManager.FindPoint(prefix + ".Y");
            float finY = Convert.ToSingle(posY);


        object posZ = dataManager.FindPoint(prefix + ".Z");
            float finZ = Convert.ToSingle(posZ);
        
        Vector3 pos = new Vector3(finX, finY, finZ);

        astro[choice].transform.LookAt(pos);
    }
 

}
