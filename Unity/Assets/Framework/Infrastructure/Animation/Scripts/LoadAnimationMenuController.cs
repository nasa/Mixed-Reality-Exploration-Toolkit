using System.Collections.Generic;
using UnityEngine;

public class LoadAnimationMenuController : MonoBehaviour
{
    public ScrollListManager animationListDisplay;
    public AnimationMenuController animationMenuController;

    private List<AnimationInfo> animations;
    private int currentSelection = -1;

    void Start()
    {
        animationListDisplay.SetTitle("Animation Name");
        PopulateScrollList();
    }

    public void Open()
    {
        if (currentSelection > -1)
        {
            MRETAnimation anim = MRETAnimation.LoadFromXML(animations[currentSelection].animationFile);
            animationMenuController.loopToggle.isOn = anim.loop;
            animationMenuController.autoplayToggle.isOn = anim.autoplay;
            animationMenuController.SetAnimation(anim);
        }

        WorldSpaceMenuManager menuMan = GetComponentInParent<WorldSpaceMenuManager>();
    }

    private void PopulateScrollList()
    {
        animationListDisplay.ClearScrollList();
        ConfigurationManager configManager = FindObjectOfType<ConfigurationManager>();
        if (configManager)
        {
            animations = configManager.animations;
            for (int i = 0; i < animations.Count; i++)
            {
                int indexToSelect = i;
                UnityEngine.Events.UnityEvent clickEvent = new UnityEngine.Events.UnityEvent();
                clickEvent.AddListener(new UnityEngine.Events.UnityAction(() => { SetActiveSelection(indexToSelect); }));
                animationListDisplay.AddScrollListItem(animations[i].name, clickEvent);
            }
        }
    }

    private void SetActiveSelection(int listID)
    {
        currentSelection = listID;
        animationListDisplay.HighlightItem(listID);
    }

    private void OnEnable()
    {
        PopulateScrollList();
    }
}