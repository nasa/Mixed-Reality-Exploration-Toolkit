using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GOV.NASA.GSFC.XR.MRET;


namespace GOV.NASA.GSFC.XR.VirtualCamera
{
    /// <remarks>
    /// History:
    /// June 2021: V1 Added into MRET
    /// </remarks>
    /// <summary>
    /// BaseVirtualDisplay is a class that provides
    /// top-level control of The Base Virtual Display in MRET.
    /// Author: Jonathan T. Reynolds
    /// </summary>
    [ExecuteInEditMode]
    public class BaseVirtualDisplay : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(BaseVirtualDisplay);
            }
        }

        /// <summary>
        /// This is to be the unique identifier for the display <see cref="SetDisplayIdentifier(DISPLAYUUID)"/>
        /// </summary>
        public DISPLAYUUID displayUUID { protected set; get; }

        /// <summary>
        /// This is the mesh scale down factor. When a render texture is set <see cref="SetRenderTexture(RenderTexture)"/>
        /// This will scale the display to match the size based on this scale down factor
        /// for example, 1920 will get scaled down to 0.192 if using a factor of 10,000 
        /// Adjust this to shange that scale factor.
        /// </summary>
        public float meshScaleDownFactor = 10000.0f;

        /// <summary>
        /// This is the currently set Render Texture, it's contained in this class for 
        /// easy access and reference
        /// </summary>
        public RenderTexture displayTexture;


        /// <summary>
        /// This is the core material on the Display. It's set up to add a new Material based on the default 
        /// material and shader from whatever render pipeline is being used. 
        /// </summary>
        private Material coreMat = null;


        /// <summary>
        /// This sets the current mesh and shader based on what pipeline is being used. 
        /// </summary>
        protected override void MRETStart()
        {
            Material mat;
            if (GraphicsSettings.currentRenderPipeline)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                {
                    mat = new Material(Shader.Find("HDRP/Lit"));
                }
                else
                {
                    mat = new Material(Shader.Find("URP/Lit"));
                }
            }
            else
            {
                mat = new Material(Shader.Find("Specular"));
            }

            coreMat = GetComponent<Renderer>().material = mat;
        }

        /// <summary>
        /// This is the Display identifier setter that the display can be identified by. 
        /// </summary>
        /// <param name="newUID"></param>
        public void SetDisplayIdentifier(DISPLAYUUID newUID)
        {
            displayUUID = newUID;
        }


        /// <summary>
        /// This sets the render texture of the display based on a given Render texture
        /// This will also resize the display based on the given render texture and it's default 
        /// it's scale down factor <see cref="meshScaleDownFactor"/>
        /// </summary>
        /// <param name="renderTexture"></param>
        /// <param name="scaleMeshToRTsize">This is the bool to scale down the mesh baed on RT size, 
        /// Default value is true</param>
        public void SetRenderTexture(RenderTexture renderTexture, bool scaleMeshToRTsize = true)
        {
            displayTexture = renderTexture;
            if (coreMat)
                coreMat.mainTexture = displayTexture;
            else
                GetComponent<Material>().mainTexture = displayTexture;

            if(scaleMeshToRTsize)
                SetMeshSizeToTextureSize(displayTexture);
        }


        /// <summary>
        /// This will remove the current render texture on the display. 
        /// </summary>
        public void RemoveRenderTexture()
        {
            displayTexture = null;
            if (coreMat)
                coreMat.mainTexture = null;
            else
                GetComponent<Material>().mainTexture = null;

        }

        /// <summary>
        /// Given a render texture, this will scale the mesh down based on the mesh scale factor
        /// <see cref="meshScaleDownFactor"/>
        /// </summary>
        /// <param name="rt"></param>
        private void SetMeshSizeToTextureSize(RenderTexture rt)
        {
            Vector3 newSize = new Vector3(rt.width / meshScaleDownFactor, 0.0f, rt.height / meshScaleDownFactor);
            transform.localScale = newSize;
        }
    }

    /// <summary>
    /// This is the class container that is responsible for containing the display Unique identifier
    /// It also contains all the comparison functions needed. 
    /// </summary>
    public class DISPLAYUUID : IEquatable<DISPLAYUUID>
    {
        public readonly int UUID;

        public DISPLAYUUID(int newUUID)
        {
            UUID = newUUID;
        }

        public static bool operator ==(DISPLAYUUID a, DISPLAYUUID b)
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

        public static bool operator !=(DISPLAYUUID a, DISPLAYUUID b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DISPLAYUUID);
        }

        public bool Equals(DISPLAYUUID other)
        {
            return other == this;
        }

        public override int GetHashCode()
        {
            return UUID.GetHashCode();
        }
    }
}