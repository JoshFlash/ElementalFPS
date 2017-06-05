using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolParent : MonoBehaviour 
{
	public static PoolParent current;

	private void Awake()
	{
		if (current == null) current = this;
	}

}
