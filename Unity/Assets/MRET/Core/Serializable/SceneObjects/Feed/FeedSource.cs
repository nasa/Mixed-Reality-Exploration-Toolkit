// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GOV.NASA.GSFC.XR.MRET.Configuration;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Feed
{
    // TODO: Reconsider whether feeds are scene objects or simply Identifiable.

    /// <remarks>
    /// History:
    /// </remarks>
    /// <summary>
    /// Defines a feed in MRET. A feed is a source of content.
    /// 
    /// Author: Jeffreey Hosler (refactored)
    /// </summary>
    public class FeedSource : SceneObject<FeedType>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(FeedSource);

        /// <summary>
        /// A serialized representation of this object. Used to retain the structure of the deserialized
        /// object for later serialization. Values should be assigned via the <code>Serialize</code>
        /// method before accessing.</br>
        /// </summary>
        private FeedType serializedFeedType;

        /// <summary>
        /// Holds the information to be displayed, must be in scene to be used.
        /// </summary>

        public enum Type { virtualFeed, externalFeed, dataFeed, htmlFeed, spriteFeed };
        public enum SpriteType { toggle, number };

        [Tooltip("Types of feeds")]
        public Type type = Type.virtualFeed;
        [Tooltip("Link to video, used for external feeds")]
        public string sourceLink;
        [Tooltip("Timestammp for video feeds")]
        public long time;
        public SpriteType spriteType = SpriteType.toggle;
        [Tooltip("Textbox to display data to in sprite number mode")]
        public Text value;
        [Tooltip("Image for sprite")]
        public Image sprite;
        [Tooltip("Standby color for image sprite")]
        public Color standby;
        [Tooltip("Alert color for image sprite")]
        public Color alert;

        /// <summary>
        /// Parent ID for this feed
        /// </summary>
        public string ParentID
        {
            // TODO: This process needs to be ironed out. Perhaps the ParentID is stored as
            // a "Temp" ParentID and another function is available to the project to resolve
            // "parents" and "attachto". Perhaps the attributes are contained within
            // IIdentifiable or ISceneObject available for all components to use, and the handling
            // logic is isolated in one place.
            get => (parent != null) ? parent.id : null;
            set
            {
                // Set the parent
                if (string.IsNullOrEmpty(value))
                {
                    // Check to see if we already had a parent specified
                    if (!string.IsNullOrEmpty(ParentID))
                    {
                        // Clearing the parent
                        transform.SetParent(null);
                    }
                }
                else
                {
                    IIdentifiable feedParent = MRET.UuidRegistry.GetByID(value);
                    if (feedParent != null)
                    {
                        transform.SetParent(feedParent.gameObject.transform);
                    }
                    else
                    {
                        // Log a warning
                        LogWarning("Specified feed parent ID does not exist in the registry: " + value, nameof(ParentID));
                    }
                }
            }
        }
        public bool isAlert = false;
        public bool upToDate;
        private bool isChanged;

        public RenderTexture renderTexture
        {
            get
            {
                return _renderTexture;
            }
        }

        [SerializeField]
        [Tooltip("The render texture for the feed")]
        private RenderTexture _renderTexture;

        #region MRETUpdateBehaviour
        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            base.MRETAwake();

            // Set the defaults
            ParentID = null;
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

            // Set up the feeds
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
                    if (cam != null)
                    {
                        cam.targetTexture = renderTexture;
                    }
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

        /// <seealso cref="MRETUpdateBehaviour.MRETUpdate"/>
        protected override void MRETUpdate()
        {
            base.MRETUpdate();

            switch (type)
            {
                case Type.dataFeed:
                    break;

                case Type.spriteFeed:
                    object val = MRET.DataManager.FindPoint(id);

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
                    break;
            }
        }
        #endregion MRETUpdateBehaviour

        #region Serializable
        /// <seealso cref="Versioned{T}.CreateInstance"/>
        public override FeedType CreateSerializedType()
        {
            FeedType result = null;

            switch (type)
            {
                case Type.externalFeed:
                    result = new ExternalFeedType();
                    break;
                case Type.dataFeed:
                    result = new DataFeedType();
                    break;
                case Type.htmlFeed:
                    result = new HtmlFeedType();
                    break;
                case Type.spriteFeed:
                    result = new SpriteFeedType();
                    break;

                case Type.virtualFeed:
                default:
                    result = new VirtualFeedType();
                    break;
            }

            return result;
        }

        /// <seealso cref="SceneObject{T}.Deserialize(T, SerializationState)"/>
        protected override IEnumerator Deserialize(FeedType serialized, SerializationState deserializationState)
        {
            StartCoroutine(base.Deserialize(serialized, deserializationState));

            // Wait for the coroutine to complete
            while (!deserializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (deserializationState.IsError) yield break;

            // Start with an incomplete indicator
            deserializationState.Clear();

            // Process this object specific deserialization

            // Save the serialized reference
            serializedFeedType = serialized;

            // TODO: Possible race condition. Parent may not have been loaded yet!
            if (!string.IsNullOrEmpty(serialized.ParentID))
            {
                ParentID = serialized.ParentID;
            }

            // Deserialize the feed
            if (serialized is VirtualFeedType)
            {
                // Deserialize the virtual feed
                type = Type.virtualFeed;

                // TODO: There is a more efficient way to do this. Right now it is done in MRETStart
                // but that isn't the correct place to do it. It should be in code that watches for
                // changes to the "type" and the RenderTexture should only be created once (the first
                // time it is needed. Camera reference too!
                _renderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                _renderTexture.depth = 24;
                _renderTexture.Create();
                Camera cam = gameObject.GetComponent<Camera>();
                cam.targetTexture = renderTexture;
            }
            else if (serialized is LinkFeedType)
            {
                // Deserialize the link feed
                sourceLink = (serialized as LinkFeedType).URL;
                if (serialized is ExternalFeedType)
                {
                    // Deserialize the external feed
                    type = Type.externalFeed;
                    time = (serialized as ExternalFeedType).TimeMillis;
                }
                else if (serialized is HtmlFeedType)
                {
                    // Deserialize the HTML feed
                    type = Type.htmlFeed;
                }
            }
            else if (serialized is DataFeedType)
            {
                // Deserialize the data feed
                type = Type.dataFeed;
            }
            else if (serialized is SpriteFeedType)
            {
                SpriteFeedType serializedSpriteFeed = (serialized as SpriteFeedType);

                // Deserialize the sprite feed
                type = Type.spriteFeed;
                SchemaUtil.DeserializeFeedSpriteType(serializedSpriteFeed.SpriteType, ref spriteType);

                //SchemUtil.DeserializeImage
                sprite = Resources.Load(serializedSpriteFeed.Image, typeof(Image)) as Image;
                Color newCol;
                if (ColorUtility.TryParseHtmlString(serializedSpriteFeed.Standby, out newCol))
                {
                    standby = newCol;
                }
                if (ColorUtility.TryParseHtmlString(serializedSpriteFeed.Alert, out newCol))
                {
                    alert = newCol;
                }
                value.text = serializedSpriteFeed.Value;
            }

            // Record the deserialization state as complete
            deserializationState.complete = true;

            yield return null;
        }

        /// <seealso cref="SceneObject{T}.Serialize(T, SerializationState)"/>
        protected override IEnumerator Serialize(FeedType serialized, SerializationState serializationState)
        {
            StartCoroutine(base.Serialize(serialized, serializationState));

            // Wait for the coroutine to complete
            while (!serializationState.IsComplete) yield return new WaitForSeconds(ConfigurationManager.DEFAULT_COROUTINE_WAIT);

            // If the parent failed, there's no point in continuing
            if (serializationState.IsError) yield break;

            // Start with an incomplete indicator
            serializationState.Clear();

            // Process this object specific serialization
            serialized.ParentID = ParentID != null ? ParentID : null;

            // Serialize the feed
            switch (type)
            {
                case Type.externalFeed:
                    ((ExternalFeedType)serialized).URL = sourceLink;
                    ((ExternalFeedType)serialized).TimeMillis = time;
                    break;
                case Type.dataFeed:
                    break;
                case Type.htmlFeed:
                    ((HtmlFeedType)serialized).URL = sourceLink;
                    break;
                case Type.spriteFeed:
                    ((SpriteFeedType)serialized).Standby = ColorUtility.ToHtmlStringRGBA(standby);
                    ((SpriteFeedType)serialized).Alert = ColorUtility.ToHtmlStringRGBA(alert);
                    ((SpriteFeedType)serialized).Image = sprite.ToString();
                    ((SpriteFeedType)serialized).Value = value.ToString();
                    SchemaUtil.SerializeFeedSpriteType(spriteType, ((SpriteFeedType)serialized).SpriteType);
                    break;

                case Type.virtualFeed:
                default:
                    break;
            }

            // Save the final serialized reference
            serializedFeedType = serialized;

            // Record the serialization state as complete
            serializationState.complete = true;

            yield return null;
        }
        #endregion Serializable

        public void AlertMode()
        {
            sprite.color = alert;
        }

        public void StandbyMode()
        {
            sprite.color = standby;
        }

    }
}