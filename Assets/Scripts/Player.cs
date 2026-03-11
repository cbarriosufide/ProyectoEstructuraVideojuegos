using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	public static Player current;
	public Rigidbody rigidbody;

	const float MOVE_SPEED = 10.0f;
	const float JUMP_SPEED = 24.0f;

	public float moveDirection;
	public bool grounded;
	public int facing = 1;
	public bool dashing = false;

	public Action movementMode;

	[SerializeField]
	public ParticleSystem jumpParticle;

	private void Awake()
	{
		current = this;
		rigidbody = GetComponent<Rigidbody>();
		movementMode = MovementGrounded;
	}

	private void FixedUpdate()
	{
		Movement();
	}

	private void Movement()
	{
		grounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

		movementMode?.Invoke();
	}

	private void MovementGrounded()
	{
		if (!grounded)
		{
			movementMode = MovementAir;
			MovementAir();
		}
		SetVelocityX(Mathf.MoveTowards(rigidbody.velocity.x, moveDirection * MOVE_SPEED, 75f * Time.fixedDeltaTime));
	}

	private void MovementAir()
	{
		if (grounded)
		{
			movementMode = MovementGrounded;
			MovementGrounded();
		}
		SetVelocityX(Mathf.MoveTowards(rigidbody.velocity.x, moveDirection * MOVE_SPEED, 62f * Time.fixedDeltaTime));
	}

	private void MovementDashing()
	{
		SetVelocityX(Mathf.MoveTowards(rigidbody.velocity.x, 0, 90f * Time.fixedDeltaTime));
	}

	void OnJump()
	{
		if (!grounded) { return; }
		if (dashing) ExitDash();

		SetVelocityY(JUMP_SPEED);
		jumpParticle.Play();
	}

	void SetVelocityX(float velocityX)
	{
		rigidbody.velocity = new Vector3(velocityX, rigidbody.velocity.y, rigidbody.velocity.z);
	}

	void SetVelocityY(float velocityY)
	{
		rigidbody.velocity = new Vector3(rigidbody.velocity.x, velocityY, rigidbody.velocity.z);
	}

	void SetVelocityXY(float velocityX, float velocityY)
	{
		rigidbody.velocity = new Vector3(velocityX, velocityY, 0);
	}

	void OnMove(InputValue value)
	{
		Vector2 direction = value.Get<Vector2>();
		moveDirection = direction.x;
		facing = (int)Mathf.Sign(direction.x);
	}

	void OnDash()
	{
		if (dashing) return;

		StartCoroutine("OnDashCoroutine");
	}

	void EnterDash()
	{

		SetVelocityXY(MOVE_SPEED * 3.6f * facing, 0);
		jumpParticle.Play();
		grounded = false;
		dashing = true;
		rigidbody.useGravity = false;
	}

	void ExitDash()
	{
		dashing = false;
		rigidbody.useGravity = true;
		movementMode = MovementAir;
		StopCoroutine("OnDashCoroutine");
	}

	IEnumerator OnDashCoroutine()
	{
		EnterDash();
		yield return new WaitUntil(() => Mathf.Abs(rigidbody.velocity.x) <= MOVE_SPEED * 1.5);
		ExitDash();
	}

}
