// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;

namespace GSFC.ARVR.MRET.Components.Camera
{
    public class ImageCameraCaptureManager : MonoBehaviour
    {
        public GameObject cameraFlash;
        public RenderTexture imageTexture;

        private string lastPathWritten = "";
        private int lastPathWrittenIndex = 0;

        public void CaptureScreenshot()
        {
            // TODO: Better way.
            if (gameObject.activeSelf == false)
            {
                return;
            }

            System.DateTime now = System.DateTime.Now;
            string imgPath = Application.dataPath + "/Captures/";

            if (!System.IO.Directory.Exists(imgPath))
            {
                System.IO.Directory.CreateDirectory(imgPath);
            }

            Texture2D tex = new Texture2D(imageTexture.width, imageTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = imageTexture;
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            byte[] bytes = tex.EncodeToPNG();

            string pathToWrite = imgPath + "image" + now.Year + now.DayOfYear + now.Hour + now.Minute + now.Second;
            if (pathToWrite == lastPathWritten)
            {
                pathToWrite = pathToWrite + "-" + lastPathWrittenIndex++;
            }
            else
            {
                lastPathWrittenIndex = 0;
                lastPathWritten = pathToWrite;
            }
            System.IO.File.WriteAllBytes(pathToWrite + ".png", bytes);
            if (gameObject.activeSelf)
            {
                StartCoroutine(FlashCamera());
            }
        }

        private IEnumerator FlashCamera()
        {
            cameraFlash.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            cameraFlash.SetActive(false);
        }
    }
}