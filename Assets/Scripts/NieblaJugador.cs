using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NieblaJugador : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (Player.current != null)
		{
			transform.position = Player.current.transform.position + Vector3.forward * 18f;
		}

	}
}
