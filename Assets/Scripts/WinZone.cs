using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinZone : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == Player.current.gameObject)
		{
			Player.current.Win();
		}
	}
}
