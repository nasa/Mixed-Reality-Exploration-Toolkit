/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VDE.UI.HUD
{
    internal class Notification : MonoBehaviour
    {
        internal GameObject target, thisGlazer;
        internal HUD HUD;
        internal Data data;
        private Bounds glazerBounds;
        internal Camera activeCamera;
        internal string text;
        internal Font fontForRenderer;
        internal float timeToLive, notificationOffset, notificationScale;
        private int positionInPreviousFrame; // not a vector, but sequential position amongst hud notifications.
        private LineRenderer lineRenderer;
        private Vector3
            notificationsDesiredPosition,
            velocity = Vector3.zero;
        internal Entity entity;

        Vector3 NotificationsDesiredPosition()
        {
            /*
             * for horizontal dashboard
             * 
            return HUD.notificationAreaOffset + new Vector3(
                    (float)positionInPreviousFrame * notificationOffset,
                    0f,
                    0f
                );
                */
            return HUD.notificationAreaOffset + new Vector3(
                    0f,
                    (float)positionInPreviousFrame * notificationOffset,
                    0f
                );
        }
        void Start()
        {
            data = HUD.vde.data;
            HUD.upperNotificationBarContents.Add(this);
            positionInPreviousFrame = HUD.upperNotificationBarContents.IndexOf(this);
            notificationOffset = data.layouts.current.variables.floats["notificationOffset"];
            notificationScale = data.layouts.current.variables.floats["notificationScale"];
            thisGlazer = new GameObject("HUDnotification_" + target.GetInstanceID().ToString());
            thisGlazer.transform.parent = HUD.transform;
            thisGlazer.transform.localPosition = notificationsDesiredPosition = NotificationsDesiredPosition();
            timeToLive = data.layouts.current.variables.floats["timeToGazeFocusOut"] + Time.realtimeSinceStartup;

            Canvas canvas = thisGlazer.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            UnityEngine.UI.CanvasScaler cs = thisGlazer.AddComponent<UnityEngine.UI.CanvasScaler>();
            cs.scaleFactor = notificationScale;
            cs.dynamicPixelsPerUnit = 10f;
            cs.referencePixelsPerUnit = 1f;

            thisGlazer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 3.0f);
            thisGlazer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 3.0f);
            thisGlazer.GetComponent<RectTransform>().localScale = notificationScale * Vector3.one;// new Vector3(0.3f, 0.3f, 0.3f);

            UnityEngine.UI.Text t = thisGlazer.AddComponent<UnityEngine.UI.Text>();
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.font = fontForRenderer;
            t.fontSize = 4;
            t.text = text;
            t.enabled = true;
            t.color = Color.white;

            lineRenderer = thisGlazer.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = HUD.notificationLineMaterial;
            lineRenderer.transform.SetParent(thisGlazer.transform);
            lineRenderer.startColor = lineRenderer.endColor = Color.white;
            lineRenderer.widthMultiplier = lineRenderer.startWidth = lineRenderer.endWidth = data.layouts.current.variables.floats["notificationConnectorWidth"];
            lineRenderer.numCapVertices = lineRenderer.numCornerVertices = lineRenderer.positionCount = data.layouts.current.variables.indrek["notificationConnectorPositions"];


            lineRenderer.SetPositions(CalculateNotificationConnectorLinePositions(lineRenderer.positionCount).ToArray());
        }
        private List<Vector3> CalculateNotificationConnectorLinePositions(int nrOfLinePositions)
        {
            List<Vector3> linePositions = new List<Vector3>() { };

            try
            {
                Vector3[] pointsForCurveCalculation =
                {
                    thisGlazer.transform.position,
                    thisGlazer.transform.position + new Vector3(
                        0, -0.3F, 0.4F),
                    target.transform.position + new Vector3(
                        0, 0.3F, -0.4F),
                    target.transform.position,
                };


                for (int position = 0; position < nrOfLinePositions; position++)
                {
                    linePositions.Add(Tools.CalculateCubicBezierPoint((float)position / (float)nrOfLinePositions, pointsForCurveCalculation));
                }
            }
            catch (MissingReferenceException)
            {
                // in some cases the nodegroup control object may be destroyed while the text is still active.
                // no biggie, lets chug on.
            }

            return linePositions;
        }
        private void Update()
        {
            if (timeToLive < Time.realtimeSinceStartup)
            {
                HUD.upperNotificationBarContents.Remove(this);
                Destroy(thisGlazer);
                Destroy(this);
            }

            if (positionInPreviousFrame != HUD.upperNotificationBarContents.IndexOf(this))
            {
                positionInPreviousFrame = HUD.upperNotificationBarContents.IndexOf(this);
                notificationsDesiredPosition = NotificationsDesiredPosition();
            }

            thisGlazer.transform.localPosition = Vector3.SmoothDamp(
                thisGlazer.transform.localPosition,
                Vector3.MoveTowards(thisGlazer.transform.localPosition, notificationsDesiredPosition, 30F),
                ref velocity,
                0.3F
            );

            thisGlazer.transform.rotation = Quaternion.Slerp(
                thisGlazer.transform.rotation,
                new Quaternion(
                    0,
                    activeCamera.transform.rotation.y,
                    0,
                    activeCamera.transform.rotation.w
                    ),
                0.6F
            );

            lineRenderer.SetPositions(CalculateNotificationConnectorLinePositions(lineRenderer.positionCount).ToArray());
        }
    }
}
