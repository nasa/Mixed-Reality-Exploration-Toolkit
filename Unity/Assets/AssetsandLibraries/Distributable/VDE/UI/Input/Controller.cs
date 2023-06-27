/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
//#define MRET
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.UI.Input
{
    internal class Controller : MonoBehaviour
    {
        Log log;
        Data data;
        HUD.HUD hud;
        ViveObserver viveObserver;
        internal InputObserver inputObserver;
        internal CameraObserver cameraObserver;
        internal CameraCollider cameraCollider;
        internal CameraController cameraController;
        internal Hands.Hand.Which primaryHand;
        internal Dictionary<Hands.Hand.Which, Hands.Hand> hands = new Dictionary<Hands.Hand.Which, Hands.Hand> { };

        internal void Init(VDE vde)
        {
            hud = vde.hud;
            data = vde.data;
            primaryHand = vde.primaryHand;
#if !MRET
            if (!(hud is null) && !(hud.cameraController is null))
            {
                cameraController = hud.cameraController;
            }
#endif
            log = new Log("Controller", data.messenger);
        }
        private void Start()
        {
            //viveObserver = gameObject.AddComponent<ViveObserver>();
            //viveObserver.inputEvent.AddListener(InputHandler);

            if (!TryGetComponent(out inputObserver))
            {
                inputObserver = gameObject.AddComponent<InputObserver>();
            }
            inputObserver.inputEvent.AddListener(InputHandler);

            if (!TryGetComponent(out cameraObserver))
            {
                cameraObserver = gameObject.AddComponent<CameraObserver>();
            }
            cameraObserver.inputEvent.AddListener(InputHandler);
            cameraObserver.data = data;
            cameraObserver.usableCamera = data.VDE.usableCamera;

            if (!(data.VDE.lazer is null))
            {
                data.VDE.lazer.inputEvent.AddListener(InputHandler);
            }
        }

        private void InputHandler(Event inputEvent)
        {
            switch (inputEvent.function)
            {
                case Event.Function.Move:
                    if (!(cameraController is null))
                    {
                        cameraController.Move(inputEvent.Vector2.y);
                    }
                    break;
                case Event.Function.Grip:
                    break;
                case Event.Function.DestroyTheMirror:
                    data.VDE.startupScreen.gameObject.SetActive(false);
                    break;
                case Event.Function.Grabbing:
                    if (hands.ContainsKey(primaryHand))
                    {
                        foreach(Shape stillHighlightedShape in data.entities.entities.Where(ent => ent.Value.enlightened).Select(ent => ent.Value.containers.GetCurrentShape()))
                        {
                            if (!(stillHighlightedShape is null))
                            {
                                stillHighlightedShape.LostFocus();
                            }
                        }
                        data.VDE.controller.hands[primaryHand].Grabbing(inputEvent.Bool);
                    }
                    break;
                case Event.Function.Select:
                    if (!(data.entities.NodeInFocus is null))
                    {
                        data.links.SetAllLinkStatusTo(false);
                        data.entities.NodeInFocus.Select();
                    } 
                    break;
                case Event.Function.PointAt:
                    if (inputEvent.type == Event.Type.Bool && !inputEvent.Bool)
                    {
                        data.entities.NodeInFocus = null;
                    } 
                    else if (inputEvent.type == Event.Type.GameObject && !(inputEvent.GameObject is null))
                    {
                        data.entities.PointAt(inputEvent.GameObject);
                    }
                    break;
                case Event.Function.GazingAt:
                    {
                        data.messenger.Post(new Communication.Message()
                        {
                            EntityEvent = Entities.Event.GotFocus,
                            from = inputEvent.Entity,
                            to = inputEvent.Entity
                        });
                    }                    
                    break;
                case Event.Function.GazePoint:
                    break;
                case Event.Function.GazeDirection:
                    break;
                case Event.Function.ExportObjectWithCoordinates:
                    data.entities.ExportShapesAndPositions();
                    data.links.Export();
                    break;
                case Event.Function.ToggleEdges:
                    data.links.SetAllLinkStatusTo(inputEvent.Bool);
                    break;
                case Event.Function.PositionVDE:
                    data.VDE.SetPositionAndScale(inputEvent.Vector3, data.VDE.localScaleInPreviousFrame);
                    break;
                case Event.Function.ScaleVDE:
                    data.VDE.SetPositionAndScale(data.VDE.transform.position, inputEvent.Vector3);
                    break;
                case Event.Function.ToggleLabels:
                    data.layouts.current.labelsVisible = inputEvent.Bool;
                    foreach (Entity entity in data.entities.entities.Values)
                    {
                        if (entity.containers.GetContainer(out Container container) && !(container.label is null))
                        {
                            container.SetLabelState(inputEvent.Bool);
                        }
                    }
                    break;
                case Event.Function.ToggleNotifications:
                    data.VDE.hud.ToggleNotifications();
                    break;
                case Event.Function.ToggleHUD:
                    data.VDE.hud.Toggle();
                    break;
                case Event.Function.SwitchSki:
                    //data.VDE.SkiBoxController.SetToNextSkibox();
                    break;
                case Event.Function.SwitchMaterial:
                    data.UI.SetToNextMaterial();
                    break;
                case Event.Function.UpdateShapes:
                    foreach (Group.Shape item in data.entities.entities.Where(ent => 
                        ent.Value.type == Entity.Type.Group).OrderByDescending(ent => 
                            ent.Value.distanceFromGroot).Select(ent => 
                                ent.Value.containers.GetCurrentGroupShape()))
                    {
                        if (!(item is null))
                        {
                            item.UpdateShape();
                        }
                    }
                    break;
                case Event.Function.SetDirectors:
                    foreach (Entity entity in data.entities.GetGroup(data.entities.Get(0)))
                    {
                        entity.containers.AddDirector(data.entities.Get(0).containers.GetContainer().gameObject);
                    }
                    break;
                case Event.Function.UpdateLinks:
                    StartCoroutine(data.links.UpdateLinks());
                    break;
                case Event.Function.LazerState:
                    data.VDE.lazer.laserBeamTransform.gameObject.SetActive(inputEvent.Bool);
                    break;
                case Event.Function.Exit:
                    data.VDE.Quit();
                    break;
                default:
                    break;
            }
        }

        internal void SetNotificationText(string text)
        {
            Hands.Hand notificationHand = hands.FirstOrDefault(hand => hand.Value.hasNotification).Value;
            if (!(notificationHand is null))
            {
                notificationHand.SetNotificationText(text);
            }
        }
    }
}
