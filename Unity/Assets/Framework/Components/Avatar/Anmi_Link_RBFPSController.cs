// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityStandardAssets.CrossPlatformInput;
//using UnityStandardAssets.Characters.FirstPerson;

public class Anmi_Link_RBFPSController : MonoBehaviour
{
    public Animation_link m_AnimationLink;
    //public RigidbodyFirstPersonController m_controllerLink; I presume Jon is updating this.
    public Camera cam;

    //Added for animation movement
    private Vector3 m_CamForward;
    private Vector3 m_Move;


    // Update is called once per frame
    void FixedUpdate()
    {
        //Vector2 input = GetInput();
        //Added for animation
        //m_CamForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        //m_Move = input.y * m_CamForward + CrossPlatformInputManager.GetAxis("Mouse X") * cam.transform.right;
        //m_AnimationLink.Move(m_Move, m_controllerLink.Grounded);
    }


    private Vector2 GetInput()
    {

        Vector2 input = new Vector2
        {
            //x = CrossPlatformInputManager.GetAxis("Horizontal"),
            //y = CrossPlatformInputManager.GetAxis("Vertical")
        };
        return input;
    }
}
