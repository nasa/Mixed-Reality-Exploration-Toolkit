using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VDE.UI
{
    internal class CameraCollider : MonoBehaviour
    {
        internal float radius = 1.5F;
        CameraController cameraController;
        internal SphereCollider collie;
        private List<GameObject> listOfObjectsThisCameraIsCurrentlyIn = new List<GameObject> { };
        private void Start()
        {
            TryGetComponent(out cameraController);
            if(!TryGetComponent(out collie))
            {
                collie = gameObject.AddComponent<SphereCollider>();
                collie.radius = radius;
                collie.isTrigger = true;
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("nodeGroup") || other.gameObject.CompareTag("node"))
            {
                listOfObjectsThisCameraIsCurrentlyIn.Add(other.gameObject);
                if (!(cameraController is null) && listOfObjectsThisCameraIsCurrentlyIn.Count > 0)
                {
                    cameraController.currentBooster = cameraController.defaultBooster / 2;
                }
                if (other.gameObject.TryGetComponent(out Shape otherShape))
                {
                    if (other.gameObject.CompareTag("node"))
                    {
                        otherShape.data.UI.ShapeIsInView(otherShape);
                    }
                    else
                    {
                        otherShape.data.UI.ShapeIsInView(otherShape);
                        otherShape.SetVisibility(false);
                    }
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("nodeGroup") || other.gameObject.CompareTag("node"))
            {
                listOfObjectsThisCameraIsCurrentlyIn.Remove(other.gameObject);
                if (!(cameraController is null) && listOfObjectsThisCameraIsCurrentlyIn.Count == 0)
                {
                    cameraController.currentBooster = cameraController.defaultBooster;
                }
                if (other.gameObject.TryGetComponent(out Shape otherShape))
                {
                    if (other.gameObject.CompareTag("node"))
                    {
                        otherShape.data.UI.ShapeIsNotInView(otherShape);
                    }
                    else
                    {
                        otherShape.data.UI.ShapeIsNotInView(otherShape);
                        otherShape.SetVisibility(true);
                    }
                }
            }
        }
    }
}
