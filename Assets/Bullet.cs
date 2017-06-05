using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalFPS.Combat.Core
{

	public class Bullet : MonoBehaviour
	{

		/// <summary>
		/// 
		/// The bullet will cast a ray every update that will check for 'collisions'
		/// This raycast will determine whether there was anything in front of the bullet last frame which it will hit 
		/// and will be calculated based upon the velocity of the bullet, its length, and last frame time (Time.deltaTime).
		/// The bullet will then move forward and check the same area now behind it to check for one-sided mesh collision.
		/// 
		/// </summary> 


		[HideInInspector] public float damage;
		[HideInInspector] public Vector3 velocity;	// velocity of the bullet where direction is in world space

		public float mass;                          //mass of the bullet in kg
		public float length;                        //length of the bullet in meters
		//public List<OnHitEffect> onHitEffects;	//to be used for things like poisons, slows, bullet explosions, players catching fire, etc

		[SerializeField] private bool useGravity = true;      //determines whether the bullet will fall due to gravity
		[SerializeField] private float lifetime = 5f;

		private float timeAlive;
		private float tipRayLength, tailRayLength;			 //the distance to raycast in front and behind the tip of the bullet
		private float speed;
		private bool hasCollided = false;
		private Vector3 gravityForce;
		private RaycastHit hit;

		private void Start()
		{
			gravityForce = new Vector3(0, Physics.gravity.y, 0);
		}

		void OnEnable()
		{
			speed = Vector3.Magnitude(velocity);
			timeAlive = 0;
		}

		private void Update()
		{
			UpdateBulletPhysicsFwd();
			UpdateBulletPosition();
			UpdateBulletPhysicsBack();

			timeAlive += Time.deltaTime;
			if (timeAlive > lifetime) gameObject.SetActive(false);

			Debug.DrawRay(transform.position, transform.forward * tipRayLength, Color.blue);
			Debug.DrawRay(transform.position, -transform.forward * tailRayLength, Color.green);

		}

		private void UpdateBulletPosition()
		{
			transform.position += velocity * Time.deltaTime;
			if (useGravity) velocity += gravityForce * Time.deltaTime;
		}

		private void UpdateBulletPhysicsFwd()
		{
			tipRayLength = ( length / 2 ) + ( speed * Time.deltaTime );
			if (Physics.Raycast(transform.position, transform.forward, out hit, tipRayLength)) {
				if (hit.collider) {
					hasCollided = true;
					Debug.Log("Bullet will hit a collider in front.");
					HandleBulletCollision(hit);
				}
			}
		}

		private void UpdateBulletPhysicsBack()
		{
			tailRayLength = ( length / 2 ) + ( speed * Time.deltaTime );
			if (Physics.Raycast(transform.position, transform.forward * ( -1 ), out hit, tailRayLength)) {
				if (hit.collider && !hasCollided) {
					hasCollided = true;
					Debug.Log("Bullet hit a collider behind.");
					HandleBulletCollision(hit);
				}
			}
		}

		void HandleBulletCollision(RaycastHit hit)
		{
			if (hit.rigidbody) {
				hit.rigidbody.AddForceAtPosition(transform.TransformDirection(this.velocity) * mass, hit.point);
				Debug.Log("Hit a rigid body");
			}
			Debug.Log("Bullet Collison Handler Called");
			gameObject.SetActive(false);
			// slow, stop or ricochet and send damage to enemy
			// break glass?
			// disable bullet and stop coroutines
		}

	}

}
