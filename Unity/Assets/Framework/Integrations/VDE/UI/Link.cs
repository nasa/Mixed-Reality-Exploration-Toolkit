/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System;
using System.Collections;
using UnityEngine;

namespace Assets.VDE.UI
{
    /// <summary>
    /// used by VDE server to send in links
    /// </summary>
    public class ImportLink : System.IComparable
    {
        public int s { get; set; }
        public int d { get; set; }
        public int w { get; set; }
        public Status status;
        public enum Status
        {
            None,
            GotSrc,
            GotSrcAndDst,
            BeingCreated,
            Faulty,
            Done,
            Unnatural,
            Duplicate
        }
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            ImportLink nodeToCompare = (ImportLink)obj;
            return s.CompareTo(nodeToCompare.s) + d.CompareTo(nodeToCompare.d) + w.CompareTo(nodeToCompare.w);
        }
    }
    /// <summary>
    /// used by VDE server to send in links
    /// </summary>
    public class ExportLink : System.IComparable
    {
        public int s { get; set; }
        public int d { get; set; }
        public int w { get; set; }
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            ImportLink nodeToCompare = (ImportLink)obj;
            return s.CompareTo(nodeToCompare.s) + d.CompareTo(nodeToCompare.d) + w.CompareTo(nodeToCompare.w);
        }
    }
#if MRET_2021_OR_LATER
    internal class Link : GSFC.ARVR.MRET.Infrastructure.Framework.Interactable.Interactable
#else
    internal class Link : MonoBehaviour
#endif
    {
        Log log;
        Data data;
        internal Color colour;
        BoxCollider collie;
        internal enum Event
        {
            None = 0,
            Create,
            Delete,
            Ready,
            Highlight
        }

        internal Container sourceContainer, destinationContainer;
        LineRenderer lineRenderer;
        internal Entity source, destination;
        internal int weight;
        internal float alpha, defaultWidth, lineWidth, defaultScale, currentScale;
        internal bool visibleOnCreation, highlightedOnCreation, highlighted;
        internal string materialColourName = "_TintColor"; //_TintColor being the default for pre-HDRP materials.
        internal bool ready = false;

        /// <summary>
        /// this cannot be done via ProcessContainer, as then the target entity might not yet exist,
        /// while links.import tasks waits until the target exists, before calling here.
        /// </summary>
        internal void Init(Data data)
        {
            this.data = data;
            defaultWidth = data.layouts.current.GetValueFromConf("edgeWidth");
            defaultScale = data.layouts.current.GetValueFromConf("defaultScale");
            lineWidth = defaultWidth;// * globalScale;
            log = new Log("Link from " + source.name + " => " + destination.name);
            if (!source.containers.GetContainer(out sourceContainer))
            {
                data.messenger.SubscribeToEvent(ReceiveCreatures, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), source.id);
                data.UI.nodeFactory.GetCreaturesOfEntity(source.id, ReceiveCreatures);
            }
            if(!destination.containers.GetContainer(out destinationContainer))
            {
                data.messenger.SubscribeToEvent(ReceiveCreatures, Layouts.Layouts.LayoutEvent.GotContainer.ToString(), destination.id);
                data.UI.nodeFactory.GetCreaturesOfEntity(destination.id, ReceiveCreatures);
            }
            CheckIfItsTime();
        }

        private void Start()
        {
#if MRET_2021_OR_LATER
            grabBehavior = GrabBehavior.Custom;
            touchBehavior = TouchBehavior.Custom;
            grabbable = false;
            useable = true;
#endif
        }
        private void OnEnable()
        {
            UpdatePositions();
        }
        internal IEnumerator ReceiveCreatures(object[] anObject)
        {
            if (
                anObject.Length == 3
                && (anObject[0] as Entity == source || anObject[0] as Entity == destination)
                && !(anObject[1] is null)
                && anObject[1].GetType().Name == "Container"
            )
            {
                Entity incoming = anObject[0] as Entity;
                if (incoming == source)
                {
                    sourceContainer = anObject[1] as Container;
                }
                else if (incoming == destination)
                {
                    destinationContainer = anObject[1] as Container;
                }
                CheckIfItsTime();
            }
            yield return true;
        }

        internal void SetCollie(bool setTo)
        {
            if (!(collie is null))
            {
                collie.enabled = setTo;
            }
        }

        internal void UnHighlight()
        {
            SetCollie(false);
            SetFocus(false, false);
        }

        internal void Highlight()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            SetCollie(true);
            SetFocus(true, false);
        }

        internal void Disable()
        {
            gameObject.SetActive(false);
            SetCollie(false);
        }

        private void CheckIfItsTime()
        {
            if (lineRenderer is null && !(sourceContainer is null) && !(destinationContainer is null))
            {
                CreateLine();
                source.AddOrUpdateLink(this);
                destination.AddOrUpdateLink(this);
            }
        }

        private void CreateLine()
        {
            gameObject.transform.SetParent(sourceContainer.transform);
            gameObject.transform.localPosition = Vector3.zero;

            lineRenderer = gameObject.GetComponent<LineRenderer>();
            lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, destinationContainer.transform.position);
            lineRenderer.SetPosition(0, sourceContainer.transform.position);
            colour = lineRenderer.endColor;
            //colour = lineRenderer.material.GetColor(materialColourName);
            SetColour(alpha);
            //lineRenderer.material.EnableKeyword("_EmissiveExposureWeight");
            sourceContainer.hasLinksToUpdate = destinationContainer.hasLinksToUpdate = true;

            if (!sourceContainer.gameObject.TryGetComponent<Assets.VDE.UI.Node.Updater>(out _))
            {
                Assets.VDE.UI.Node.Updater update = sourceContainer.gameObject.AddComponent<Assets.VDE.UI.Node.Updater>();
                update.owner = sourceContainer;
                update.enabled = true;
            }

            if (!destinationContainer.gameObject.TryGetComponent<Assets.VDE.UI.Node.Updater>(out _))
            {
                Assets.VDE.UI.Node.Updater update = destinationContainer.gameObject.AddComponent<Assets.VDE.UI.Node.Updater>();
                update.owner = destinationContainer;
                update.enabled = true;
            }

            collie = gameObject.GetComponent<BoxCollider>();
            collie.isTrigger = true;
            collie.size = Vector3.one;
            SetCollie(data.layouts.current.variables.flags["edgeCollidersEnabled"]);
            ResizeSelf();

            if (!data.links.linksReady && (visibleOnCreation || highlightedOnCreation))
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
            
            if (highlightedOnCreation)
            {
                SetHighlightedColour(true);
            }
            ready = true;
            Announce();
        }

        private void Announce()
        {
            data.messenger.Post(new Communication.Message() { 
                LinkEvent = Event.Ready,
                obj = new object[] { source.id, destination.id }
            });
        }

        private void SetHighlightedColour(bool setTo)
        {
            if (lineRenderer is null)
            {
                highlightedOnCreation = true;
            }
            else
            {
                if (setTo)
                {
                    // negative expw turns on emissions.
                    lineRenderer.material.SetFloat("_EmissiveExposureWeight", 1 - (alpha + 0.123F));
                    //lineRenderer.material.SetColor(materialColourName, new Color(colour.r, colour.g, colour.b, alpha + 0.3F));
                    SetColour(alpha + 0.3F, true);
                    highlighted = true;
                }
                else
                {
                    lineRenderer.material.SetFloat("_EmissiveExposureWeight", 0.9F);
                    //lineRenderer.material.SetColor(materialColourName, colour);
                    SetColour(alpha, true);
                    highlighted = false;
                }
            }
        }

        internal void SetMaterialTo(Material setLinkMaterialTo)
        {
            lineRenderer.material = setLinkMaterialTo;
        }

        private void ResizeSelf()
        {
            if (!(lineRenderer is null))
            {
                transform.position = lineRenderer.GetPosition(0) + (lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)) / 2;
                transform.LookAt(lineRenderer.GetPosition(0));
                transform.localScale = new Vector3(lineWidth, lineWidth, (lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)).magnitude / defaultScale);
                //transform.localScale = new Vector3(0.2F, 0.2F, (lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)).magnitude / defaultScale);
            }
        }

        internal void UpdatePosition(Container container)
        {
            if (container == sourceContainer)
            {
                lineRenderer.SetPosition(0, container.transform.position);
            }
            else if (container == destinationContainer)
            {
                lineRenderer.SetPosition(1, container.transform.position);
            }
            ResizeSelf();
        }

        internal void UpdatePositions()
        {
            if (!(sourceContainer is null))
            {
                lineRenderer.SetPosition(0, sourceContainer.transform.position);
            }
            if (!(destinationContainer is null))
            {
                lineRenderer.SetPosition(1, destinationContainer.transform.position);
            }
            ResizeSelf();
        }

        internal void GlobalScaleChangedTo(float scale)
        {
            currentScale = scale / data.layouts.current.GetValueFromConf("defaultScale");
            lineWidth = defaultWidth * currentScale;
            lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
        }

        internal void SetFocus(bool setTo, bool setText = true)
        {
            SetHighlightedColour(setTo);
            if (setTo && setText)
            {
                data.VDE.controller.SetNotificationText(name);
            }
        }

        internal void SetColour(float alphaville, bool temp = false)
        {
            if (!temp)
            {
#if UNITY_2019_4_17
                alpha = alphaville / 2;
#else
                alpha = alphaville;
#endif
                colour = new Color(colour.r, colour.g, colour.b, alpha);
            }
            if (!(lineRenderer is null))
            {
                lineRenderer.startColor = lineRenderer.endColor = colour;
                if (!(destination.c is null) && ColorUtility.TryParseHtmlString(destination.c, out Color dstColour))
                {
                    lineRenderer.endColor = dstColour;
                }
                if (!(source.c is null) && ColorUtility.TryParseHtmlString(source.c, out Color srcColour))
                {
                    lineRenderer.startColor = srcColour;
                }
#if HDRP
                lineRenderer.material.SetColor(materialColourName, new Color(1, 1, 1, alphaville * 2));
#else
                lineRenderer.material.SetColor(materialColourName, new Color(1, 1, 1, alphaville));
#endif
            }
        }

#if MRET_2021_OR_LATER
        protected override void BeginTouch(GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.InputHand hand)
        {
            SetFocus(true);
        }
        protected override void EndTouch()
        {
            SetFocus(false);
        }
#endif
    }
}