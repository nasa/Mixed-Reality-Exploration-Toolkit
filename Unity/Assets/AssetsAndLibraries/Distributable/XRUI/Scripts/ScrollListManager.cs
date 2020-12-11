using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScrollListManager : MonoBehaviour
{
    public Text titleText;
    public GameObject viewport;
    public GameObject scrollListItemPrefab;
    public GameObject contentHolder;
    public float startingPos = -36;
    public float horizontalPos = 181;
    public float distanceBetweenEntries = -71;
    public int numItems
    {
        get
        {
            return scrollListButtons.Count;
        }
    }

    private List<Button> scrollListButtons = new List<Button>();
    private int selectedButtonIndex = -1;
    private ColorBlock selectedButtonColorBlock;
    private float originalStartingPos;
    private bool originalSet = false, viewportSet = false;
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

    public void AddScrollListItem(string label)
    {
        GameObject newScrollListItem = Instantiate(scrollListItemPrefab);
        newScrollListItem.transform.SetParent(contentHolder.transform);
        newScrollListItem.transform.localScale = Vector3.one;
        newScrollListItem.transform.localRotation = Quaternion.identity;
        newScrollListItem.transform.localPosition = new Vector3(horizontalPos, startingPos, 0);

        if (!originalSet)
        {
            originalStartingPos = startingPos;
            originalSet = true;
        }
        startingPos += distanceBetweenEntries;

        Text textObject = newScrollListItem.GetComponentInChildren<Text>();
        if (textObject)
        {
            textObject.text = label;
        }

        Button buttonObject = newScrollListItem.GetComponentInChildren<Button>();
        scrollListButtons.Add(buttonObject);
    }

    public void AddScrollListItem(string label, UnityEvent onClickEvent)
    {
        GameObject newScrollListItem = Instantiate(scrollListItemPrefab);
        newScrollListItem.transform.SetParent(contentHolder.transform);
        newScrollListItem.transform.localScale = Vector3.one;
        newScrollListItem.transform.localRotation = Quaternion.identity;
        newScrollListItem.transform.localPosition = new Vector3(horizontalPos, startingPos, 0);

        if (!originalSet)
        {
            originalStartingPos = startingPos;
            originalSet = true;
        }
        startingPos += distanceBetweenEntries;

        Text textObject = newScrollListItem.GetComponentInChildren<Text>();
        if (textObject)
        {
            textObject.text = label;
        }

        Button buttonObject = newScrollListItem.GetComponentInChildren<Button>();
        if (buttonObject)
        {
            buttonObject.onClick.AddListener(() =>
            {
                onClickEvent.Invoke();
            });
        }
        scrollListButtons.Add(buttonObject);
    }

    public void AddScrollListItem(string label, Texture2D thumbnail, UnityEvent onClickEvent)
    {
        GameObject newScrollListItem = Instantiate(scrollListItemPrefab);
        newScrollListItem.transform.SetParent(contentHolder.transform);
        newScrollListItem.transform.localScale = Vector3.one;
        newScrollListItem.transform.localRotation = Quaternion.identity;
        newScrollListItem.transform.localPosition = new Vector3(horizontalPos, startingPos, 0);

        if (!originalSet)
        {
            originalStartingPos = startingPos;
            originalSet = true;
        }
        startingPos += distanceBetweenEntries;

        Text textObject = newScrollListItem.GetComponentInChildren<Text>();
        if (textObject)
        {
            textObject.text = label;
        }

        Image imageObject = newScrollListItem.transform.Find("Thumbnail").GetComponent<Image>();
        if (imageObject && thumbnail)
        {
            imageObject.sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f));
        }

        Button buttonObject = newScrollListItem.GetComponentInChildren<Button>();
        if (buttonObject)
        {
            buttonObject.onClick.AddListener(() =>
            {
                onClickEvent.Invoke();
            });
        }
        scrollListButtons.Add(buttonObject);
    }

    public void ClearScrollList()
    {
        foreach (Button scrollItem in scrollListButtons)
        {
            Destroy(scrollItem.gameObject);
        }

        if (originalSet)
        {
            startingPos = originalStartingPos;
        }

        if (viewportSet)
        {
            ResetPosition();
        }
        scrollListButtons = new List<Button>();

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
        if (scrollListButtons[itemIndex])
        {
            // Revert previous button.
            if (selectedButtonIndex != -1)
            {
                scrollListButtons[selectedButtonIndex].colors = selectedButtonColorBlock;
                selectedButtonIndex = -1;
            }

            // Select button.
            selectedButtonIndex = itemIndex;
            selectedButtonColorBlock = scrollListButtons[itemIndex].colors;
            ColorBlock tempColors = scrollListButtons[itemIndex].colors;
            tempColors.normalColor = scrollListButtons[itemIndex].colors.pressedColor;
            scrollListButtons[itemIndex].colors = tempColors;
        }
    }

    public void UpdateScrollListItemLabel(int itemIndex, string newLabel)
    {
        if (scrollListButtons[itemIndex])
        {
            Text textObject = scrollListButtons[itemIndex].GetComponentInChildren<Text>();
            if (textObject)
            {
                textObject.text = newLabel;
            }
        }
    }

    public Button GetScrollListButton(int itemIndex)
    {
        return scrollListButtons[itemIndex];
    }
}