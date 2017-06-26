using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ElementalFPS.Characters.Player
{
	//this is derived from the standard assets first person controller
	//changes to be made to simplify and improve functionality

	//head bob will have to be included in a later version of ElementalFPS
	//FOV Kick will have to be implemented later as well if desired

		/// <summary>
		/// Thigngs to fix:
		/// Can control movement in midair - should be changed to pitch or no control depending on desired result
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

		//the code relating to inputVector must be checked in order to ensure proper walk speeds
		//when it accepts gamepad inputs for any direction make sure normalises appropriately and adjusts speeds (only when walking)
		//that is, if a joystick is partially pressed down at 45 degree fwd it should adjust the speed to a slower 'walk'
		//all running should be done at the same speed regardless of joystick pressure (a 'triggered' run as is intuitive)
		//must review GetAxis when interneted - will examine later
		private Vector2 inputVector;

		private Vector3 moveDirection = Vector3.zero;
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
		}

		private void RotateView()
		{
			smoothMouseLook.LookRotation(transform, mainCamera.transform);
		}

		private void HandleJumping()
		{
			//corrected in-air jump triggering by adding '&& !isJumping' to the following line (conditional part)
			if (!pressedJump && !isJumping)
			{
				pressedJump = CrossPlatformInputManager.GetButtonDown("Jump");
			}
			if (!wasPreviouslyGrounded && characterController.isGrounded)
			{
				moveDirection.y = 0f;
				isJumping = false;
			}
			if (!characterController.isGrounded && !isJumping && wasPreviouslyGrounded)
			{
				moveDirection.y = 0f;
			}
			wasPreviouslyGrounded = characterController.isGrounded;
		}

		//refactor the fixedupdate code to better represent what is actually happeneing
		private void FixedUpdate()
		{
			float speed;
			GetInput(out speed);

			// desiredMove will have to be adjust as GetInput is rewritten to incorporate gamepad controllers
			//for now it looks the same
			Vector3 desiredMove;
			desiredMove = transform.forward * inputVector.y + transform.right * inputVector.x;

			// get a normal for the surface that is being touched to move along it
			RaycastHit hitInfo;
			Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
							   characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
			desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

			moveDirection.x = desiredMove.x * speed;
			moveDirection.z = desiredMove.z * speed;


			if (characterController.isGrounded)
			{
				moveDirection.y = -stickToGroundForce;

				if (pressedJump)
				{
					moveDirection.y = jumpSpeed;
					isJumping = true;
					pressedJump = false;
				}
			}
			else
			{
				moveDirection += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
			}
			collisionFlags = characterController.Move(moveDirection * Time.fixedDeltaTime);

			//ProgressStepCycle(speed);
			UpdateCameraPosition(speed);

			smoothMouseLook.UpdateCursorLock();
		}

		private void UpdateCameraPosition(float speed)
		{
			//code for camera position due to head-bob goes here
		}


		// this will have to be hugely rewritten to include gamepad controllers
		private void GetInput(out float speed)
		{
			// Read input
			float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
			float vertical = CrossPlatformInputManager.GetAxis("Vertical");

			bool wasWalking = isWalking;

#if !MOBILE_INPUT
			// Keeps track of whether character is walking or running
			//should be modified to include input from GamePads as well (not just LeftShift)
			isWalking = isJumping ? wasWalking : !Input.GetKey(KeyCode.LeftShift);
#endif
			// set the desired speed to be walking or running
			inputVector = new Vector2(horizontal, vertical);
			speed = isWalking ? walkSpeed : runSpeed;

			// normalize input if it exceeds 1 in combined length:
			if (inputVector.sqrMagnitude > 1)
			{
				inputVector.Normalize();
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
