// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class TileListManager : MonoBehaviour
{
    public Text titleText;
    public GameObject viewport;
    public GameObject tileListItemPrefab;
    public GameObject contentHolder;
    public int numColumns = 3;
    public float xStartingPos = 120, yStartingPos = -110;
    public float horizontalDistanceBetweenEntries = 160, verticalDistanceBetweenEntries = -210;
    public int numItems
    {
        get
        {
            return tileListButtons.Count;
        }
    }

    private List<Button> tileListButtons = new List<Button>();
    private int selectedButtonIndex = -1;
    private ColorBlock selectedButtonColorBlock;
    private float originalXStartingPos, originalYStartingPos;
    private bool originalSet = false, viewportSet = false;
    private int columnCount = 0;
    private Vector3 originalViewportPos;

    void Start()
    {
        if (viewport)
        {
            originalViewportPos = viewport.transform.localPosition;
            viewportSet = true;
        }
    }

    public void SetTitle(string title)
    {
        titleText.text = title;
    }

    public void AddTileListItem(string label)
    {
        GameObject newTileListItem = Instantiate(tileListItemPrefab);
        newTileListItem.transform.SetParent(contentHolder.transform);
        newTileListItem.transform.localScale = Vector3.one;
        newTileListItem.transform.localRotation = Quaternion.identity;
        newTileListItem.transform.localPosition = new Vector3(xStartingPos, yStartingPos, 0);

        if (!originalSet)
        {
            originalXStartingPos = xStartingPos;
            originalYStartingPos = yStartingPos;
            originalSet = true;
        }
        xStartingPos += horizontalDistanceBetweenEntries;
        if (columnCount++ >= numColumns - 1)
        {
            xStartingPos = originalXStartingPos;
            yStartingPos += verticalDistanceBetweenEntries;
            columnCount = 0;
        }

        Text textObject = newTileListItem.GetComponentInChildren<Text>();
        if (textObject)
        {
            textObject.text = label;
        }

        Button buttonObject = newTileListItem.GetComponentInChildren<Button>();
        tileListButtons.Add(buttonObject);
    }

    public void AddTileListItem(string label, UnityEvent onClickEvent)
    {
        GameObject newTileListItem = Instantiate(tileListItemPrefab);
        newTileListItem.transform.SetParent(contentHolder.transform);
        newTileListItem.transform.localScale = Vector3.one;
        newTileListItem.transform.localRotation = Quaternion.identity;
        newTileListItem.transform.localPosition = new Vector3(xStartingPos, yStartingPos, 0);

        if (!originalSet)
        {
            originalXStartingPos = xStartingPos;
            originalYStartingPos = yStartingPos;
            originalSet = true;
        }
        xStartingPos += horizontalDistanceBetweenEntries;
        if (columnCount++ >= numColumns - 1)
        {
            xStartingPos = originalXStartingPos;
            yStartingPos += verticalDistanceBetweenEntries;
            columnCount = 0;
        }

        Text textObject = newTileListItem.GetComponentInChildren<Text>();
        if (textObject)
        {
            textObject.text = label;
        }

        Button buttonObject = newTileListItem.GetComponentInChildren<Button>();
        if (buttonObject)
        {
            buttonObject.onClick.AddListener(() =>
            {
                onClickEvent.Invoke();
            });
        }
        tileListButtons.Add(buttonObject);
    }

    public void AddTileListItem(string label, Texture2D thumbnail, UnityEvent onClickEvent)
    {
        GameObject newTileListItem = Instantiate(tileListItemPrefab);
        newTileListItem.transform.SetParent(contentHolder.transform);
        newTileListItem.transform.localScale = Vector3.one;
        newTileListItem.transform.localRotation = Quaternion.identity;
        newTileListItem.transform.localPosition = new Vector3(xStartingPos, yStartingPos, 0);

        if (!originalSet)
        {
            originalXStartingPos = xStartingPos;
            originalYStartingPos = yStartingPos;
            originalSet = true;
        }
        xStartingPos += horizontalDistanceBetweenEntries;
        if (columnCount++ >= numColumns - 1)
        {
            xStartingPos = originalXStartingPos;
            yStartingPos += verticalDistanceBetweenEntries;
            columnCount = 0;
        }

        Text textObject = newTileListItem.GetComponentInChildren<Text>();
        if (textObject)
        {
            textObject.text = label;
        }

        Image imageObject = newTileListItem.transform.Find("Thumbnail").GetComponent<Image>();
        if (imageObject && thumbnail)
        {
            imageObject.sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f));
        }

        Button buttonObject = newTileListItem.GetComponentInChildren<Button>();
        if (buttonObject)
        {
            buttonObject.onClick.AddListener(() =>
            {
                onClickEvent.Invoke();
            });
        }
        tileListButtons.Add(buttonObject);
    }

    public void ClearTileList()
    {
        foreach (Button scrollItem in tileListButtons)
        {
            Destroy(scrollItem.gameObject);
        }

        if (originalSet)
        {
            xStartingPos = originalXStartingPos;
            yStartingPos = originalYStartingPos;
            columnCount = 0;
        }

        if (viewportSet)
        {
            ResetPosition();
        }
        tileListButtons = new List<Button>();

        selectedButtonIndex = -1;
    }

    public void ResetPosition()
    {
        if (viewport)
        {
            viewport.transform.localPosition = originalViewportPos;
        }
    }

    public void HighlightItem(int itemIndex)
    {
        if (tileListButtons[itemIndex])
        {
            // Revert previous button.
            if (selectedButtonIndex != -1)
            {
                tileListButtons[selectedButtonIndex].colors = selectedButtonColorBlock;
                selectedButtonIndex = -1;
            }

            // Select button.
            selectedButtonIndex = itemIndex;
            selectedButtonColorBlock = tileListButtons[itemIndex].colors;
            ColorBlock tempColors = tileListButtons[itemIndex].colors;
            tempColors.normalColor = tileListButtons[itemIndex].colors.pressedColor;
            tileListButtons[itemIndex].colors = tempColors;
        }
    }

    public void UpdateTileListItemLabel(int itemIndex, string newLabel)
    {
        if (tileListButtons[itemIndex])
        {
            Text textObject = tileListButtons[itemIndex].GetComponentInChildren<Text>();
            if (textObject)
            {
                textObject.text = newLabel;
            }
        }
    }
}