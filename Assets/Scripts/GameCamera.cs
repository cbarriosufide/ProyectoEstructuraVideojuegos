using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameCamera : MonoBehaviour
{
	public static GameCamera current;

	public Func<Vector3> mode;
	public Vector3 velocity;


	void Awake()
	{
		current = this;
		mode = ModeFollowPlayer;


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
		return Player.current.transform.position + new Vector3(0, 4, -14);
	}
}
