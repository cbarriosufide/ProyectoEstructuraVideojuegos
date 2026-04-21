using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameCamera : MonoBehaviour
{
	public const float DEFAULT_CAM_DISTANCE = 21f;

	public static GameCamera current;
	public AudioLowPassFilter lowPassFilter;

	public Func<Vector3> mode;
	Vector3 velocity;
	public float camDistance = DEFAULT_CAM_DISTANCE;
	public float yOffset = 2f;


	void Awake()
	{
		current = this;

		// Si Player.current aún no está inicializado, búscalo
		if (Player.current == null)
		{
			Player player = FindObjectOfType<Player>();
			if (player != null)
				Player.current = player;
		}
	}

	private void Start()
	{
		lowPassFilter = GetComponent<AudioLowPassFilter>();
	}

	private void Update()
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

	public Vector3 ModeFixed()
	{
		return new Vector3(0, yOffset, -camDistance);
	}

	public void LowPassImpact(float time)
	{
		StopCoroutine(LowPassFilterCoroutine(time));
		StartCoroutine(LowPassFilterCoroutine(time));
	}

	IEnumerator LowPassFilterCoroutine(float delay)
	{
		lowPassFilter.cutoffFrequency = 500;

		yield return new WaitForSecondsRealtime(delay);

		while (lowPassFilter.cutoffFrequency < 22000)
		{
			lowPassFilter.cutoffFrequency = Mathf.MoveTowards(lowPassFilter.cutoffFrequency, 22000, 500);
			yield return new WaitForSecondsRealtime(0.1f);
		}
	}
}
