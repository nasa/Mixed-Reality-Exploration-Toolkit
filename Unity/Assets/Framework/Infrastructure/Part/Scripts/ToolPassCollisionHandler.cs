using UnityEngine;
using System.Collections.Generic;

// This script handles collision behavior. It should be attached to game
//  objects when they are being picked up, and removed when being put down.
public class ToolPassCollisionHandler : MonoBehaviour
{
    public GameObject grabbingObj;

    private SessionConfiguration sessionConfig;
    private MeshRenderer[] objectRenderers;
    private Material[] objectMaterials;
    private Material collisionMaterial;
    private AudioClip collisionClip;
    private AudioSource collisionSound;
    private bool isColliding = false, first = true;
    private int collisionTimer = 0;

    public void ResetTextures()
    {
        int i = 0;

        // Change material to collision material.
        foreach (MeshRenderer rend in objectRenderers)
        {
            int rendMatCount = rend.materials.Length;
            Material[] rendMats = new Material[rendMatCount];
            for (int j = 0; j < rendMatCount; j++)
            {
                rendMats[j] = objectMaterials[i++];
            }
            rend.materials = rendMats;
        }
    }

    public void Start()
    {
        objectRenderers = GetComponentsInChildren<MeshRenderer>();
        
        List<Material> objMatList = new List<Material>();
        foreach (MeshRenderer rend in objectRenderers)
        {
            foreach (Material mat in rend.materials)
            {
                objMatList.Add(mat);
            }
        }
        objectMaterials = objMatList.ToArray();

        /////////////////////////// Get Session Configuration. ///////////////////////////
        GameObject sessionManager = GameObject.Find("SessionManager");
        if (sessionManager)
        {
            sessionConfig = sessionManager.GetComponent<SessionConfiguration>();
            if (sessionConfig)
            {
                collisionMaterial = sessionConfig.collisionMaterial;
                collisionClip = sessionConfig.collisionSound;
            }
            else
            {
                Debug.LogError("[CollisionHandler->Start] Unable to get Session Configuration.");
            }
        }
        else
        {
            Debug.LogError("[CollisionHandler->Start] Unable to get Session Manager.");
        }
        //////////////////////////////////////////////////////////////////////////////////

        LoadCollisionSound();
    }

    public void Update()
    {
        if (isColliding && collisionTimer < 20)
        {
            int strength = Mathf.RoundToInt(Mathf.Lerp(0, 3999, 500));
            VRTK.VRTK_ControllerHaptics.TriggerHapticPulse(VRTK.VRTK_ControllerReference.GetControllerReference(grabbingObj), strength);

            if (!collisionSound.isPlaying && (collisionSound.clip.loadState == AudioDataLoadState.Loaded))
            {
                collisionSound.Play();
            }
            collisionTimer++;
        }
    }

    private void LoadCollisionSound()
    {
        collisionSound = gameObject.AddComponent<AudioSource>();
        collisionSound.clip = collisionClip;
    }

    private void OnTriggerEnter(Collider collision)
    {
        isColliding = true;

        // Change material to collision material.
        foreach (MeshRenderer rend in objectRenderers)
        {
            int rendMatCount = rend.materials.Length;
            Material[] rendMats = new Material[rendMatCount];
            for (int j = 0; j < rendMatCount; j++)
            {
                rendMats[j] = collisionMaterial;
            }
            rend.materials = rendMats;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        collisionTimer = 0;
        isColliding = false;

        ResetTextures();
    }

    private void OnDestroy()
    {
        Destroy(collisionSound);
    }
}