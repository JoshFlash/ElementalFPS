using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalFPS.Combat.Core
{

	public abstract class Firearm : MonoBehaviour, IRangedWeapon
	{
		public abstract void FireSecondary();                   //secondary fire method to be implemented in subclass
		public abstract void FirePrimary();                     //primary fire method to be implemented in subclass

		public int primaryMagazineSize, secondaryMagazineSize;  //up to 400 will be available in bullet pool - additional ammo created on the fly
		public GameObject primaryAmmo, secondaryAmmo;           //bullet objects to use in respective fire mode - must have a bullet component
		public Camera playerCamera;                             //the camera associated with the player using the fireArm

		[SerializeField]	protected float maxRange = 500f;                        //maximum range the firearm can accurately aim (meters) when firing from gun
		[SerializeField]	protected float minRange = 5f;                          //minimum range the firearm can aim accurately when not zoomed
		[SerializeField]	protected float reloadTime = 0.5f;                      //the reload time (duh)
		[SerializeField]	protected float primaryFireRate, secondaryFireRate;		//respective shots per second in each  fire mode
		[SerializeField]	protected float muzzleVelocity;						    //speed of bullet as it leaves gun
		[SerializeField]	protected bool fireFromCamera = false;					//determines whether bullets instantiate at center of camera or at gun
		[SerializeField]	protected bool useSecondaryFire = false;                //weapons will generally have two fire modes

		protected int currentAmmoPrimary, currentAmmoSecondary; //bullets left in magazine before reload required
		protected bool isZoomed = false;                        //determines if the firearm is zoomed which will affect fireFromCamera
		protected BulletPool bulletPool;                        //pool for all bullets

		private Muzzle muzzle;
		private Vector3 muzzlePosition;                         //position of gun muzzle in world space as determined by muzzle game object

		private void Awake()
		{
			if (!playerCamera) playerCamera = Camera.main;
			bulletPool = GetComponent<BulletPool>() ?? gameObject.AddComponent<BulletPool>();
			muzzle = GetComponentInChildren<Muzzle>();
		}

		private void Start()
		{
			muzzlePosition = muzzle.GetMuzzlePosition();
		}

		public void Fire()
		{
			if (useSecondaryFire) { FireSecondary(); } else { FirePrimary(); }
		}

		public virtual void Reload()
		{
			//reload code goes here
		}

		protected void InstantiateBullet()
		{
			if (useSecondaryFire)
			{
				InitializeBullet(bulletPool.GetSecondaryAmmo());
				currentAmmoSecondary--;
			}
			else
			{
				InitializeBullet(bulletPool.GetPrimaryAmmo());
				currentAmmoPrimary--;
			}
		}

		protected virtual void InitializeBullet(GameObject goBullet)
		{
			Bullet bullet = goBullet.GetComponent<Bullet>();
			muzzlePosition = muzzle.GetMuzzlePosition();

			bullet.transform.position = fireFromCamera ? playerCamera.transform.position : muzzlePosition;
			bullet.velocity = fireFromCamera ? playerCamera.transform.forward * muzzleVelocity : GetFireDirection() * muzzleVelocity;

			goBullet.transform.rotation = transform.rotation * Quaternion.Euler(-( transform.localRotation.eulerAngles ));
			goBullet.SetActive(true);
		}

		private Vector3 GetFireDirection()
		{
			Ray cameraForward = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
			RaycastHit hit;
			Vector3 aimVector = cameraForward.direction;
			if (Physics.Raycast(cameraForward, out hit, minRange))
			{
				aimVector = playerCamera.transform.position + ( playerCamera.transform.forward * minRange ) - muzzlePosition;
				return Vector3.Normalize(aimVector);
			}
			if (Physics.Raycast(cameraForward, out hit, maxRange))
			{
				aimVector = hit.point - muzzlePosition;
			}
			return Vector3.Normalize(aimVector);
		}

	}

}
