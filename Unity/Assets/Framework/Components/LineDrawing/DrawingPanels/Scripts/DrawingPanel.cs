using UnityEngine;
using UnityEngine.UI;

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

        switch(drawingType.value)
        {
            case 0:
                mlr.drawingScript.renderType = LineDrawing.RenderTypes.Drawing;
                mlr.drawingScript.SetMat(drawing);
                mlr.drawingScript.Rerender();
                break;
            case 1:
                mlr.drawingScript.renderType = LineDrawing.RenderTypes.Cable;
                mlr.drawingScript.SetMat(cabling);
                mlr.drawingScript.Rerender();
                break;
            case 2:
                mlr.drawingScript.renderType = LineDrawing.RenderTypes.Measurement;
                mlr.drawingScript.SetMat(measurement);
                mlr.drawingScript.Rerender();
                break;
        }

    }

    public void changeWidth()
    {
        MeshLineRenderer mlr = gameObject.transform.parent.GetComponent<MeshLineRenderer>();
        if (!string.IsNullOrEmpty(width.text))
        {
            mlr.drawingScript.lineWidth = float.Parse(width.text.ToString());
            mlr.drawingScript.Rerender();
        }
    }

    public void loadMenu(Transform par, Quaternion rot, Vector3 pos)
    {
        GameObject menu = Instantiate(menuPrefab, pos, rot, par);
        menu.transform.rotation =
                Quaternion.LookRotation((VRTK.VRTK_DeviceFinder.HeadsetTransform().position
                - menu.transform.position) * -1, Vector3.up);

    }

    public void closeMenu(GameObject menu)
    {
        Destroy(menu);
    }
}