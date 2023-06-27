// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Pin;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin
{
    /// <remarks>
    /// History:
    /// 3 October 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// PinMarkerDeprecated
    ///
    /// Manages Individual pins and pin and path relationships.
    /// <Works with cref="PinPanelController"/>
    ///
    /// Author: Sean Letavish
    /// </summary>
    /// 
    public class PinMarkerDeprecated : MRETUpdateBehaviour
    {
        //Transform for where the pin marker should spawn
        Transform pinSpawn;

        public override string ClassName
        {
            get
            {
                return nameof(PinMarkerDeprecated);
            }
        }
        // Start is called before the first frame update
        /// <summary>
        /// Places this pin as a child of the hands. To be used as the transform that all other pins will spawn at. 
        /// </summary>
        protected override void MRETStart()
        {
            base.MRETStart();

            updateRate = UpdateFrequency.Hz1;

            if (!PinPanelControllerDeprecated.setPinEnabled)
            {
                //Get translucent material
                GetComponent<Renderer>().material = ProjectManager.PinManagerDeprecated.holoMaterial;


                if (MRET.HudManager.leftController.GetComponentInChildren<PinMarkerDeprecated>(true) == null)
                {
                    transform.SetParent(MRET.HudManager.leftController.transform);
                    if (MRET.HudManager.leftController.GetComponentInChildren<PinPanelControllerDeprecated>() != null)
                    {
                        GetComponent<Renderer>().enabled = false;
                    }
                }

                else
                {
                    transform.SetParent(MRET.HudManager.rightController.transform);
                    if (MRET.HudManager.rightController.GetComponentInChildren<PinPanelControllerDeprecated>() != null)
                    {
                        GetComponent<Renderer>().enabled = false;
                    }
                }

                transform.localPosition = new Vector3(0, 0, 0);
                transform.localRotation = new Quaternion(0, 0, 0, 0);
                transform.localScale = transform.localScale * 10;
                PinPanelControllerDeprecated.modelInHand = true;
            }

            else
            {
                pinSpawn = SetPinDeprecated.pinMarkerTransform;

                gameObject.transform.position = pinSpawn.position;
                gameObject.transform.rotation = pinSpawn.rotation;
                GetComponent<Renderer>().material.color = ProjectManager.PinManagerDeprecated.pinAndPathColor;
                InteractablePinDeprecated ipin = gameObject.AddComponent<InteractablePinDeprecated>();
                ProjectManager.PinManagerDeprecated.AddPin(ipin);

            }

        }
    }
}