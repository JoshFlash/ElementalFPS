using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ElementalFPS.Characters.Player
{
	//head bob will have to be included in a later version of ElementalFPS
	//FOV Kick will have to be implemented later as well if desired

		/// <summary>
		/// Thigngs to fix:
		/// Can control movement in midair - should be changed to pitch or no control depending on desired resut
		/// can trigger next jump whie jumping which causes immediate jump upon landing - super annoying
		/// also the jump might be more intuitive like slower up faster down
		/// </summary>

	public class FPSController : MonoBehaviour
	{
		
		[SerializeField]		private bool isWalking;
		[SerializeField]		private float walkSpeed;
		[SerializeField]		private float runSpeed;
		[SerializeField]		private float jumpSpeed;
		[SerializeField]		private float stickToGroundForce;
		[SerializeField]		private float gravityMultiplier;
		[SerializeField]		private SmoothMouseLook smoothMouseLook;

		private Camera mainCamera;
		private bool pressedJump;
		private bool isJumping;
		private bool wasPreviouslyGrounded;

		private Vector2 m_Input;					//rename this once you figure out what it means
		private Vector3 m_MoveDir = Vector3.zero;	//rename this too

		private CharacterController characterController;
		private CollisionFlags collisionFlags;
		private AudioSource audioSource;



		void Start()
		{
			characterController = GetComponent<CharacterController>();
			mainCamera = Camera.main;
			isJumping = false;
			audioSource = GetComponent<AudioSource>();
			smoothMouseLook.Init(transform, mainCamera.transform);
		}

		// Update is called once per frame
		private void Update()
		{
			RotateView();
			HandleJumping();

			// the jump state needs to read here to make sure it is not missed

		}

		private void RotateView()
		{
			smoothMouseLook.LookRotation(transform, mainCamera.transform);
		}

		private void HandleJumping()
		{
			//THIS (default) JUMP CODE IS GARBAGE - FIX IT
			if (!pressedJump)
			{
				pressedJump = CrossPlatformInputManager.GetButtonDown("Jump");
			}
			if (!wasPreviouslyGrounded && characterController.isGrounded)
			{
				m_MoveDir.y = 0f;
				isJumping = false;
			}
			if (!characterController.isGrounded && !isJumping && wasPreviouslyGrounded)
			{
				m_MoveDir.y = 0f;
			}
			wasPreviouslyGrounded = characterController.isGrounded;
		}

		private void FixedUpdate()
		{
			float speed;
			GetInput(out speed);
			// always move along the camera forward as it is the direction that it being aimed at
			Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

			// get a normal for the surface that is being touched to move along it
			RaycastHit hitInfo;
			Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
							   characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
			desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

			m_MoveDir.x = desiredMove.x * speed;
			m_MoveDir.z = desiredMove.z * speed;


			if (characterController.isGrounded)
			{
				m_MoveDir.y = -stickToGroundForce;

				if (pressedJump)
				{
					m_MoveDir.y = jumpSpeed;
					pressedJump = false;
					isJumping = true;
				}
			}
			else
			{
				m_MoveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
			}
			collisionFlags = characterController.Move(m_MoveDir * Time.fixedDeltaTime);

			//ProgressStepCycle(speed);
			UpdateCameraPosition(speed);

			smoothMouseLook.UpdateCursorLock();
		}

		private void UpdateCameraPosition(float speed)
		{
			//code for camera position due to head-bob goes here
		}

		private void GetInput(out float speed)
		{
			// Read input
			float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
			float vertical = CrossPlatformInputManager.GetAxis("Vertical");

			bool waswalking = isWalking;

#if !MOBILE_INPUT
			// Keeps track of whether character is walkig or running -  should be modified to include input from GamePads as well
			isWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
			// set the desired speed to be walking or running
			speed = isWalking ? walkSpeed : runSpeed;
			m_Input = new Vector2(horizontal, vertical);

			// normalize input if it exceeds 1 in combined length:
			if (m_Input.sqrMagnitude > 1)
			{
				m_Input.Normalize();
			}
		}

		private void OnControllerColliderHit(ControllerColliderHit hit)
		{
			Rigidbody body = hit.collider.attachedRigidbody;
			//dont move the rigidbody if the character is on top of it
			if (collisionFlags == CollisionFlags.Below)
			{
				return;
			}

			if (body == null || body.isKinematic)
			{
				return;
			}
			body.AddForceAtPosition(characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
		}
	}

}
