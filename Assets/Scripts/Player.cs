using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	public enum Facing
	{
		LEFT = -1,
		RIGHT = 1
	}

	public static Player current;
	Rigidbody rigidbody;
	Animator animator;
	SkinnedMeshRenderer renderer;

	const float MOVE_SPEED = 10.0f;
	const float JUMP_SPEED = 24.0f;
	const int MAX_AIR_DASHES = 1;
	const float JUMP_CHARGE_SPEED = 9.0f;

	float moveDirection;
	bool grounded;
	public Facing facing = Facing.RIGHT;
	bool dashing = false;
	int airDashesLeft = MAX_AIR_DASHES;
	bool jumping = false;

	bool jumpCharging = false;
	bool jumpChargingFinished = false;
	float maxJumpCharge = 15.0f;
	float jumpCharge = 0;


	public int hp = 5;


	public Action movementMode;

	public ParticleSystem jumpParticle;
	public ParticleSystem dashParticle;
	public ParticleSystem jumpChargingParticle;
	public ParticleSystem jumpChargedReleaseParticle;
	public ParticleSystem jumpChargingComplete;

	public AudioSource sfxHurt;

	private void Awake()
	{
		current = this;
		rigidbody = GetComponent<Rigidbody>();
		renderer = GetComponentInChildren<SkinnedMeshRenderer>();

		jumpParticle = transform.Find("JumpParticle").GetComponent<ParticleSystem>();
		dashParticle = transform.Find("DashParticle").GetComponent<ParticleSystem>();
		jumpChargingParticle = transform.Find("JumpChargingParticle").GetComponent<ParticleSystem>();
		jumpChargedReleaseParticle = transform.Find("JumpChargedReleaseParticle").GetComponent<ParticleSystem>();
		jumpChargingComplete = transform.Find("JumpChargingCompleteParticle").GetComponent<ParticleSystem>();

		sfxHurt = transform.Find("SFXHurt").GetComponent<AudioSource>();

		animator = GetComponent<Animator>();


		movementMode = MovementGrounded;
	}

	private void FixedUpdate()
	{
		Movement();
	}

	private void Movement()
	{
		grounded = Physics.Raycast(transform.position + Vector3.up * 0.02f, Vector3.down, 1f);
		animator.SetBool("Grounded", grounded);
		animator.SetFloat("MoveSpeed", Mathf.Abs(rigidbody.velocity.x));

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
		transform.eulerAngles = new Vector3(0, facing == Facing.RIGHT ? 90 : -90, 0);
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
		transform.eulerAngles = new Vector3(0, facing == Facing.RIGHT ? 90 : -90, 0);

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

	private void MovementKnockback()
	{
		SetVelocityX(Mathf.MoveTowards(rigidbody.velocity.x, 0, 90f * Time.fixedDeltaTime));
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

		float particleScale = 1 + jumpCharge / 5f;

		jumpParticle.transform.localScale = new Vector3(particleScale, particleScale, particleScale);
		jumpParticle.Play();
		jumping = true;

		if (jumpCharging)
		{
			ResetJumpCharge();
			AnimateHeight(1f);
		}
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

		if (pressed)
		{
			StartJumpCharge();
			AnimateHeight(0.5f);
		}
		else
		{
			ResetJumpCharge();
			AnimateHeight(1f);
		}

	}


	void AnimateHeight(float finalHeight)
	{
		StopCoroutine("_AnimateHeightCoroutine");
		StartCoroutine("_AnimateHeightCoroutine", finalHeight);
	}

	IEnumerator _AnimateHeightCoroutine(float finalHeight)
	{
		float compensation = (1 - finalHeight) / 2;
		Vector3 finalScale = new Vector3(1 + compensation, finalHeight, 1 + compensation);

		while (Mathf.Abs(transform.localScale.y - finalHeight) > 0.01f)
		{
			transform.localScale = Vector3.Lerp(transform.localScale, finalScale, 0.3f);
			yield return new WaitForFixedUpdate();
		}

		transform.localScale = finalScale;
	}

	void StartJumpCharge()
	{
		jumpChargingParticle.Play();
		movementMode = MovementJumpCharging;
		jumpCharging = true;
	}

	void ResetJumpCharge()
	{
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

		if (direction.x != 0)
		{
			facing = (Facing)Mathf.Sign(direction.x);
		}
	}

	void OnDash()
	{
		if (dashing || airDashesLeft <= 0 || jumpCharging) return;

		StartCoroutine("OnDashCoroutine");
	}

	IEnumerator OnDashCoroutine()
	{
		EnterDash();
		yield return new WaitUntil(() => Mathf.Abs(rigidbody.velocity.x) <= MOVE_SPEED * 1.5);
		ExitDash();
	}

	void EnterDash()
	{

		SetVelocityXY(MOVE_SPEED * 3.6f * (int)facing, 0);
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


	public void Hurt(int damage)
	{
		hp -= damage;

		if (hp <= 0)
		{
			Die();
		}

		StopCoroutine("_HurtCoroutine");
		StartCoroutine("_HurtCoroutine");
	}

	IEnumerator _HurtCoroutine()
	{
		sfxHurt.Play();
		GameCamera.current.LowPassImpact(0.8f);

		renderer.material.color = Color.red;
		renderer.materials[1].color = Color.red;
		Time.timeScale = 0.0f;

		yield return new WaitForSecondsRealtime(0.1f);

		renderer.material.color = Color.black;
		renderer.materials[1].color = Color.black;

		while (Time.timeScale < 1.0f)
		{
			Time.timeScale = Mathf.MoveTowards(Time.timeScale, 1.0f, 0.1f);
			yield return new WaitForSecondsRealtime(0.01f);
		}

	}

	public void Knockback(Vector3 direction, float force)
	{
		StopCoroutine("_KnockbackCoroutine");
		StartCoroutine(_KnockbackCoroutine(direction, force));
	}

	IEnumerator _KnockbackCoroutine(Vector3 direction, float force)
	{
		rigidbody.useGravity = false;
		movementMode = MovementKnockback;

		SetVelocityXY(direction.x * force, direction.y * force);
		yield return new WaitForSeconds(0.08f);
		SetVelocityXY(0, 0);

		rigidbody.useGravity = true;
		movementMode = MovementAir;
	}

	void Die()
	{
		hp = 0;

		StopCoroutine("_DieCoroutine");
		StartCoroutine("_DieCoroutine");
	}

	IEnumerator _DieCoroutine()
	{
		Time.timeScale = 0.0f;

		yield return new WaitForSecondsRealtime(0.5f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

		Time.timeScale = 1.0f;

	}

}
