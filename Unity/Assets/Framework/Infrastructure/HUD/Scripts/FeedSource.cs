using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GSFC.ARVR.MRET.Common.Schemas;

public class FeedSource : MonoBehaviour
{
    /// <summary>
    /// Holds the information to be displayed, must be in scene to be used.
    /// </summary>

    public enum Type { virtualFeed, externalFeed, dataFeed, htmlFeed, spriteFeed };
    public enum SpriteType { toggle, number };

    [Tooltip("Title of feed")]
    public string title;
    [Tooltip("Types of feeds")]
    public Type type;
    [Tooltip("Link to video, used for external feeds")]
    public string sourceLink;
    [Tooltip("Timestammp for video feeds")]
    public long time;
    public SpriteType spriteType;
    [Tooltip("Datamanager name for data for sprite")]
    public string spriteData;
    [Tooltip("Textbox to display data to in sprite number mode")]
    public Text value;
    [Tooltip("Image for sprite")]
    public Image sprite;
    [Tooltip("Standby color for image sprite")]
    public Color standby;
    [Tooltip("Alert color for image sprite")]
    public Color alert;

    public bool isAlert = false;
    public bool upToDate;
    private bool isChanged;
    private DataManager dataManager;
    public RenderTexture renderTexture
    {
        get
        {
            return _renderTexture;
        }
    }

    public RenderTexture _renderTexture;

    void Start()
    {
        GameObject loadedProjectObject = GameObject.Find("LoadedProject");
        if (loadedProjectObject)
        {
            UnityProject loadedProject = loadedProjectObject.GetComponent<UnityProject>();
            if (loadedProject)
            {
                dataManager = loadedProject.dataManager;
            }
        }
        switch (type)
        {
            case Type.dataFeed:

                break;

            case Type.externalFeed:

                break;

            case Type.virtualFeed:
                _renderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                _renderTexture.depth = 24;
                _renderTexture.Create();
                Camera cam = gameObject.GetComponent<Camera>();
                cam.targetTexture = renderTexture;
                break;

            case Type.htmlFeed:
                break;

            case Type.spriteFeed:
                switch (spriteType)
                {
                    case SpriteType.toggle:
                        StandbyMode();
                        isChanged = isAlert;
                        break;


                    case SpriteType.number:

                        break;

                }
                break;
        }

    }

    void Update()
    {
        switch (type)
        {
            case Type.dataFeed:

                break;

            case Type.spriteFeed:

                if (dataManager)
                {
                    object val = dataManager.FindPoint(spriteData);

                    switch (spriteType)
                    {
                        case SpriteType.toggle:
                            if (val != null)
                            {
                                isAlert = (bool)val;
                                if (isAlert)
                                {
                                    AlertMode();
                                }
                                else if (!isAlert)
                                {
                                    StandbyMode();
                                }
                                upToDate = false;
                                isChanged = isAlert;
                            }
                            break;


                        case SpriteType.number:
                            if (val != null)
                            {
                                float number = (float)val;
                                string content = number.ToString();
                                value.text = content;
                            }

                            break;
                    }
                }
                break;
        }
    }

    public void AlertMode()
    {
        sprite.color = alert;
    }

    public void StandbyMode()
    {
        sprite.color = standby;
    }

    public VideoSourceType Serialize()
    {
        VideoSourceType feedSource = new VideoSourceType();
        feedSource.ParentName = transform.parent.ToString();
        feedSource.Title = title;

        feedSource.Transform = new UnityTransformType
        {
            Position = new Vector3Type
            {
                X = transform.localPosition.x,
                Y = transform.localPosition.y,
                Z = transform.localPosition.z
            },

            Rotation = new QuaternionType
            {
                W = transform.localRotation.w,
                X = transform.localRotation.x,
                Y = transform.localRotation.y,
                Z = transform.localRotation.z
            },

            Scale = new Vector3Type
            {
                X = transform.localScale.x,
                Y = transform.localScale.y,
                Z = transform.localScale.z
            }
        };

        switch (type)
        {
            case Type.virtualFeed:
                feedSource.Type = VideoSourceTypeType.virtualFeed;
                break;
            case Type.externalFeed:
                feedSource.Type = VideoSourceTypeType.externalFeed;
                feedSource.SourceLink = sourceLink;
                feedSource.Time = time;
                break;
            case Type.dataFeed:
                feedSource.Type = VideoSourceTypeType.dataFeed;
                break;
            case Type.htmlFeed:
                feedSource.Type = VideoSourceTypeType.htmlFeed;
                feedSource.SourceLink = sourceLink;
                break;
            case Type.spriteFeed:
                feedSource.Type = VideoSourceTypeType.spriteFeed;
                feedSource.Standby = ColorUtility.ToHtmlStringRGBA(standby);
                feedSource.Alert = ColorUtility.ToHtmlStringRGBA(alert);
                feedSource.SpriteData = spriteData;
                feedSource.Image = sprite.ToString();
                break;
            default:
                break;
        }

        switch (spriteType)
        {
            case SpriteType.toggle:
                feedSource.SpriteType = VideoSourceTypeSpriteType.Toggle;
                break;
            case SpriteType.number:
                feedSource.SpriteType = VideoSourceTypeSpriteType.Number;
                feedSource.Value = value.text.ToString();
                break;
            default:
                break;
        }

        return feedSource;
    }


    public void Deserialize(VideoSourceType video)
    {
        transform.SetParent(GameObject.Find(video.ParentName).transform);
        title = video.Title;
        sourceLink = video.SourceLink;
        spriteData = video.SpriteData;
        value.text = video.Value;
        time = video.Time;
        sprite = (Image)Resources.Load(video.Image); // This will not work, it needs to be replaced with the path
        Color newCol;
        if (ColorUtility.TryParseHtmlString(video.Alert, out newCol))
        {
            alert = newCol;
        }
        if (ColorUtility.TryParseHtmlString(video.Standby, out newCol))
        {
            standby = newCol;
        }

        float x = video.Transform.Position.X;
        float y = video.Transform.Position.Y;
        float z = video.Transform.Position.Z;
        transform.localPosition = new Vector3(x, y, z);

        x = video.Transform.Scale.X;
        y = video.Transform.Scale.Y;
        z = video.Transform.Scale.Z;
        transform.localScale = new Vector3(x, y, z);

        x = video.Transform.Rotation.X;
        y = video.Transform.Rotation.Y;
        z = video.Transform.Rotation.Z;
        float w = video.Transform.Rotation.W;
        transform.localRotation = new Quaternion(x, y, z, w);


        switch (video.Type)
        {
            case VideoSourceTypeType.virtualFeed:
                type = Type.virtualFeed;
                break;
            case VideoSourceTypeType.externalFeed:
                type = Type.externalFeed;
                break;
            case VideoSourceTypeType.dataFeed:
                type = Type.dataFeed;
                break;
            case VideoSourceTypeType.htmlFeed:
                type = Type.htmlFeed;
                break;
            case VideoSourceTypeType.spriteFeed:
                type = Type.spriteFeed;
                break;
            default:
                break;
        }

        switch (video.SpriteType)
        {
            case VideoSourceTypeSpriteType.Toggle:
                spriteType = SpriteType.toggle;
                break;
            case VideoSourceTypeSpriteType.Number:
                spriteType = SpriteType.number;
                break;
            default:
                break;
        }
    }
}