using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET;

namespace GSFC.ARVR.MRET.Infrastructure.Framework.VirtualCameras
{
    /// <remarks>
    /// History:
    /// June 2021: V1 Added into MRET
    /// </remarks>
    /// <summary>
    /// BaseVirtualCamera is a class that provides
    /// top-level control of The Base Virtual Camera in MRET.
    /// Author: Jonathan T. Reynolds
    /// </summary>
    [ExecuteInEditMode]
    public class BaseVirtualCamera : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(BaseVirtualDisplay);
            }
        }

        [Tooltip("This will hide the camera visuals at runtime.")]
        public bool HideVisuals;

        /// <summary>
        /// This should coorispond to the index in the VirtualCameras list as it's identifier. 
        /// <see cref="SetCamIdentifier(CAMUUID)"/> to set properly
        /// </summary>
        public CAMUUID camIdentifier { protected set; get; }

        /// <summary>
        /// This sets whether the preview visual for the camera is set.
        /// </summary>
        public bool camActive { protected set; get; }
        /// <summary>
        /// This sets whether or not we're recording from this camera
        /// </summary>
        public bool recordActive { protected set; get; }


        /// <summary>
        /// This is the default camera feed size, it can be changed.
        /// </summary>
        [Tooltip("This is the output render Texture Size")]
        public Vector2Int CameraFeedSize = new Vector2Int(1920, 1080);

        /// <summary>
        /// These are the base variable components that should be preset
        /// </summary>
        [Tooltip("These Variables should be preset in the prefab.")]
        public CoreLinkComponents coreLinkComponents = new CoreLinkComponents();


        /// <summary>
        /// Turn the editor camera visual off on start if it's de-activated.
        /// </summary>
        protected override void MRETStart()
        {
            if (HideVisuals)
            {
                SetCamVisual(false);
            }
        }


        /// <summary>
        /// This is the specific set function for the Camera Identifier. 
        /// </summary>
        /// <param name="UUID"></param>
        public void SetCamIdentifier(CAMUUID UUID)
        {
            camIdentifier = UUID;
        }


        /// <summary>
        /// This coorisponds to the Actual Camera Visual, the camera model. 
        /// and it will set that based on the given bool.
        /// </summary>
        /// <param name="active"></param>
        public void SetCamVisual(bool active)
        {
            if (coreLinkComponents.Visuals != null)
            {
                coreLinkComponents.Visuals.SetActive(active);
            }
        }

        /// <summary>
        /// This will set whether or not the preview visual on this camera is active or not. 
        /// </summary>
        /// <param name="active"></param>
        public void SetCamActive(bool active)
        {
            if (coreLinkComponents.PreviewVisual != null)
            {
                coreLinkComponents.PreviewVisual.SetActive(active);
                camActive = active;
            }
        }

        /// <summary>
        /// This will set whether or not the record icon is showing from this camera to indicate 
        /// it's being recorded from.
        /// </summary>
        /// <param name="active"></param>
        public void SetRecordActive(bool active)
        {
            if (coreLinkComponents.PreviewVisual != null)
            {
                coreLinkComponents.RecordIcons.SetActive(active);
                recordActive = active;
            }
        }

        /// <summary>
        /// This sets the camera feed active, the actual output of the render Texture.
        /// </summary>
        /// <param name="active"></param>
        public void SetFeedActive(bool active)
        {

            if (coreLinkComponents.CameraFeed != null)
            {
                coreLinkComponents.CameraFeed.gameObject.SetActive(active);
                if (!CameraFeedHasRenderTexture())
                {
                    Debug.Log($"[BaseVirtualCamera] WARNING: Feed active but no render texture is set. " +
                        $"Automatically making Render Texture with size {CameraFeedSize.x}x{CameraFeedSize.y}");

                }
            }
        }


        /// <summary>
        /// This removes whatever rendertexture exists on the camera
        /// </summary>
        public void RemoveFeedRenderTexture()
        {
            RenderTexture toDelete = coreLinkComponents.CameraFeed.targetTexture;
            coreLinkComponents.CameraFeed.targetTexture = null;
            if (Application.isEditor)
            {
                DestroyImmediate(toDelete);
            }
            else
            {
                Destroy(toDelete);
            }
        }

        /// <summary>
        /// This is a basic set function that just sets a camera feed based on it's default size
        /// </summary>
        public void SetFeedRenderTexture()
        {
            SetFeedRenderTexture(CameraFeedSize);
        }


        /// <summary>
        /// This Sets the FeedRender Texture based on a given screen width and height
        /// </summary>
        /// <param name="screenWidthHeight"></param>
        public void SetFeedRenderTexture(Vector2Int screenWidthHeight)
        {
            CameraFeedSize = screenWidthHeight;
            if (coreLinkComponents.CameraFeed.targetTexture == null)
            {
                RenderTexture rt = new RenderTexture((int)screenWidthHeight.x, (int)screenWidthHeight.y, 16, RenderTextureFormat.ARGB32);
                rt.name = "RenderTex_" + transform.name;
                rt.Create();
                SetFeedRenderTexture(rt);
            }
        }


        /// <summary>
        /// This sets the feed render Texture given a new render texture.
        /// </summary>
        /// <param name="tex"></param>
        public void SetFeedRenderTexture(RenderTexture tex)
        {
            coreLinkComponents.CameraFeed.targetTexture = tex;
        }


        /// <summary>
        /// This returns the status of the target Render texture on the camera Feed
        /// </summary>
        /// <returns>Returns true if it exists and false if it doesn't</returns>
        public bool CameraFeedHasRenderTexture()
        {
            return coreLinkComponents.CameraFeed.targetTexture != null;
        }

    }


    /// <summary>
    /// This class represents the core preset link components that should be set on the prefab.
    /// and shouldn't change between cameras.
    /// </summary>
    [Serializable]
    public class CoreLinkComponents
    {
        public Transform RecordAttachPoint;
        public GameObject Visuals;
        public GameObject PreviewVisual;
        public GameObject RecordIcons;
        public Camera CameraFeed;
    }

    /// <summary>
    /// This represents the Cameras unique identifier. And how it acts.
    /// </summary>
    public class CAMUUID : IEquatable<CAMUUID>
    {
        public readonly int UUID;

        public CAMUUID(int newUUID)
        {
            UUID = newUUID;
        }

        public override string ToString()
        {
            return UUID.ToString();
        }

        public static bool operator ==(CAMUUID a, CAMUUID b)
        {
            // an item is always equal to itself
            if (object.ReferenceEquals(a, b))
                return true;

            // if both a and b were null, we would have already escaped so check if either is null
            if (object.ReferenceEquals(a, null))
                return false;
            if (object.ReferenceEquals(b, null))
                return false;

            // Now that we've made sure we are working with real objects:
            return a.UUID == b.UUID;
        }

        public static bool operator !=(CAMUUID a, CAMUUID b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CAMUUID);
        }

        public bool Equals(CAMUUID other)
        {
            return other == this;
        }

        public override int GetHashCode()
        {
            return UUID.GetHashCode();
        }
    }
}