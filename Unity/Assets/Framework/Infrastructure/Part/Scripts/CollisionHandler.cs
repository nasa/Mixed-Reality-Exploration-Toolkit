using UnityEngine;

// This script handles collision behavior. It should be attached to game
//  objects when they are being picked up, and removed when being put down.
public class CollisionHandler : MonoBehaviour
{
    public GameObject grabbingObj;

    private SessionConfiguration sessionConfig;
    private MeshRenderer[] objectRenderers;
    private Material[] objectMaterials;
    private Material collisionMaterial;
    private AudioClip collisionClip;
    private AudioSource collisionSound;
    private bool isColliding = false, first = true;

    public void ResetTextures()
    {
        int i = 0;
        
        foreach (MeshRenderer rend in objectRenderers)
        {
            rend.material = objectMaterials[i++];
        }
    }

    public void Start()
    {
        objectRenderers = GetComponentsInChildren<MeshRenderer>();
        
        // This preserves the ordering of materials for each mesh renderer.
        objectMaterials = new Material[objectRenderers.Length];

        int i = 0;
        foreach (MeshRenderer rend in objectRenderers)
        {
            objectMaterials[i++] = rend.material;
        }

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
        if (isColliding)
        {
            int strength = Mathf.RoundToInt(Mathf.Lerp(0, 3999, 500));
            VRTK.VRTK_ControllerHaptics.TriggerHapticPulse(VRTK.VRTK_ControllerReference.GetControllerReference(grabbingObj), strength);

            if (!collisionSound.isPlaying && (collisionSound.clip.loadState == AudioDataLoadState.Loaded))
            {
                collisionSound.Play();
            }
        }
    }

    private void LoadCollisionSound()
    {
        collisionSound = gameObject.AddComponent<AudioSource>();
        collisionSound.clip = collisionClip;
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;

        // Change material to collision material.
        foreach (MeshRenderer rend in objectRenderers)
        {
            rend.material = collisionMaterial;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;

        ResetTextures();
    }

    private void OnDestroy()
    {
        Destroy(collisionSound);
    }
}