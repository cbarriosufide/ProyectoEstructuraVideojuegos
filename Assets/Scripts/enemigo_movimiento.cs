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

    [Header("Combate")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageCooldown = 1f;

    [Header("Visuals (opcional)")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody rb;
    private Transform player;

    private Vector3 startPosition;
    private bool facingRight = true;
    private bool isChasing = false;
    private float lastDamageTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ConfigureRigidbody();
    }

    void Start()
    {
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"[{name}] No se encontró un GameObject con tag 'Player'.");

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
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
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        if (Player.current != null)
        {
            Player.current.Hurt(damageAmount);
            lastDamageTime = Time.time;
        }
    }

    private void Patrol()
    {
        float offsetX = transform.position.x - startPosition.x;

        if (offsetX >= patrolDistance)
            facingRight = false;
        else if (offsetX <= -patrolDistance)
            facingRight = true;

        MoveHorizontally(facingRight ? 1f : -1f, patrolSpeed);
    }

    private void Chase()
    {
        float dirX = player.position.x - transform.position.x;
        facingRight = dirX > 0f;
        MoveHorizontally(Mathf.Sign(dirX), chaseSpeed);
    }

    private void MoveHorizontally(float direction, float speed)
    {
        rb.velocity = new Vector3(direction * speed, rb.velocity.y, 0f);
        UpdateFacing();
    }

    private void UpdateFacing()
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = !facingRight;
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

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 pos = Application.isPlaying ? startPosition : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pos + Vector3.left * patrolDistance, pos + Vector3.right * patrolDistance);
        Gizmos.DrawWireSphere(pos + Vector3.left  * patrolDistance, 0.15f);
        Gizmos.DrawWireSphere(pos + Vector3.right * patrolDistance, 0.15f);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, losePlayerRange);
    }
#endif
}