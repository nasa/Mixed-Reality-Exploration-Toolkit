// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Components.Locomotion;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem.SDK.Desktop;
using GSFC.ARVR.MRET.Infrastructure.CrossPlatformInputSystem;
using GSFC.ARVR.MRET.Infrastructure.Framework;

public class Animation_link : MonoBehaviour
{

	[SerializeField] float m_AnimSpeedMultiplier = 1f;

	Animator m_Animator;
	bool m_IsGrounded;

	float m_ForwardAmount;
	Vector3 m_GroundNormal;
	float m_TurnAmount;
	public GameObject PlayerController;
	public Camera cam;

	//Added for animation movement
	private Vector3 m_CamForward;
	private Vector3 m_Move;



	// Start is called before the first frame update
	void Start()
	{
		m_Animator = GetComponent<Animator>();

	}



	// Update is called once per frame
	/// <summary>
	/// This relys on the old unity input system because the values given back from activeHands[1].navigateValue isn't consistent
	/// this results in a behavior that makes the run animation only play in one direction.
	/// May need to revisit this to correct this bug in the future. 
	/// </summary>
	void FixedUpdate()
	{
		List<InputHand> activeHands = MRET.InputRig.hands;
		
		if(activeHands.Count > 0 && activeHands.Count <= 2)
        {
			Vector2 input = activeHands[1].navigateValue;
			//Added for animation
			m_CamForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;

			//TODO: UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetAxis("Mouse X") << This will need changed maybe?
			//Will need inputrig specific function that will allow for getting whether or not the mouse is moving left or right across the screen
			//based on a -1.0 to a 1.0 value

			//TODO: Look at events for input head so that you can tie to input head event to know if head moved across it's "axis" up/down left/right

			//Dicslaimer: UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager don't model after this
			m_Move = input.y * m_CamForward + UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetAxis("Mouse X") * cam.transform.right;
			transform.forward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
			Move(m_Move);
		}
		
	}


	/// <summary>
	/// This moves the values of the animator and then calls the update function
	/// </summary>
	/// <param name="move"></param>
	/// <param name="grounded"></param>
	/// <param name="crouch"></param>
	public void Move(Vector3 move, bool grounded = true, bool crouch = false)
	{

		m_IsGrounded = grounded;
		//m_IsGrounded = grounded;
		// convert the world relative moveInput vector into a local-relative
		// turn amount and forward amount required to head in the desired
		// direction.
		if (move.magnitude > 1f) move.Normalize();
		move = transform.InverseTransformDirection(move);
		move = Vector3.ProjectOnPlane(move, m_GroundNormal);
		m_TurnAmount = Mathf.Atan2(move.x, move.z);


		m_ForwardAmount = move.z / 2;




		// send input and other state parameters to the animator
		UpdateAnimator(move);
	}


	/// <summary>
	/// This function takes the move values and then proceedes to drive the animator with them with a basic set. 
	/// </summary>
	/// <param name="move"></param>
	public void UpdateAnimator(Vector3 move)
	{
		// update the animator parameters
		m_Animator.SetFloat("Forward", m_ForwardAmount, 0.05f, Time.deltaTime);
		m_Animator.SetFloat("Turn", m_TurnAmount, 0.05f, Time.deltaTime);
		m_Animator.SetBool("OnGround", m_IsGrounded);
		if (!m_IsGrounded)
		{
			//m_Animator.SetFloat("Jump", PlayerController.GetComponent<Rigidbody>().velocity.y);
		}


		// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
		// which affects the movement speed because of the root motion.
		if (m_IsGrounded && move.magnitude > 0)
		{
			m_Animator.speed = m_AnimSpeedMultiplier;
		}
		else
		{
			// don't use that while airborne
			m_Animator.speed = 1;
		}

	}
}

