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
	const int MAX_AIR_DASHES = 1;
	const float JUMP_CHARGE_SPEED = 6.0f;

	public float moveDirection;
	public bool grounded;
	public int facing = 1;
	public bool dashing = false;
	public int airDashesLeft = MAX_AIR_DASHES;
	public bool jumping = false;

	public bool jumpCharging = false;
	public bool jumpChargingFinished = false;
	float maxJumpCharge = 15.0f;
	public float jumpCharge = 0;

	public Action movementMode;

	[SerializeField]
	public ParticleSystem jumpParticle;

	[SerializeField]
	public ParticleSystem dashParticle;

	[SerializeField]
	public ParticleSystem jumpChargingParticle;

	[SerializeField]
	public ParticleSystem jumpChargedReleaseParticle;

	[SerializeField]
	public ParticleSystem jumpChargingComplete;
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

		SetVelocityX(moveDirection * MOVE_SPEED);
	}

	private void MovementAir()
	{
		if (grounded)
		{
			movementMode = MovementGrounded;
			airDashesLeft = MAX_AIR_DASHES;
			MovementGrounded();
		}

		SetVelocityX(moveDirection * MOVE_SPEED);
	}

	private void MovementDashing()
	{
		SetVelocityX(Mathf.MoveTowards(rigidbody.velocity.x, 0, 90f * Time.fixedDeltaTime));
	}

	private void MovementJumpCharging()
	{
		SetVelocityX(Mathf.MoveTowards(rigidbody.velocity.x, 0, 90f * Time.fixedDeltaTime));

		if (jumpChargingFinished) return;

		jumpCharge += Time.deltaTime * JUMP_CHARGE_SPEED;

		if (jumpCharge >= maxJumpCharge)
		{
			jumpChargingFinished = true;
			jumpChargingParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			jumpChargingComplete.Play();
		}

		jumpCharge = Mathf.Clamp(jumpCharge, 0, maxJumpCharge);
	}

	void OnJump(InputValue value)
	{
		if (!value.isPressed)
		{
			OnJumpCanceled();
			return;
		}

		if (!grounded) { return; }
		if (dashing) ExitDash();


		if (jumpCharge == maxJumpCharge)
		{
			jumpChargedReleaseParticle.Play();
		}

		SetVelocityY(JUMP_SPEED + jumpCharge);
		jumpParticle.Play();
		jumping = true;

		if (jumpCharging) ResetJumpCharge();
	}

	void OnJumpCanceled()
	{
		Debug.Log($"{rigidbody.velocity.y}  {jumping}");
		if (!jumping) return;
		jumping = false;

		if (rigidbody.velocity.y <= 5) return;

		Debug.Log("Jump canceled");
		SetVelocityY(rigidbody.velocity.y / 3);

	}

	void OnCrouch(InputValue value)
	{
		if (!grounded) return;

		bool pressed = value.isPressed;

		if (pressed) StartJumpCharge();
		else ResetJumpCharge();

	}

	void StartJumpCharge()
	{
		Debug.Log("starting charge jump");
		jumpChargingParticle.Play();
		movementMode = MovementJumpCharging;
		jumpCharging = true;
	}

	void ResetJumpCharge()
	{
		Debug.Log($"charge jump {jumpCharge}");
		jumpCharging = false;
		jumpChargingFinished = false;
		jumpCharge = 0;
		jumpChargingParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		movementMode = MovementAir;
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
		if (dashing || airDashesLeft <= 0) return;

		StartCoroutine("OnDashCoroutine");
	}

	void EnterDash()
	{

		SetVelocityXY(MOVE_SPEED * 3.6f * facing, 0);
		dashParticle.Play();
		grounded = false;
		dashing = true;
		rigidbody.useGravity = false;
		movementMode = MovementDashing;
		airDashesLeft--;
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
