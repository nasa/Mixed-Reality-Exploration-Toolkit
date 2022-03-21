using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class complexMeshColoring : MonoBehaviour
{
    public MeshRenderer thisMeshRenderer;
    public Mesh thisMesh;
    public Material thisMeshMaterial;
    

    public Vector3[] vertices;
    public int[] triangles;

    public Color[] originalColors;
    public Color[] newColors;
    public Color[] femapColors;

    // Start is called before the first frame update
    void Start()
    {
        thisMeshRenderer = GetComponent<MeshRenderer>();
        thisMesh = GetComponent<MeshFilter>().mesh;
        thisMeshMaterial = thisMeshRenderer.material;

        vertices = thisMesh.vertices;
        triangles = thisMesh.triangles;


        /* Log all vertex positions
         for (int i = 0; i < vertices.Length; i++)
         {
             Debug.Log("Vertex Coordinate " + i + " :" + thisMesh.vertices[i].ToString("F4"));
         }
        */

        SetRandomColors();
        

        thisMesh.colors = femapColors;
        newColors = thisMesh.colors;

        

    }
    public void SetRandomColors()
    {
        femapColors = new Color[vertices.Length];
        
        for (int i = 0; i <= vertices.Length - 1; i = i + 3)
        {
            femapColors[i] = new Color(0f, 0f, 1f, 0.4f);
            if (i + 1 < vertices.Length)
            {
                femapColors[i + 1] = new Color(0f, 1f, 0f, 0.4f);
            }
            if (i + 2 < vertices.Length)
            {
                femapColors[i + 2] = new Color(1f, 0f, 0f, 0.4f);
            }
        }
    }


}
