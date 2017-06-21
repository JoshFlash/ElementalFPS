using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElementalFPS.Combat.Core
{

	// the Base Class for all Ranged Wepaons
	public interface IRangedWeapon
	{
		void Fire();
		void Reload();
	}

}
