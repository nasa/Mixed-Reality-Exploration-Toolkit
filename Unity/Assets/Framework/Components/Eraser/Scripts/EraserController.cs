// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GSFC.ARVR.MRET.Components.Eraser
{
    // TODO: This needs an overhaul.
    public class EraserController : MonoBehaviour
    {
        public EraserManager eraserManager;

        public bool left;

        private void OnTriggerEnter(Collider other)
        {
            if (left)
            {
                eraserManager.LControllerTouched(other.gameObject);
            }
            else
            {
                eraserManager.RControllerTouched(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (left)
            {
                eraserManager.LControllerUnTouched();
            }
            else
            {
                eraserManager.RControllerUnTouched();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (left)
            {
                eraserManager.LControllerTouched(collision.gameObject);
            }
            else
            {
                eraserManager.RControllerTouched(collision.gameObject);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (left)
            {
                eraserManager.LControllerUnTouched();
            }
            else
            {
                eraserManager.RControllerUnTouched();
            }
        }
    }
}