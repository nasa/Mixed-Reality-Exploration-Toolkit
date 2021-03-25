// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMinimapHeightmapDisplay : MonoBehaviour
{
    [SerializeField] Material displayMaterial;
    [SerializeField, Range(6, 12)] int resolution;
    [SerializeField] MinimapHeightMapReader reader;
    [SerializeField] Vector2 heightBounds;
    private Texture2D _heightmapTexture;
    private int _size;

    private void Update()
    {
        UpdateTexture();
    }

    // makes the texture closer to white if it is closer to the camera
    void UpdateTexture()
    {
        if (_heightmapTexture == null)
        {
            _size = (int)Mathf.Pow(2, resolution);
            _heightmapTexture = new Texture2D(_size, _size);
            displayMaterial.SetTexture("_DisplaceTex", _heightmapTexture);
        }
        Color32[] colors = new Color32[_size*_size];

        for (int x = 0; x < _size; x++)
        {
            for (int y = 0; y < _size; y++)
            {
                float xfac = (1.0f * x) / (1.0f * _size);
                float yfac = (1.0f * y) / (1.0f * _size);
                float height;
                Color32 color; 

                if (reader.TrySampleHeight(xfac, yfac, out height))
                {
                    float interpolatedHeight = Mathf.InverseLerp(heightBounds.x, heightBounds.y, height);
                    byte value = (byte)(interpolatedHeight * 255);
                    color = new Color32(value, value, value, 255);
                }
                else
                {
                    color = Color.blue;
                }

                colors[x * _size + y] = color;
            
            }
        }
        _heightmapTexture.SetPixels32(colors);
        _heightmapTexture.Apply();
    }
}
