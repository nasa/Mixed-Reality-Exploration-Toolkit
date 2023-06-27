// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Feed;

namespace GOV.NASA.GSFC.XR.MRET.UI.HUD
{
    public class PanelSwitcher : MRETUpdateBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(PanelSwitcher);

        /// <summary>
        ///  Handles the feed being displayed 
        /// </summary>
        [Tooltip("GameObject that contains the contentImage")]
        public GameObject contentImageGameObject;
        [Tooltip("Image that holds the content that is being displayed")]
        public Texture defaultContentTexture;
        public Text label;
        public Text displayText;
        public TMP_Dropdown sourceDropdownTMP;
        public Dropdown sourceDropdown;
        public TMP_Dropdown miniSourceDropdownTMP;
        public Dropdown miniSourceDropdown;
        [Tooltip("GameObject that contains the HTML display")]
        public GameObject htmlGameObject;
        public DisplayController dispControl;
        public GameObject cameraFlash;
        [HideInInspector]
        public FeedSource feedSource;
        [HideInInspector]
        public bool Initialized = false;

        private RenderTexture renderTexture;
        private FeedSource[] availableSources = { null };
        private float actionTime = 0.0f;
        private float period = 0.25f;

        private string lastPathWritten = "";
        private int lastPathWrittenIndex = 0;

        private bool suppressEvents = false;
#if MRET_EXTENSION_VUPLEX
        private Vuplex.WebView.BaseWebViewPrefab htmlBrowser;
#endif
        private RawImage contentImage;

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            if (htmlGameObject != null)
            {
#if MRET_EXTENSION_VUPLEX
                htmlBrowser = htmlGameObject.GetComponentInChildren<Vuplex.WebView.BaseWebViewPrefab>();
#else
                LogWarning("Vuplex 3D WebView is not installed");
#endif
            }
            else
            {
                LogWarning("HTML GameObject is not defined");
            }

            if (contentImageGameObject != null)
            {
                contentImage = contentImageGameObject.GetComponentInChildren<RawImage>();
            }
            else
            {
                LogWarning("Content Image GameObject is not defined");
            }

            // cameraFlash.SetActive(false);
            if (dispControl.type == DisplayController.Type.miniDisplay)
            {
                sourceDropdown = miniSourceDropdown;
                sourceDropdownTMP = miniSourceDropdownTMP;
            }

            if (!Initialized)
            {
                SetPanelSource(null);
            }
            SetSourceDropdownOptions();
        }

        void OnEnable()
        {
            SetSourceDropdownOptions();
        }

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            if (feedSource != null)
            {
                bool upToDate = feedSource.upToDate;
                if (!upToDate)
                {
                    switch (feedSource.type)
                    {
                        case FeedSource.Type.virtualFeed:
                            break;

                        case FeedSource.Type.externalFeed:
                            // Deprecated: Use HTML for media
                            break;

                        case FeedSource.Type.dataFeed:
                            break;

                        case FeedSource.Type.htmlFeed:
#if MRET_EXTENSION_VUPLEX
                            if (htmlBrowser != null && feedSource.sourceLink != null)
                            {
                                if (feedSource.sourceLink != htmlBrowser.InitialUrl)
                                {
                                    htmlBrowser.InitialUrl = feedSource.sourceLink;
                                    ReloadURL();
                                }
                            }
#endif
                            break;

                        case FeedSource.Type.spriteFeed:
                            switch (feedSource.spriteType)
                            {
                                case FeedSource.SpriteType.toggle:
                                    if (contentImage != null)
                                    {
                                        contentImage.color = feedSource.sprite.color;
                                    }
                                    upToDate = true;
                                    break;

                                case FeedSource.SpriteType.number:

                                    if (UnityEngine.Time.time > actionTime)
                                    {
                                        displayText.text = feedSource.value.text;
                                        actionTime += period;
                                    }

                                    upToDate = true;
                                    break;

                            }
                            break;

                        default:
                            break;
                    }
                }
            }

        }

#if MRET_EXTENSION_VUPLEX
        private async void ReloadURL()
        {
            await htmlBrowser.WaitUntilInitialized();
            htmlBrowser.WebView.LoadUrl(htmlBrowser.InitialUrl);
        }
#endif

        public void SelectPanelSource()
        {
            if (suppressEvents) return;

            if (sourceDropdown.value == 0)
            {
                SetPanelSource(null); // end click
            }
            else
            {
                renderTexture = availableSources[sourceDropdown.value - 1].renderTexture;
                Log("Feed selected: " + availableSources[sourceDropdown.value - 1].description, nameof(SelectPanelSource));
                SetPanelSource(availableSources[sourceDropdown.value - 1]);
            }
        }

        public void SelectPanelSourceTMP()
        {
            if (suppressEvents) return;

            if (sourceDropdownTMP.value == 0)
            {
                SetPanelSource(null); // end click
            }
            else
            {
                renderTexture = availableSources[sourceDropdownTMP.value - 1].renderTexture;
                Log("Feed selected " + availableSources[sourceDropdownTMP.value - 1].description, nameof(SelectPanelSourceTMP));
                SetPanelSource(availableSources[sourceDropdownTMP.value - 1]);
            }
        }

        public void SelectPanelSource(Dropdown dropdown)
        {
            if (suppressEvents) return;

            if (dropdown.value == 0)
            {
                SetPanelSource(null); // end click
            }
            else
            {
                renderTexture = availableSources[dropdown.value - 1].renderTexture;
                Log("Feed selected: " + availableSources[dropdown.value - 1].description, nameof(SelectPanelSource));
                SetPanelSource(availableSources[dropdown.value - 1]);
            }
        }

        public void SelectPanelSourceTMP(TMP_Dropdown dropdown)
        {
            if (suppressEvents) return;

            if (dropdown.value == 0)
            {
                SetPanelSource(null); // end click
            }
            else
            {
                renderTexture = availableSources[dropdown.value - 1].renderTexture;
                Log("Feed selected: " + availableSources[dropdown.value - 1].description, nameof(SelectPanelSourceTMP));
                SetPanelSource(availableSources[dropdown.value - 1]);
            }
        }

        public void SetPanelSource(FeedSource source)
        {
            feedSource = source;
            Initialized = true;

            if (source == null)
            {
                htmlGameObject.SetActive(false);
                contentImageGameObject.SetActive(true);
                label.text = "Feed Disconnected";
                if (contentImage != null)
                {
                    contentImage.texture = defaultContentTexture;
                }
            }
            else
            {
                label.text = source.description;

                switch (source.type)
                {
                    case FeedSource.Type.virtualFeed:
                        if (dispControl.size) dispControl.size.interactable = true;
                        htmlGameObject.SetActive(false);
                        contentImageGameObject.SetActive(true);
                        if (contentImage != null)
                        {
                            contentImage.texture = source.renderTexture;
                        }
                        break;

                    case FeedSource.Type.externalFeed:
                        if (dispControl.size) dispControl.size.interactable = true;
                        htmlGameObject.SetActive(false);
                        contentImageGameObject.SetActive(true);
                        break;

                    case FeedSource.Type.dataFeed:
                        if (dispControl.size) dispControl.size.interactable = true;
                        htmlGameObject.SetActive(false);
                        contentImageGameObject.SetActive(true);
                        break;

                    case FeedSource.Type.htmlFeed:
                        if (dispControl.size) dispControl.size.interactable = false;
                        htmlGameObject.SetActive(true);
                        contentImageGameObject.SetActive(false);
#if MRET_EXTENSION_VUPLEX
                        htmlBrowser.InitialUrl = source.sourceLink;
                        ReloadURL();
#endif
                        break;

                    case FeedSource.Type.spriteFeed:
                        if (dispControl.size) dispControl.size.interactable = true;
                        htmlGameObject.SetActive(false);
                        contentImageGameObject.SetActive(true);

                        switch (source.spriteType)
                        {
                            case FeedSource.SpriteType.toggle:
                                contentImageGameObject.SetActive(true);
                                displayText.gameObject.SetActive(false);
                                if (contentImage != null)
                                {
                                    contentImage.texture = source.sprite.mainTexture;
                                    contentImage.color = source.sprite.color;
                                }
                                break;
                            case FeedSource.SpriteType.number:
                                contentImageGameObject.SetActive(false);
                                displayText.gameObject.SetActive(true);
                                displayText.text = feedSource.value.text;
                                break;
                        }
                        break;

                    default:
                        if (dispControl.size) dispControl.size.interactable = true;
                        htmlGameObject.SetActive(false);
                        contentImageGameObject.SetActive(true);
                        LogWarning("Unable to decipher video source type.", nameof(SetPanelSource));
                        break;

                }
                if (dispControl.feedSettings) dispControl.feedSettings.SetActive(false);
                if (dispControl.miniFeedSettings) dispControl.miniFeedSettings.SetActive(false);
                if (dispControl.feedSettingsDimmed) dispControl.feedSettingsDimmed = true;
            }

            // Update options for next time.
            SetSourceDropdownOptions();
        }

        public void SetSourceDropdownOptions()
        {
            List<string> dropdownLabels = new List<string>();
            dropdownLabels.Add("None");

            availableSources = GetVideoSources();
            int currentSourceIndex = dropdownLabels.Count - 1;
            foreach (FeedSource source in availableSources)
            {
                dropdownLabels.Add(source.description);
                if ((feedSource != null) && (feedSource.description.Equals(source.description)))
                {
                    currentSourceIndex = dropdownLabels.Count - 1;
                }
            }
            
            // Set the correct dropdown index
            if (sourceDropdown != null)
            {
                sourceDropdown.ClearOptions();
                sourceDropdown.AddOptions(dropdownLabels);
                suppressEvents = true;
                sourceDropdown.value = currentSourceIndex;
                suppressEvents = false;
            }

            if (sourceDropdownTMP != null)
            {
                sourceDropdownTMP.ClearOptions();
                sourceDropdownTMP.AddOptions(dropdownLabels);
                suppressEvents = true;
                sourceDropdownTMP.value = currentSourceIndex;
                suppressEvents = false;
            }

        }

        private FeedSource[] GetVideoSources()
        {
            return MRET.UuidRegistry.RegisteredTypes<FeedSource>();
        }

        private int resizeScalex = 2;
        private int resizeScaley = 2;

        public void Screenshot()
        {
            System.DateTime now = System.DateTime.Now;
            string imgPath = Application.dataPath + "/Captures/";

            if (!System.IO.Directory.Exists(imgPath))
            {
                System.IO.Directory.CreateDirectory(imgPath);
            }
            Texture2D tex = new Texture2D(renderTexture.width * resizeScalex, renderTexture.height * resizeScaley, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;

            for (int x = 0; x < renderTexture.width; x++)
            {
                for (int y = 0; y < renderTexture.height; y++)
                {
                    for (int i = 0; i < resizeScalex; i++)
                    {
                        for (int j = 0; j < resizeScaley; j++)
                        {
                            tex.ReadPixels(new Rect(x, y, 1, 1), x * resizeScalex + i, y * resizeScaley + j);
                        }
                    }
                }
            }
            tex.Apply();
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

            //StartCoroutine(FlashCamera());
        }

        private IEnumerable FlashCamera()
        {
            if (cameraFlash != null) cameraFlash.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            if (cameraFlash != null) cameraFlash.SetActive(false);
        }

    }
}