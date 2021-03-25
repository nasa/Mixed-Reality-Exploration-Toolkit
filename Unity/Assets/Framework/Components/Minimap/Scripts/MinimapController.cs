// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using GSFC.ARVR.MRET.Common;

public class MinimapController : MonoBehaviour
{
    [Tooltip("Position of player/minimap")]
    public Transform playerPos;
    [Tooltip("Minimap camera reference")]
    public Camera minimapCamera;
    public MainHierarchyManager mmHierarchy;
    [Tooltip("Object pooler reference")]
    public ObjectPool objectPool;

    private static List<MinimapDrawnObject> minimapDrawnObjects = new List<MinimapDrawnObject>();
    private List<Image> mapDisplays = new List<Image>();
    private static MinimapController INSTANCE;
    private Dictionary<GameObject, Button> mmoDict = new Dictionary<GameObject, Button>();

    private void Awake()
    {
        INSTANCE = this;
    }

    public Dictionary<GameObject, Button> getMmoDict()
    {
        return mmoDict;
    }

    // add minimap object of interest to minimap UI display
    public static void RegisterMapObject(MinimapDrawnObject o)
    {
        minimapDrawnObjects.Add(o);
    }

    // remove minimap obeject of interest from minimap UI display
    public static void DeRegisterMapObject(MinimapDrawnObject o)
    {
        minimapDrawnObjects.Remove(o);
    }

    void setClickEvent()
    {
        for (int i = 0; i < minimapDrawnObjects.Count; i++)
        {
            MinimapDrawnObject mdo = minimapDrawnObjects[i];
            Image md;

            if (i >= mapDisplays.Count)
            {

                void DeRegisterImage(Image image)
                {
                    mapDisplays.Remove(image);
                    if (image)
                    {
                        image.gameObject.SetActive(false);
                    }
                }

                md = objectPool.Get().GetComponent<Image>();

                mapDisplays.Add(md);
                md.sprite = mdo.minimapSprite;
                MethodDelayer.DelayMethodByPredicateAsync(() => DeRegisterImage(md), () => !minimapDrawnObjects.Contains(mdo));
            }
            else
            {
                md = mapDisplays[i];
            }

            Button mmButton = md.gameObject.GetComponent<Button>();

            UnityEvent clickEvent = new UnityEvent();
            clickEvent.AddListener(new UnityAction(() => { mmHierarchy.mmTeleport(mdo.gameObject); }));
            mmButton.onClick.AddListener(() =>
            {
                clickEvent.Invoke();
            });


            if (!mmoDict.ContainsKey(mdo.gameObject))
                mmoDict.Add(mdo.gameObject, mmButton);
            else
                mmoDict[mdo.gameObject] = mmButton;
        }
    }


    // code to draw minimap icons to the UI
    void DrawMapIcons()
    {
        for (int i = 0; i < minimapDrawnObjects.Count; i++)
        {
            MinimapDrawnObject mdo = minimapDrawnObjects[i];
            Image md = null;

            if (i >= mapDisplays.Count)
            {

                void DeRegisterImage(Image image)
                {
                    mapDisplays.Remove(image);
                    image.gameObject.SetActive(false);
                }

                GameObject poolObj = objectPool.Get();
                if (poolObj)
                {
                    md = poolObj.GetComponent<Image>();
                    if (md)
                    {
                        mapDisplays.Add(md);
                        md.sprite = mdo.minimapSprite;
                        MethodDelayer.DelayMethodByPredicateAsync(() => DeRegisterImage(md), () => !minimapDrawnObjects.Contains(mdo));
                    }
                    else
                    {
                        Debug.LogError("[MinimapController] Unable to get image.");
                    }
                }
                else
                {
                    Debug.LogError("[MinimapController] Unable to get pool object.");
                }
            }
            else
            {
                md = mapDisplays[i];
            }

            Vector2 mop = new Vector2(mdo.transform.position.x, mdo.transform.position.y);
            Vector2 pp = new Vector2(playerPos.position.x, playerPos.position.y);

            if (Vector2.Distance(mop, pp) > 100)
            {
                md.enabled = false;
                continue;
            }
            else
            {
                md.enabled = true;
            }

            Vector3 screenPos = minimapCamera.WorldToScreenPoint(mdo.transform.position);
            md.transform.SetParent(this.transform);

            RectTransform rt = this.GetComponent<RectTransform>();
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            md.rectTransform.localScale = Vector3.one;
            screenPos.x = 100.0f * screenPos.x / minimapCamera.pixelWidth;
            screenPos.y = 100.0f * screenPos.y / minimapCamera.pixelHeight;

            // keep minimap icons enabled when on screen, disable them otherwise
            if ((screenPos.x < 100 && screenPos.x > 0) && (screenPos.y < 100 && screenPos.y > 0))
            {
                md.enabled = true;
            }
            else
            {
                md.enabled = false;
                continue;

            }

            screenPos.z = 0;
            md.rectTransform.anchoredPosition = screenPos;
            md.transform.localPosition = new Vector3(md.transform.localPosition.x, md.transform.localPosition.y, -1);
        }
    }

    void Start()
    {
        mmoDict.Clear();
        setClickEvent();
    }

    void Update()
    {
        DrawMapIcons();    
    }

}
