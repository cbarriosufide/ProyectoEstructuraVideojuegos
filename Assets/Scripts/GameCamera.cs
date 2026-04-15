using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameCamera : MonoBehaviour
{
	public static GameCamera current;

	public Func<Vector3> mode;
	Vector3 velocity;
	public float camDistance = 21f;
	public float yOffset = 2f;


	void Awake()
	{
		current = this;
		mode = ModeFollowPlayer;
		
		// Si Player.current aún no está inicializado, búscalo
		if (Player.current == null)
		{
			Player player = FindObjectOfType<Player>();
			if (player != null)
				Player.current = player;
		}
	}

	private void LateUpdate()
	{
		if (mode != null)
		{
			transform.position = Vector3.SmoothDamp(transform.position, mode(), ref velocity, 0.16f);
		}
	}

	public Vector3 ModeFollowPlayer()
	{
		if (Player.current == null)
			return transform.position;
		
		return Player.current.transform.position + new Vector3(0, yOffset, -camDistance);
	}
}
