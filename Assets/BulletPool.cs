using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalFPS.Combat.Core
{

	[RequireComponent(typeof(Firearm))]
	public class BulletPool : MonoBehaviour
	{

		private List<GameObject> primaryBulletPool;
		private List<GameObject> secondaryBulletPool;
		private Firearm fireArm;

		private void Awake()
		{
			fireArm = GetComponent<Firearm>();
			primaryBulletPool = new List<GameObject>();
			secondaryBulletPool = new List<GameObject>();
		}

		void Start()
		{
			int poolSize;
			poolSize = Mathf.Clamp(fireArm.primaryMagazineSize, 0, 400);
			for (int i = 0; i < poolSize; i++) {
				GameObject bullet = Instantiate(fireArm.primaryAmmo, PoolParent.current.transform);
				primaryBulletPool.Add(bullet);
				bullet.SetActive(false);
			}
			poolSize = Mathf.Clamp(fireArm.secondaryMagazineSize, 0, 400);
			for (int i = 0; i < poolSize * 2; i++) {
				GameObject bullet = Instantiate(fireArm.secondaryAmmo, PoolParent.current.transform);
				secondaryBulletPool.Add(bullet);
				bullet.SetActive(false);
			}
		}

		public GameObject GetPrimaryAmmo()
		{
			foreach (GameObject bullet in primaryBulletPool) {
				if (!bullet.activeInHierarchy) return bullet;
			}

			Debug.Log("More ammo was needed");
			GameObject newBullet = Instantiate(fireArm.primaryAmmo);
			primaryBulletPool.Add(newBullet);
			return newBullet;
		}

		public GameObject GetSecondaryAmmo()
		{
			foreach (GameObject bullet in secondaryBulletPool) {
				if (!bullet.activeInHierarchy) return bullet;
			}

			GameObject newBullet = Instantiate(fireArm.secondaryAmmo);
			secondaryBulletPool.Add(newBullet);
			return newBullet;
		}

	}

}
