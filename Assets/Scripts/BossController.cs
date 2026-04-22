using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class BossController : MonoBehaviour
{

    [Tooltip("Velocidad al caminar de lado a lado.")]
    [SerializeField] private float patrolSpeed = 2.5f;

    [Tooltip("Distancia máxima desde el punto de inicio en cada dirección.")]
    [SerializeField] private float patrolDistance = 6f;

    [Tooltip("Rango en el que el Boss empieza a perseguir al Player.")]
    [SerializeField] private float detectionRange = 12f;

    [Tooltip("Margen extra antes de perder al Player (evita flickering).")]
    [SerializeField] private float detectionHysteresis = 2f;

    [Tooltip("Distancia a la que el Boss se detiene y ataca.")]
    [SerializeField] private float attackRange = 2.5f;

    [Tooltip("Velocidad de persecución.")]
    [SerializeField] private float chaseSpeed = 5f;

    [Tooltip("Daño que inflige cada golpe.")]
    [SerializeField] private int attackDamage = 2;

    [Tooltip("Segundos entre ataques consecutivos.")]
    [SerializeField] private float attackCooldown = 1.5f;

    [Tooltip("Duración del clip de ataque (en segundos). Usada para esperar antes de volver a Chase/Idle).")]
    [SerializeField] private float attackAnimDuration = 0.8f;

    [SerializeField] private int maxHealth = 20;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Puerta")]
    [SerializeField] private float doorMoveDistance = 5f;
    [SerializeField] private float doorMoveDuration = 2f;

    private static readonly int AnimIsRunning = Animator.StringToHash("isRunning");
    private static readonly int AnimAttack    = Animator.StringToHash("attack");
    private static readonly int AnimDie       = Animator.StringToHash("die");

    private enum BossState { Idle, Chase, Attack, Dead }

    private BossState currentState = BossState.Idle;
    private Rigidbody  rb;
    private Animator   anim;
    private Transform  player;

    private Vector3 startPosition;
    private bool    facingRight    = true;
    private int     currentHealth;
    private float   lastAttackTime = -999f;
    private bool    isAttacking    = false;


    void Awake()
    {
        rb   = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        ConfigureRigidbody();
    }

    void Start()
    {
        startPosition  = transform.position;
        currentHealth  = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"[BossController] No se encontró GameObject con tag 'Player'.");

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        TransitionTo(BossState.Idle);
    }

    void FixedUpdate()
    {
        if (currentState == BossState.Dead) return;
        if (isAttacking) return;

        EvaluateState();
        ExecuteState();
    }

    private void EvaluateState()
    {
        if (player == null)
        {
            if (currentState != BossState.Idle)
                TransitionTo(BossState.Idle);
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case BossState.Idle:
                if (dist <= detectionRange)
                    TransitionTo(BossState.Chase);
                break;

            case BossState.Chase:
                if (dist <= attackRange)
                    TransitionTo(BossState.Attack);
                else if (dist > detectionRange + detectionHysteresis)
                    TransitionTo(BossState.Idle);
                break;

            case BossState.Attack:
                if (dist > attackRange)
                    TransitionTo(BossState.Chase);
                break;
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case BossState.Idle:   DoPatrol();  break;
            case BossState.Chase:  DoChase();   break;
            case BossState.Attack: DoAttack();  break;
        }
    }

    private void TransitionTo(BossState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (newState)
        {
            case BossState.Idle:
                anim.SetBool(AnimIsRunning, false);
                StopHorizontalMovement();
                break;

            case BossState.Chase:
                anim.SetBool(AnimIsRunning, true);
                break;

            case BossState.Attack:
                anim.SetBool(AnimIsRunning, false);
                StopHorizontalMovement();
                break;

            case BossState.Dead:
                anim.SetBool(AnimIsRunning, false);
                StopHorizontalMovement();
                if (HasAnimatorParam(AnimDie))
                    anim.SetTrigger(AnimDie);
                break;
        }
    }

    private void DoPatrol()
    {
        float offsetX = transform.position.x - startPosition.x;

        if      (offsetX >=  patrolDistance) facingRight = false;
        else if (offsetX <= -patrolDistance) facingRight = true;

        MoveHorizontal(facingRight ? 1f : -1f, patrolSpeed);
    }

    private void DoChase()
    {
        if (player == null) return;

        float dirX = player.position.x - transform.position.x;
        facingRight = dirX > 0f;
        MoveHorizontal(Mathf.Sign(dirX), chaseSpeed);
    }

    private void DoAttack()
    {
        if (player != null)
        {
            float dirX = player.position.x - transform.position.x;
            facingRight = dirX > 0f;
            UpdateFacing(Mathf.Sign(dirX));
        }

        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetTrigger(AnimAttack);

        yield return new WaitForSeconds(attackAnimDuration);

        isAttacking = false;
    }

    public void DealDamage()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange + 0.5f)
        {
            if (Player.current != null)
                Player.current.Hurt(attackDamage);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isAttacking) return;
        if (!other.CompareTag("Player")) return;

        if (Player.current != null)
            Player.current.Hurt(attackDamage);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (currentState == BossState.Dead) return;
        if (!other.gameObject.CompareTag("Player")) return;

        Vector3 contactPoint = other.GetContact(0).point;
        Rigidbody playerRb = Player.current.GetComponent<Rigidbody>();

        float contactHeight = contactPoint.y - transform.position.y;
        float bossHeight = GetComponent<Collider>().bounds.size.y;

        bool isOnTop = contactHeight > (bossHeight * 0.5f);
        bool isFalling = playerRb != null && playerRb.velocity.y < 0f;

        if (isOnTop && isFalling)
        {
            TakeDamage(1);
            Player.current.Knockback(Vector3.up, 20f);
        }
        else
        {
            Player.current.Knockback(-other.GetContact(0).normal, 40f);
            Player.current.Hurt(1);
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentState == BossState.Dead) return;

        currentHealth -= amount;
        currentHealth  = Mathf.Max(currentHealth, 0);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        TransitionTo(BossState.Dead);
        rb.velocity = Vector3.zero;

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        GameObject door = GameObject.Find("DoorContainer");
        if (door != null)
            StartCoroutine(MoveDoor(door));

        Destroy(gameObject, 2f);
    }

    private IEnumerator MoveDoor(GameObject door)
    {
        Vector3 startPos = door.transform.position;
        Vector3 endPos   = startPos + Vector3.up * doorMoveDistance;
        float   elapsed  = 0f;

        while (elapsed < doorMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / doorMoveDuration));
            door.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        door.transform.position = endPos;
    }

    private void MoveHorizontal(float direction, float speed)
    {
        rb.velocity = new Vector3(direction * speed, rb.velocity.y, 0f);
        UpdateFacing(direction);
    }

    private void StopHorizontalMovement()
    {
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    private void UpdateFacing(float direction)
    {
        if (direction == 0f) return;
        float y = direction > 0f ? 90f : 270f;
        transform.rotation = Quaternion.Euler(0f, y, 0f);
    }

    private bool HasAnimatorParam(int hash)
    {
        foreach (var p in anim.parameters)
            if (p.nameHash == hash) return true;
        return false;
    }

    private void ConfigureRigidbody()
    {
        if (rb == null) return;

        rb.isKinematic            = false;
        rb.mass                   = 5f;
        rb.drag                   = 0f;
        rb.angularDrag            = 10f;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints            =
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
        Gizmos.DrawLine(pos + Vector3.left  * patrolDistance,
                        pos + Vector3.right * patrolDistance);
        Gizmos.DrawWireSphere(pos + Vector3.left  * patrolDistance, 0.2f);
        Gizmos.DrawWireSphere(pos + Vector3.right * patrolDistance, 0.2f);

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}