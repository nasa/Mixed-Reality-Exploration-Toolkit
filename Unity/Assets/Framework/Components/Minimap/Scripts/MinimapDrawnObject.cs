using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapDrawnObject : MonoBehaviour
{
    public Sprite minimapSprite;

    private void OnEnable()
    {
    MinimapController.RegisterMapObject(this);
    }
    
    private void OnDisable()
    {
        MinimapController.DeRegisterMapObject(this);
    }
}
