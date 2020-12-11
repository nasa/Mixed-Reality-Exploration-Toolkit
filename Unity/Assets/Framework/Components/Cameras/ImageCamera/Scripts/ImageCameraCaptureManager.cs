using System.Collections;
using UnityEngine;

public class ImageCameraCaptureManager : MonoBehaviour
{
    public VRTK.VRTK_ControllerEvents leftControllerEvents, rightControllerEvents;
    public GameObject cameraFlash;
    public RenderTexture imageTexture;
    public bool capturingLeft = false, capturingRight = false;

    private string lastPathWritten = "";
    private int lastPathWrittenIndex = 0;

    void Start()
    {
        if (VRDesktopSwitcher.isDesktopEnabled())
        {
            EventManager.OnLeftClick += CaptureScreenshot;
        }
        else
        {
            leftControllerEvents.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(CaptureScreenshot);
            rightControllerEvents.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(CaptureScreenshot);
        }
    }

    public void CaptureScreenshot(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if ((VRDesktopSwitcher.isVREnabled() && (e.controllerReference.hand == VRTK.SDK_BaseController.ControllerHand.Left && capturingLeft) ||
            (e.controllerReference.hand == VRTK.SDK_BaseController.ControllerHand.Right && capturingRight)))
        {
            CaptureScreenshot();
        }
    }

    private void CaptureScreenshot()
    {
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