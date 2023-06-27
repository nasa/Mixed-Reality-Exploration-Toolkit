// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Tool
{
    // This script handles collision behavior. It should be attached to game
    // objects when they are being picked up, and removed when being put down.
    public class ToolPassCollisionHandler : MonoBehaviour
    {
        public GameObject grabbingObj;

        private MeshRenderer[] objectRenderers;
        private Material[] objectMaterials;
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

            LoadCollisionSound();
        }

        public void Update()
        {
            if (isColliding && collisionTimer < 20)
            {
                int strength = Mathf.RoundToInt(Mathf.Lerp(0, 3999, 500));
                // TODO.
                //VRTK.VRTK_ControllerHaptics.TriggerHapticPulse(VRTK.VRTK_ControllerReference.GetControllerReference(grabbingObj), strength);

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
            collisionSound.clip = MRET.CollisionSound;
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
                    rendMats[j] = MRET.CollisionMaterial;
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
}