/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.VDE.UI.HUD
{
    public class HUD : MonoBehaviour
    {
        public VDE vde;
        public TextMeshPro HUDtext;
        public Camera activeCamera;
        public CameraController cameraController;
        public Material notificationLineMaterial;
        public GameObject target;

        internal enum Event
        {
            NotSet,

            Notification,
            Progress,
            SetText
        }

        Vector3 velocity = Vector3.zero;
        internal List<Notification> upperNotificationBarContents = new List<Notification>() { };
        internal Vector3 
            notificationAreaOffset = new Vector3(-1F, 2F, -3F),
            HUDPosition = Vector3.zero;
        internal Dictionary<int, UnityEngine.UI.Image> progress = new Dictionary<int, UnityEngine.UI.Image>() { };
        private bool fadingAway;
        internal bool notificationsEnabled = true;

        private void Start()
        {
            if (activeCamera is null)
            {
                activeCamera = vde.usableCamera;
            }
            if (activeCamera is null)
            {
                activeCamera = UnityEngine.Camera.main;
            }

            foreach (UnityEngine.UI.Image progressor in GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                if (progressor.type == UnityEngine.UI.Image.Type.Filled)
                {
                    progress.Add(progress.Count, progressor);
                }
            }
        }
        void Update()
        {
            if (activeCamera)
            {
                /*
                 * This will work as a dashboard fixed to the horizon
                 * 
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    Vector3.MoveTowards(transform.position, activeCamera.transform.position + positionFromCamera, 30F), 
                    ref velocity, 
                    0.3F
                );
                */
                transform.SetPositionAndRotation(
                    Vector3.SmoothDamp(
                        transform.position,
#if DOTNETWINRT_PRESENT
                        Vector3.MoveTowards(
                            transform.position,
                            target.transform.position,
                            2F
                        ),                        
                        ref velocity,
                        0.6F, 3F
#else
                        Vector3.MoveTowards(
                            transform.position,
                            (activeCamera.transform.forward * HUDPosition.z) + activeCamera.transform.position,
                            HUDPosition.z
                        ),
                        ref velocity,
                        0.6F
#endif
                    ),
                    Quaternion.Slerp(
                        transform.rotation,
                        new Quaternion(
                            0,
                            activeCamera.transform.rotation.y,
                            0,
                            activeCamera.transform.rotation.w
                            ),
                        0.6F
                    )
                );
            }
            else {
                activeCamera = vde.usableCamera;
            }

            if (
                HUDPosition == Vector3.zero && 
                !(vde is null) &&
                !(vde.data is null) &&
                !(vde.data.layouts is null) &&
                !(vde.data.layouts.current is null) &&
                !(vde.data.layouts.current.variables is null) &&
                !(vde.data.layouts.current.variables.vectors is null)
                )
            {
                notificationAreaOffset = vde.data.layouts.current.variables.vectors["notificationPosition"];
                HUDPosition = vde.data.layouts.current.variables.vectors["HUDPosition"];
                HUDtext.text = "";
            }
        }
        internal void AddLineToBoard(string entry)
        {
            HUDtext.text = HUDtext.text + "\n" + entry;
        }
        internal void SetBoardTextTo(string entry)
        {
            HUDtext.text = entry;
        }
        internal void Toggle()
        {
            try
            {
                if (HUDtext.gameObject.activeSelf)
                {
                    HUDtext.gameObject.SetActive(false);
                }
                else
                {
                    HUDtext.gameObject.SetActive(true);
                }
            }
            catch (System.Exception)
            {
            }
        }
        internal void ToggleProgressor(bool setTo = true)
        {
            foreach (UnityEngine.UI.Image image in progress.Values)
            {
                image.gameObject.SetActive(setTo);
            }
        }
        internal void ToggleNotifications()
        {
            if (notificationsEnabled)
            {
                notificationsEnabled = false;
                foreach (Notification notification in upperNotificationBarContents)
                {
                    notification.timeToLive = 0;
                }
            }
            else
            {
                notificationsEnabled = true;
            }
        }
        internal void CreateLabel(Entity target)
        {
            if (
                notificationsEnabled &&
                target.data.layouts.current.variables.indrek["showNotificationsOnDashboard"] > 0 && 
                !upperNotificationBarContents.Where(note => note.entity == target).Any()
                //!upperNotificationBarContents.Exists(note => note.entity == target)
                )
            {
                if(target.containers.GetCurrentShape(out Shape targetShape))
                {
                    Notification notification = gameObject.AddComponent<Notification>();
                    notification.HUD = this;
                    notification.entity = target;
                    notification.target = target.containers.GetCurrentShape().gameObject;
                    notification.text = target.name;
                    notification.activeCamera = activeCamera;
                    notification.fontForRenderer = target.data.VDE.font;
                }
            }
        }
        internal bool ProgressorActive()
        {
            return progress.Where(img => img.Value.fillAmount > 0).Count() > 0 && !fadingAway;
        }
        internal IEnumerator FadeAwayAndDefault()
        {
            fadingAway = true;
            IEnumerable<KeyValuePair<int, UnityEngine.UI.Image>> alphaProgress = progress.OrderByDescending(img => img.Value.color.a);

            while (alphaProgress.FirstOrDefault().Value.color.a > 0)
            {
                alphaProgress.Where(img => img.Value.color.a > 0).ToList().ForEach(img => {
                    if (img.Value.color.a > 0.2)
                    {
                        img.Value.color = new Color(img.Value.color.r, img.Value.color.g, img.Value.color.b, img.Value.color.a * 0.9F);
                    }
                    else
                    {
                        img.Value.color = new Color(img.Value.color.r, img.Value.color.g, img.Value.color.b, 0);
                    }
                });
                yield return new WaitForEndOfFrame();
            }
            progress.ToList().ForEach(img => {
                img.Value.fillAmount = 0;
                img.Value.gameObject.SetActive(false);
            });
            fadingAway = false;
        }
    }
}
