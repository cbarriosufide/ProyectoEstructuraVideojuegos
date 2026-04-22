using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class enemy_fly : MonoBehaviour
{
	[Header("Patrullaje")]
	[SerializeField] private float patrolSpeed = 3f;
	[SerializeField] private float patrolDistance = 5f;
	[SerializeField] private float hoverHeight = 5f;

	[Header("Picada")]
	[SerializeField] private float diveSpeed = 12f;
	[SerializeField] private float detectionRange = 10f;
	[SerializeField] private float losePlayerRange = 15f;
	[SerializeField] private float collisionDelay = 0.5f;

	[Header("Vida")]
	public float health = 5f;
	public float maxHealth = 5f;

	private Rigidbody rb;
	private Transform player;
	private Animator animator;

	private Vector3 startPosition;
	private bool facingRight = true;
	private bool isDiving = false;
	private bool isInCollisionDelay = false;
	private float collisionDelayTimer = 0f;

	private float currentSpeed = 0;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		ConfigureRigidbody();
	}

	void Start()
	{
		startPosition = transform.position;
		health = maxHealth;

		GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
		if (playerObj != null)
			player = playerObj.transform;
		else
			Debug.LogWarning($"[{name}] No se encontró un GameObject con tag 'Player'.");
	}

	void FixedUpdate()
	{
		currentSpeed = 0f;

		if (isInCollisionDelay)
		{
			collisionDelayTimer -= Time.fixedDeltaTime;
			if (collisionDelayTimer <= 0f)
			{
				isInCollisionDelay = false;
			}
			return;
		}

		if (player == null)
		{
			Patrol();
			return;
		}

		float distanceToPlayer = Vector3.Distance(transform.position, player.position);

		if (!isDiving && distanceToPlayer <= detectionRange)
			isDiving = true;

		if (isDiving && distanceToPlayer > losePlayerRange)
			isDiving = false;

		if (isDiving)
			Dive();
		else
			Patrol();

		if (animator != null)
			animator.SetFloat("Speed", currentSpeed);
	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			Vector3 contactPoint = other.GetContact(0).point;
			Rigidbody playerRb = Player.current.GetComponent<Rigidbody>();
			
			float contactHeight = contactPoint.y - transform.position.y;
			float enemyHeight = GetComponent<Collider>().bounds.size.y;
			
			bool isOnTop = contactHeight > (enemyHeight * 0.5f);
			bool isFalling = playerRb != null && playerRb.velocity.y < 0f;
			
			if (isOnTop && isFalling)
			{
				TakeDamage(1f);
				Player.current.Knockback(Vector3.up, 20f);
			}
			else
			{
				Player.current.Knockback(-other.GetContact(0).normal, 40f);
				Player.current.Hurt(1);
			}
			
			if (health > 0f)
			{
				Vector3 knockbackDirection = (transform.position - player.position).normalized;
				rb.velocity = knockbackDirection * 5f;
			}
			
			isInCollisionDelay = true;
			collisionDelayTimer = collisionDelay;
			isDiving = false;
		}
	}

	private void Patrol()
	{
		float offsetX = transform.position.x - startPosition.x;

		if (offsetX >= patrolDistance)
			facingRight = false;
		else if (offsetX <= -patrolDistance)
			facingRight = true;

		currentSpeed = patrolSpeed;
		MoveHorizontally(facingRight ? 1f : -1f, patrolSpeed);
		MaintainHoverHeight();
	}

	private void Dive()
	{
		Vector3 directionToPlayer = (player.position - transform.position).normalized;
		currentSpeed = diveSpeed;
		
		rb.velocity = directionToPlayer * diveSpeed;
		UpdateFacingFromDirection(directionToPlayer.x);
	}

	private void MoveHorizontally(float direction, float speed)
	{
		rb.velocity = new Vector3(direction * speed, rb.velocity.y, 0f);
		UpdateFacing(direction);
	}

	private void MaintainHoverHeight()
	{
		float yDifference = startPosition.y - transform.position.y;

		if (Mathf.Abs(yDifference) > 0.5f)
		{
			float correction = Mathf.Sign(yDifference) * patrolSpeed;
			rb.velocity = new Vector3(rb.velocity.x, correction, 0f);
		}
		else
		{
			rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);
		}
	}

	private void UpdateFacing(float direction)
	{
		if (direction == 0f) return;

		float y = direction > 0f ? 90f : 270f;
		transform.rotation = Quaternion.Euler(0f, y, 0f);
	}

	private void UpdateFacingFromDirection(float directionX)
	{
		facingRight = directionX > 0f;
		float y = directionX > 0f ? 90f : 270f;
		transform.rotation = Quaternion.Euler(0f, y, 0f);
	}

	private void ConfigureRigidbody()
	{
		if (rb == null) return;

		rb.isKinematic = false;
		rb.mass = 1.5f;
		rb.drag = 0.5f;
		rb.angularDrag = 10f;
		rb.interpolation = RigidbodyInterpolation.Interpolate;
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

		rb.useGravity = false;

		rb.constraints =
			RigidbodyConstraints.FreezePositionZ |
			RigidbodyConstraints.FreezeRotationX |
			RigidbodyConstraints.FreezeRotationY |
			RigidbodyConstraints.FreezeRotationZ;
	}

	public void TakeDamage(float damage)
	{
		health -= damage;
		if (health <= 0f)
		{
			Die();
		}
	}

	private void Die()
	{
		if (animator != null)
			animator.enabled = false;
			
		if (rb != null)
			rb.isKinematic = true;
			
		Destroy(gameObject);
	}

#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Vector3 pos = Application.isPlaying ? startPosition : transform.position;

		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(pos + Vector3.left * patrolDistance, pos + Vector3.right * patrolDistance);
		Gizmos.DrawWireSphere(pos + Vector3.left * patrolDistance, 0.15f);
		Gizmos.DrawWireSphere(pos + Vector3.right * patrolDistance, 0.15f);

		Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
		Gizmos.DrawWireSphere(transform.position, losePlayerRange);
	}
#endif
}
