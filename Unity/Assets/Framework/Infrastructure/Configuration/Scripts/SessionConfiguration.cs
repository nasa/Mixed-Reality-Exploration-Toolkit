using UnityEngine;
using System.Collections.Generic;

public class SessionConfiguration : MonoBehaviour
{
    public static readonly int raycastLayer = 15;
    public static readonly int previewLayer = 20;
    public static readonly int defaultLayer = 0;

    public List<Material> defaultPartMaterials;
    public Material collisionMaterial;
    public Material highlightMaterial;
    public Material selectMaterial;
    public AudioClip collisionSound;
    public GameObject drawingPanelPrefab;
    public GameObject objectPlacementContainer;
    public bool partPanelEnabled = true;

    public static SessionConfiguration instance = null;

    private void Awake()
    {
        instance = this;
    }
}