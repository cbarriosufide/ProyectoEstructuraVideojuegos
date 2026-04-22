using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
	[Header("Patrullaje")]
	[SerializeField] private float patrolSpeed = 4f;
	[SerializeField] private float patrolDistance = 5f;

	[Header("Persecución")]
	[SerializeField] private float chaseSpeed = 7f;
	[SerializeField] private float detectionRange = 10f;
	[SerializeField] private float losePlayerRange = 13f;

	[Header("Vida")]
	public float health = 10f;
	public float maxHealth = 10f;

	private Rigidbody rb;
	private Transform player;
	private Animator animator;

	private Vector3 startPosition;
	private bool facingRight = true;
	private bool isChasing = false;

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

		if (player == null)
		{
			Patrol();
			return;
		}

		float distanceToPlayer = Vector3.Distance(transform.position, player.position);

		if (!isChasing && distanceToPlayer <= detectionRange)
			isChasing = true;

		if (isChasing && distanceToPlayer > losePlayerRange)
			isChasing = false;

		if (isChasing)
			Chase();
		else
			Patrol();

		animator.SetFloat("Speed", currentSpeed);
	}

	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject == Player.current.gameObject)
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
				Debug.Log($"Jugador cayó encima del enemigo");
			}
			else
			{
				Player.current.Knockback(-other.GetContact(0).normal, 40f);
				Player.current.Hurt(1);
			}
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
	}

	private void Chase()
	{
		float dirX = player.position.x - transform.position.x;
		facingRight = dirX > 0f;

		currentSpeed = chaseSpeed;
		MoveHorizontally(Mathf.Sign(dirX), chaseSpeed);
	}

	private void MoveHorizontally(float direction, float speed)
	{
		rb.velocity = new Vector3(direction * speed, rb.velocity.y, 0f);
		UpdateFacing(direction);
	}

	private void UpdateFacing(float direction)
	{
		if (direction == 0f) return;

		float y = direction > 0f ? 90f : 270f;
		transform.rotation = Quaternion.Euler(0f, y, 0f);
	}

	private void ConfigureRigidbody()
	{
		if (rb == null) return;

		rb.isKinematic = false;

		rb.mass = 2f;

		rb.drag = 0f;

		rb.angularDrag = 10f;

		rb.interpolation = RigidbodyInterpolation.Interpolate;

		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

		rb.constraints =
			RigidbodyConstraints.FreezePositionZ |
			RigidbodyConstraints.FreezeRotationX |
			RigidbodyConstraints.FreezeRotationY |
			RigidbodyConstraints.FreezeRotationZ;

		rb.useGravity = true;
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

		Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
		Gizmos.DrawWireSphere(transform.position, detectionRange);

		Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
		Gizmos.DrawWireSphere(transform.position, losePlayerRange);
	}
#endif
}
