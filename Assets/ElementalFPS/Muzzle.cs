using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muzzle : MonoBehaviour 
{
	public Vector3 GetMuzzlePosition() 
    {
		return transform.position;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position, 0.01f);
	}

}
