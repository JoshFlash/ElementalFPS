using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ElementalFPS.Combat.Core;

public class BurstGun : Firearm 
{
	public int burstFireRounds = 3;
	public float delayBetweenBurstRounds = 0.1f;
	public AudioSource audioSource;

	private float timeSinceLastFire;
	private int burstsLeft;

	private void Start()
	{
		//ensures that we start ready to fire
		timeSinceLastFire = (1/primaryFireRate) + (1/secondaryFireRate);
	}

	public override void FirePrimary() 
    {
		if (ReadyToFire()) {
			InstantiateBullet();
			audioSource.Play();
			timeSinceLastFire = 0f;
		}
	}

	public override void FireSecondary()
	{
		if (ReadyToFire()) {
			burstsLeft = burstFireRounds;
			InvokeRepeating("Burst", 0.001f, delayBetweenBurstRounds);
			timeSinceLastFire = 0f;
		}
	}

	private void Burst()
	{
		InstantiateBullet();
		audioSource.Play();
		burstsLeft--;
		if (burstsLeft <= 0) {
			CancelInvoke();
		}
	}

	private bool ReadyToFire()
	{
		if (useSecondaryFire && timeSinceLastFire > 1 / secondaryFireRate) return true;
		if (!useSecondaryFire && timeSinceLastFire > 1 / primaryFireRate) return true;
		return false;
	}

	public override void Reload()
	{
		base.Reload();
		if (useSecondaryFire) currentAmmoSecondary = secondaryMagazineSize; 
		if (!useSecondaryFire) currentAmmoPrimary = primaryMagazineSize;
	}

	private void Update()
	{
		timeSinceLastFire += Time.deltaTime;
		if (Input.GetMouseButtonDown(0)) {
			Fire();
		}
		if (Input.GetKeyDown(KeyCode.Q)) {
			useSecondaryFire = !useSecondaryFire;
		}
	}
}
