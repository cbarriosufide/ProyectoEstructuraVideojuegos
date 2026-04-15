using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject == Player.current.gameObject)
		{
			Player.current.Knockback(-other.GetContact(0).normal, 40f);
			Player.current.Hurt(1);
		}
	}
}
