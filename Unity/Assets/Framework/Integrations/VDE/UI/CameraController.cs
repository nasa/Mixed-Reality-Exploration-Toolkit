/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.UI.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
 
namespace Assets.VDE.UI
{
    public class CameraController : MonoBehaviour
    {
        internal float
            acCeleration,
            deCeleration = 0.1F;
        float
            deadJoy = 0.02F,
            sheerJoy = 0.95F,
            speedForward = 0F,
            speedBackward = 0F,
            speedSideways = 0F,
            rotationYspeed = 0F,
            rotationXspeed = 0F,
            currentTopspeed = 0.5F;
        internal float
            currentBooster = 10F,
            defaultBooster = 10F;
        internal Demo demo;

        public Camera referenceCamera = null;
        public enum CameraAxises { rudder, x6DOF }
        public CameraAxises cameraMovementType = CameraAxises.rudder;

        /// <summary>
        /// this function creates camera controller object so that the user could "drive" the camera;
        /// it also enables the usage of a "demo", to automagically control the camera's positioning in the scene.
        /// </summary>
        void Start()
        {
            if (referenceCamera is null)
            {
                referenceCamera = UnityEngine.Camera.main;
            }
        }
       
        internal void Move(float acCeleration = 1F)
        {
            this.acCeleration = acCeleration;
        }
        void Update()
        {
            // automagical positioning and moving of the camera, following the positions and transfers read from a conf.
            if (demo != null && demo.targetStop != null && demo.active)
            {
                demo.Move(transform, currentBooster, defaultBooster);
            }
            else
            {
                // forwards
                if (acCeleration > deadJoy)
                {
                    if (speedForward < currentTopspeed)
                    {
                        if (acCeleration > sheerJoy)
                        {
                            speedForward += acCeleration / 30;
                        }
                        else
                        {
                            speedForward += acCeleration / 80;
                        }
                    }

                    if (acCeleration > deCeleration)
                    {
                        acCeleration -= deCeleration;
                    } 
                    else
                    {
                        acCeleration = 0;
                    }
                }
                else if (speedForward > 0 && speedForward > 0.01F)
                {
                    speedForward -= speedForward / 5;
                    acCeleration -= deCeleration / 5;
                }

                // backwards
                if (acCeleration < 0-deadJoy)
                {
                    if (speedBackward < currentTopspeed)
                    {
                        if (acCeleration < -sheerJoy)
                        {
                            speedBackward += Math.Abs(acCeleration) / 30;
                        }
                        else
                        {
                            speedBackward += Math.Abs(acCeleration) / 80;
                        }
                    }

                    if (Math.Abs(acCeleration) < deCeleration)
                    {
                        acCeleration += deCeleration;
                    }
                    else
                    {
                        acCeleration = 0;
                    }
                }
                else if (speedBackward > 0 && speedBackward > 0.01F)
                {
                    speedBackward -= speedBackward / 5;
                    acCeleration += deCeleration / 5;
                }
                
                if((speedBackward > 0 || speedForward > 0) && speedBackward < 0.01F && speedForward < 0.01F)
                {
                    acCeleration = speedBackward = speedForward = 0;
                }

                //Debug.Log("speedForward : " + speedForward.ToString() + "speedBackward: " + speedBackward.ToString());
                /*
                // for controlling the "observers camera" with xbox controller.
                if (cameraMovementType == CameraAxises.x6DOF)
                {
                    // move leftwards
                    if (input.controllers[inputDevice].joysticAxis.x < -deadJoy)
                    {
                        if (speedSideways > -currentTopspeed)
                            speedSideways += input.controllers[inputDevice].joysticAxis.x / 10;
                    }
                    // move rightwards
                    else if (input.controllers[inputDevice].joysticAxis.x > deadJoy)
                    {
                        if (speedSideways < currentTopspeed)
                            speedSideways += input.controllers[inputDevice].joysticAxis.x / 10;
                    }
                    else if (speedSideways > 0.01F)
                    {
                        speedSideways -= speedSideways / 10;
                    }
                    else if (speedSideways < 0.01F)
                    {
                        speedSideways -= speedSideways / 10;
                    }
                    else
                    {
                        speedSideways = 0;
                    }

                    // look up
                    if (input.controllers[inputDevice].auxilaryAxis.y < -deadJoy)
                    {
                        if (rotationYspeed > -currentTopspeed)
                        {
                            rotationYspeed += input.controllers[inputDevice].auxilaryAxis.y / 10;
                        }
                    }
                    // look down
                    else if (input.controllers[inputDevice].auxilaryAxis.y > deadJoy)
                    {
                        if (rotationYspeed < currentTopspeed)
                        {
                            rotationYspeed += input.controllers[inputDevice].auxilaryAxis.y / 10;
                        }
                    }
                    else if (rotationYspeed > 0.03F)
                    {
                        rotationYspeed -= rotationYspeed / 10;
                    }
                    else if (speedSideways < 0.03F)
                    {
                        rotationYspeed -= rotationYspeed / 10;
                    }
                    else
                    {
                        rotationYspeed = 0;
                    }

                    // turn left
                    if (input.controllers[inputDevice].auxilaryAxis.x < -deadJoy)
                    {
                        if (rotationXspeed > -currentTopspeed)
                        {
                            rotationXspeed += input.controllers[inputDevice].auxilaryAxis.x / 10;
                        }
                    }
                    // turn right
                    else if (input.controllers[inputDevice].auxilaryAxis.x > deadJoy)
                    {
                        if (rotationXspeed < currentTopspeed)
                        {
                            rotationXspeed += input.controllers[inputDevice].auxilaryAxis.x / 10;
                        }
                    }
                    else if (rotationXspeed > 0.03F)
                    {
                        rotationXspeed -= rotationXspeed / 10;
                    }
                    else if (rotationXspeed < 0.03F)
                    {
                        rotationXspeed -= rotationXspeed / 10;
                    }
                    else
                    {
                        rotationXspeed = 0;
                    }
                }
                */
            }
            try
            {
                if (speedForward > 0F)
                {
                    transform.position += referenceCamera.transform.forward * Time.deltaTime * speedForward * currentBooster;
                }
                if (speedBackward > 0F)
                {
                    transform.position -= referenceCamera.transform.forward * Time.deltaTime * speedBackward * currentBooster;
                }
                if (speedSideways < 0F || speedSideways > 0F)
                {
                    transform.position += referenceCamera.transform.right * Time.deltaTime * speedSideways * currentBooster;
                }
                if (cameraMovementType == CameraAxises.x6DOF)
                {
                    transform.RotateAround(transform.position, Vector3.left, rotationYspeed);
                    transform.RotateAround(transform.position, Vector3.up, rotationXspeed);
                    transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
                }
            }
            catch (Exception exe)
            {
                Debug.Log("A camera is acting up: " + exe.Message);
            }
        }
    }
}
