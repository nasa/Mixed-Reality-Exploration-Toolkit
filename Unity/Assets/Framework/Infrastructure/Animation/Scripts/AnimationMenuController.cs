// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GSFC.ARVR.MRET.Common.Schemas;
using GSFC.ARVR.MRET.Components.Keyboard;
using GSFC.ARVR.MRET.Components.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.Animation
{
    public class AnimationMenuController : MonoBehaviour
    {
        public MRETAnimationPlayer ActivePlayer
        {
            get => activePlayer;
            private set
            {
                activePlayer = value;
                if (activePlayer != null)
                {
                    UpdateDropdownSelection(activePlayer.Name);
                    UpdateNameEditor(activePlayer.Name);
                    wrapmode.value = wrapmode.options.FindIndex(option => option.text == activePlayer.WrapMode.ToString());
                    direction.value = direction.options.FindIndex(option => option.text == activePlayer.Direction.ToString());
                    Debug.Log("[AnimationMenuController] ActivePlayer:"+ activePlayer.Name);
                }
            }
        }

        public Image recordImage;
        public Sprite recordActive, recordInactive;
        public Image playPauseImage;
        public Sprite playSprite, pauseSprite;
        public Slider playbackSlider;
        public Dropdown dropdown;
        public Dropdown wrapmode;
        public Dropdown direction;
        public Text durationLabel;
        public GameObject animationNameEditor;
        public GameObject keyboardEditor;
        public bool testAnimation = false;
        public GameObject testTarget;

        private MRETAnimationPlayer activePlayer;
        private MRETAnimationManager animationManager;
        private bool isBeingDragged = false;
        private int sliderDelayTimer = 0;
        private VR_InputField nameEditorScript;
        private KeyboardManager keyboardEditorScript;

        private void OnEnable()
        {
            animationManager = FindObjectOfType<MRETAnimationManager>();
            MRETAnimationManager.ActivePlayerChangeEvent += UpdateSelectedPlayer;
            MRETAnimationManager.PlayerListChangeEvent += UpdateDropdownOptions;
            ActivePlayer = animationManager.ActivePlayer;
            UpdateDropdownOptions();
        }

        void Start()
        {
            ActivePlayer = animationManager.ActivePlayer;
            if (activePlayer == null)
            {
                ActivePlayer = animationManager.NewAnimation();
            }

            UpdateDropdownOptions();
        }

        void Update()
        {
            UpdateViewElements();
        }

        // Unsubscribe to events
        void OnDisable()
        {
            //Debug.Log("[AnimationMenuController] OnDisable");
            MRETAnimationManager.ActivePlayerChangeEvent -= UpdateSelectedPlayer;
            MRETAnimationManager.PlayerListChangeEvent -= UpdateDropdownOptions;
        }

        private void UpdateViewElements()
        {
            if (activePlayer == null) return;

            // Update slider position
            if (activePlayer.Duration == 0)
            {
                playbackSlider.value = 0;
            }
            else if (!isBeingDragged && activePlayer.Duration > 0)
            {
                playbackSlider.value = (float)(activePlayer.CurrentTime
                            / (float)activePlayer.Duration);
            }
            //else if (animationManager.IsRecording)
            //{
            //    playbackSlider.value = 1;
            //}

            // Update Play/Pause icon
            if (activePlayer.State == State.Running)
            {
                playPauseImage.GetComponent<Image>().sprite = pauseSprite;
            }
            else
            {
                playPauseImage.GetComponent<Image>().sprite = playSprite;
            }

            // Update Record icon
            if (animationManager.IsRecording)
            {
                recordImage.GetComponent<Image>().sprite = recordActive;
            }
            else
            {
                recordImage.GetComponent<Image>().sprite = recordInactive;
            }

            if (durationLabel != null)
            {
                durationLabel.text = TimeSpan.FromSeconds(activePlayer.Duration).ToString();
            }
        }

        public void ToggleTest()
        {
            if (testAnimation)
            {
                //animationManager.TestAnimation(testTarget);
                Debug.Log("[AnimationMenuController] ToggleTest:" + testAnimation);
            }
        }

        public void SetAnimation(MRETAnimationGroup animationToSet)
        {
            //Debug.Log("[AnimationMenuController] SetAnimation:" + animationToSet.Name);
            animationManager.AddSelectAnimation(animationToSet);
        }

        public void ToggleRecording()
        {
            if (animationManager.IsRecording)
            {
                animationManager.StopRecord();
            }
            else
            {
                animationManager.Record();
            }
        }

        public void TogglePlayback()
        {
            if (activePlayer.State == State.Running)
            {
                activePlayer.PauseAnimation();
                playPauseImage.GetComponent<Image>().sprite = playSprite;
            }
            else if (activePlayer.State == State.Paused)
            {
                activePlayer.ResumeAnimation();
                playPauseImage.GetComponent<Image>().sprite = pauseSprite;
            }
            else // Stopped state
            {
                activePlayer.StartAnimation();
                playPauseImage.GetComponent<Image>().sprite = pauseSprite;
            }
        }

        public void StepBack()
        {
            activePlayer.StepBack();
        }

        public void StepForward()
        {
            activePlayer.StepForward();
        }

        public void JumpToEnd()
        {
            activePlayer.JumpToEnd();
        }

        public void Rewind()
        {
            activePlayer.Rewind();
        }

        public void StartDragging()
        {
            isBeingDragged = true;
            Debug.Log("[AnimationMenuController] StartDragging:" + isBeingDragged);
        }

        public void StopDragging()
        {
            isBeingDragged = false;
            Debug.Log("[AnimationMenuController] StopDragging:" + isBeingDragged);
        }

        public void NewAnimation()
        {
            animationManager.NewAnimation();
        }

        public void AnimationNameChange(String name)
        {
            Debug.Log("[AnimationMenuController] AnimationNameChange: " + name);
            if (ActivePlayer != null)
            {
                ActivePlayer.Name = name;
                UpdateDropdownOptions();
            }
        }

        public void SliderValueChanged()
        {
            if (isBeingDragged)
            {
                float offsetRatio = playbackSlider.value;

                // Reduce the overhead of dragging the slider by throwing out some of the events
                // except when the slider is near the lower and upper limits.
                if (sliderDelayTimer++ < 2 && offsetRatio > 0.1 && offsetRatio < 0.9)
                {
                    return;
                }
                sliderDelayTimer = 0;

                Debug.Log("[AnimationMenuController] SliderValueChanged: " + offsetRatio);
                activePlayer.JumpToTime((int)(offsetRatio * (float)activePlayer.Duration));
            }
        }

        void DropdownValueChanged(Dropdown dropdown)
        {
            Debug.Log("[AnimationMenuController] DropdownValueChanged Event " + dropdown.options[dropdown.value]);
            MRETAnimationPlayer selected = animationManager.SelectPlayer(dropdown.options[dropdown.value].text);
        }

        public void AnimationListValueChanged()
        {
            Debug.Log("[AnimationMenuController] DropdownValueChanged Event " + dropdown.options[dropdown.value]);
            MRETAnimationPlayer selected = animationManager.SelectPlayer(dropdown.options[dropdown.value].text);
            if (selected != null) ActivePlayer = selected;
        }

        public void WrapmodeValueChanged()
        {
            Debug.Log("[AnimationMenuController] WrapmodeValueChanged Event " + wrapmode.options[wrapmode.value]);
            ActivePlayer.WrapMode = (WrapMode)Enum.Parse(typeof(WrapMode), wrapmode.options[wrapmode.value].text);
        }

        public void DirectionValueChanged()
        {
            Debug.Log("[AnimationMenuController] DirectionValueChanged Event " + direction.options[direction.value]);
            ActivePlayer.Direction = (Direction)Enum.Parse(typeof(Direction), direction.options[direction.value].text);
        }

        void UpdateSelectedPlayer()
        {
            Debug.Log("[AnimationMenuController] [AnimationMenuController] UpdateSelectedAnimation:");
            ActivePlayer = animationManager.ActivePlayer;
        }

        private void UpdateNameEditor(string name)
        {
            if (nameEditorScript == null)
            {
                nameEditorScript = animationNameEditor.GetComponent<VR_InputField>();
            }

            nameEditorScript.SetTextWithoutNotify(name);
            nameEditorScript.SetTextWithoutNotify(name);

            if (keyboardEditorScript == null)
            {
                //keyboardEditorScript = keyboardEditor.GetComponent<KeyboardManager>();
            }

            //keyboardEditorScript.enteredText = "";
        }

        private void UpdateDropdownSelection(String selection)
        {
            int index = dropdown.options.FindIndex(0, (e => e.text.Equals(selection)));
            dropdown.SetValueWithoutNotify(index);
        }

        private void UpdateDropdownOptions()
        {
            List<string> dropOptions = new List<string>();
            foreach (MRETAnimationPlayer anim in animationManager.Players)
            {
                dropOptions.Add(anim.Name);
            }

            //Clear the old options of the Dropdown menu
            dropdown.ClearOptions();
            //Add the current List of animations
            dropdown.AddOptions(dropOptions);
            // Reset selection
            if (activePlayer != null)
            {
                UpdateDropdownSelection(activePlayer.Name);
            }
        }
    }
}