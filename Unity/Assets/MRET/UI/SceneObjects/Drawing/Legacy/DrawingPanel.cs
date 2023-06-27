// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing.Legacy;

namespace GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing.Legacy
{
    // TODO: Is this class used?
    public class DrawingPanel : MonoBehaviour
    {
        public Text typeField;
        public Dropdown unit;
        public Dropdown drawingType;
        public Text width;
        public Material drawing;
        public Material cabling;
        public Material measurement;
        public GameObject menuPrefab;

        public void submitButtonPressed()
        {
            changeType();
            changeWidth();
        }

        public void changeType()
        {

            MeshLineRenderer mlr = gameObject.transform.parent.GetComponent<MeshLineRenderer>();

            switch (drawingType.value)
            {
                case 0:
                default:
                    mlr.drawingScript.Material = drawing;
                    mlr.drawingScript.SetRenderType(DrawingRender3dType.Basic);
                    break;
                case 1:
                    mlr.drawingScript.Material = cabling;
                    mlr.drawingScript.SetRenderType(DrawingRender3dType.Volumetric);
                    break;
            }

        }

        public void changeWidth()
        {
            MeshLineRenderer mlr = gameObject.transform.parent.GetComponent<MeshLineRenderer>();
            if (!string.IsNullOrEmpty(width.text) && float.TryParse(width.text, out float newWidth))
            {
                mlr.drawingScript.width = newWidth;
            }
        }

        public void loadMenu(Transform par, Quaternion rot, Vector3 pos)
        {
            GameObject menu = Instantiate(menuPrefab, pos, rot, par);
            menu.transform.rotation =
                    Quaternion.LookRotation((MRET.InputRig.head.transform.position
                    - menu.transform.position) * -1, Vector3.up);

        }

        public void closeMenu(GameObject menu)
        {
            Destroy(menu);
        }
    }
}