using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    EnemyBaseState currentState;
    public EnemyIdleState IdleState = new EnemyIdleState();
    public EnemyChaseState ChaseState = new EnemyChaseState();
    public EnemyAttackState AttackState = new EnemyAttackState();
    public EnemyDeadState DeadState = new EnemyDeadState();

    public EnemyConfig enemyConfig;
    public Transform bulletStart;

    [HideInInspector] public IAttackBehavior attackBehavior;

    [HideInInspector] public MovementController movementController;

    [HideInInspector] public Vector3 idleDir = Vector3.zero;
    

    [HideInInspector] public Transform playerTransform;
    private Vector3 lastPlayerPosition = Vector3.zero;

    private CapsuleCollider capsuleCollider;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public float sight;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Vector3 randomPosition;
    [HideInInspector] public Vector3 enemyVelocity = Vector3.zero;
    [HideInInspector] public Vector3 enemyExternalVelocity = Vector3.zero;

    private Rigidbody[] rigidbodies;
    private Collider[] colliders;

    private Health enemyHealth;

    public GameObject projectile;

    void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        attackBehavior = enemyConfig.GetAttackBehavior();

        enemyHealth = GetComponent<Health>();
        enemyHealth.SetMaxHealth(enemyConfig.maxHealth);

        animator = GetComponentInChildren<Animator>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        capsuleCollider = GetComponent<CapsuleCollider>();
        sight = GetComponentInChildren<SphereCollider>().radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        
        movementController = GetComponent<MovementController>();
        movementController.SetGravity(enemyConfig.gravity);
        
        currentState = IdleState;
        currentState.EnterState(this);
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();

        SetRagdollColliders(false);
        SetRagdollRigidBody(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        currentState.FixedUpdateState(this);
    }

    public void SwitchState(EnemyBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }

    public Vector3 GetVectorToLastPlayerPosition()
    {
        return lastPlayerPosition - transform.position;

    }

    public bool IsPlayerInSight()
    {
        if ((playerTransform.position - transform.position).sqrMagnitude < sight * sight)
        {
            if (Physics.Raycast(transform.position, playerTransform.position - transform.position, out RaycastHit hit, 100, ~((1 << 8) | (1 << 6))))
            {
                if (hit.transform.gameObject.CompareTag("Player"))
                {
                    lastPlayerPosition = hit.transform.position;
                    Debug.DrawRay(lastPlayerPosition, transform.up, Color.white, 1f);
                    return true;
                }
            }
        }
        return false;
    }

    public Vector3 UpdateLastPlayerPosition()
    {
        if (Physics.Raycast(transform.position, playerTransform.position - transform.position, out RaycastHit hit, 100, ~((1 << 8) | (1 << 6) | (1 << 9))))
        {
            if (hit.transform.gameObject.CompareTag("Player"))
            {
                lastPlayerPosition = hit.transform.position;
                Debug.DrawRay(lastPlayerPosition, transform.up, Color.white, 1f);
            }
        }
        return lastPlayerPosition;
    }

    public void InvokeRandomDirection()
    {
        InvokeRepeating("GetRandomDirection", 0, 2);
    }

    public void GoInDirection(Vector3 dir)
    {
        enemyVelocity = Vector3.Project(enemyVelocity, transform.up) + Vector3.ProjectOnPlane(dir, transform.up);

        Quaternion targetRotation = Quaternion.LookRotation(dir, transform.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * enemyConfig.rotationSpeed);
        enemyVelocity = movementController.Move(enemyVelocity);
        Debug.DrawRay(transform.position, enemyVelocity);
    }

    public void RotateInDirection(Vector3 dir)
    {
        Quaternion targetRotation = Quaternion.LookRotation(dir, transform.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * enemyConfig.rotationSpeed);
    }

    public void FlyInDirection(Vector3 dir)
    {
        Quaternion targetRotation = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * enemyConfig.rotationSpeed);
        enemyVelocity = movementController.Move(dir);
    }

    public void GoToPosition(Vector3 pos, float speed)
    {
        if ((pos - transform.position).sqrMagnitude < 0.25)
            return;

        Vector3 dir = Vector3.ProjectOnPlane(pos - transform.position, transform.up).normalized;
        enemyVelocity = Vector3.Project(enemyVelocity, transform.up) + dir * speed;

        Quaternion targetRotation = Quaternion.LookRotation(dir, transform.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * enemyConfig.rotationSpeed);
        enemyVelocity = movementController.Move(enemyVelocity);
        Debug.DrawRay(transform.position, enemyVelocity);
    }

    public void JumpTo(Vector3 pos)
    {
        if (!movementController.GroundCheck())
            return;

        if ((pos - transform.position).sqrMagnitude > enemyConfig.maxJumpDistance * enemyConfig.maxJumpDistance)
            pos = transform.position+(pos - transform.position).normalized * enemyConfig.maxJumpDistance;

        Vector3 up = playerTransform.transform.up;

        // Bottom of the player (for reference)
        float height = capsuleCollider.height;
        Vector3 lowestPoint = transform.position - up * (height / 2f);

        // Vertical displacement from player bottom to grapple point
        float grappleY = Vector3.Dot(pos - lowestPoint, up);

        // Determine the apex of the arc
        float apexHeight = grappleY /*+ upForce*/; // desired arc above grapple point

        // Make sure apex is always above player's bottom
        apexHeight = Mathf.Max(apexHeight, 0.5f); // minimum 0.5 meters to avoid NaN

        // Total displacement
        Vector3 displacement = pos - transform.position;

        // Vertical and horizontal components
        float displacementY = Vector3.Dot(displacement, up); // vertical
        Vector3 displacementXZ = Vector3.ProjectOnPlane(displacement, up); // horizontal

        // Gravity
         // positive number, magnitude of downward acceleration

        // Vertical velocity to reach apex
        float velocityY = Mathf.Sqrt(2f * enemyConfig.gravity * apexHeight);

        // Time to reach apex
        float timeUp = velocityY / enemyConfig.gravity;

        // Time to fall from apex to target
        float fallHeight = apexHeight - displacementY;
        float timeDown = Mathf.Sqrt(Mathf.Max(2f * fallHeight / enemyConfig.gravity, 0.01f)); // avoid sqrt(0)

        float totalTime = timeUp + timeDown;

        // Horizontal velocity needed
        Vector3 velocityXZ = displacementXZ / totalTime;

        // Final velocity
        Vector3 jumpVelocity = (velocityXZ + up * velocityY);






        enemyVelocity = Vector3.Project(enemyVelocity, transform.up) + jumpVelocity;

        //Quaternion targetRotation = Quaternion.LookRotation(dir, transform.up);
//
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        enemyVelocity = movementController.Move(enemyVelocity);
        Debug.DrawRay(transform.position, enemyVelocity);
    }



    public void GetRandomDirection()
    {
        idleDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        idleDir = Vector3.ProjectOnPlane(idleDir, transform.up).normalized;
    }

    public Vector3 GetPlayerPosition()
    {
        return playerTransform.transform.position;
    }
    
    public void SetRagdollRigidBody(bool state)
    {
        foreach(Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !state;
        }
        
    }

    public void SetRagdollColliders(bool state)
    {
        foreach(Collider col in colliders)
        {
            col.enabled = state;
        }
        capsuleCollider.enabled = !state;
    }

    void OnEnable()
    {
        if (enemyHealth != null)
            enemyHealth.OnDeath += OnDeath;
    }

    void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnDeath -= OnDeath;
    }

    private void OnDeath()
    {
        SwitchState(DeadState);
        Debug.Log("DIED!!!!!");
    }
}
