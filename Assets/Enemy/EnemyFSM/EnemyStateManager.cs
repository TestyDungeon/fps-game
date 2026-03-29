using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.Animations;
using UnityEngine.UI;
using Animancer;

public class EnemyStateManager : MonoBehaviour
{
    EnemyBaseState currentState;
    public EnemyIdleState IdleState = new EnemyIdleState();
    public EnemyChaseState ChaseState = new EnemyChaseState();
    public EnemyAttackState AttackState = new EnemyAttackState();
    public EnemyFalterState FalterState = new EnemyFalterState();
    public EnemyAirFalterState AirFalterState = new EnemyAirFalterState();
    public EnemyStaggerState StaggerState = new EnemyStaggerState();
    public EnemyWanderState WanderState = new EnemyWanderState();
    public EnemyDeadState DeadState = new EnemyDeadState();

    public EnemyConfig enemyConfig;
    public Transform[] bulletStart;
    public BoxCollider meleeAttackCollider;


    [HideInInspector] public IAttackBehavior attackBehavior;
    [HideInInspector] public ITraversalBehavior traversalBehavior;

    [HideInInspector] public MovementController movementController;
    private CapsuleCollider capsuleCollider;
    [HideInInspector] public float height;
    [HideInInspector] public NavMeshAgent agent;
    

    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public Vector3 lookDir = Vector3.forward;

    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public float sight;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Vector3 enemyVelocity = Vector3.zero;
    private int layermask = ~((1 << 6) | (1 << 9) | (1 << 11) | (1 << 2) | (1 << 13) | (1 << 12) | (1 << 10));

    [HideInInspector] public int playerLayer = 1 << 3;
    private int enemyLayer = 1 << 8;

    private float avoidanceRadius = 2;
    private float avoidanceForce = 1;

    [HideInInspector] public Rigidbody[] rigidbodies;
    private Collider[] colliders;

    private Health enemyHealth;


    private TextMeshProUGUI text;
    private Image healthFill;
    private Image postureFill;
    private LookAtConstraint lookAtConstraint;
    
    [HideInInspector] public bool canAttack = true;


    [HideInInspector] public AnimancerComponent animancer;
    [HideInInspector] public SkinnedMeshRenderer[] smrs;
    public Material staggerMat;

    [HideInInspector] public Vector3 lastDamageVector = Vector3.zero;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;

        smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        animancer = GetComponentInChildren<AnimancerComponent>();
        audioSource = GetComponentInChildren<AudioSource>();
        attackBehavior = enemyConfig.GetAttackBehavior();
        traversalBehavior = enemyConfig.GetTraversalBehavior();

        enemyHealth = GetComponent<Health>();
        enemyHealth.SetMaxHealth(enemyConfig.maxHealth);
        enemyHealth.SetHealth(enemyConfig.maxHealth);
        enemyHealth.SetMaxPosture(enemyConfig.maxHealth);
        enemyHealth.SetPosture(enemyConfig.maxHealth);

        animator = GetComponentInChildren<Animator>();
        targetTransform = GameObject.FindGameObjectWithTag("Player").transform;

        capsuleCollider = GetComponent<CapsuleCollider>();
        height = capsuleCollider.height;
        sight = GetComponentInChildren<SphereCollider>().radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        
        movementController = GetComponent<MovementController>();
        movementController.SetGravity(enemyConfig.gravity);
        
        currentState = IdleState;
        currentState.EnterState(this);
    }

    void Start()
    {
        // Debug ui above enemy, for health, posture, current state, etc,
        text = GetComponentInChildren<TextMeshProUGUI>();
        if(text != null)
        {
            lookAtConstraint = GetComponentInChildren<LookAtConstraint>();
            ConstraintSource constraintSource = new ConstraintSource();
            constraintSource.sourceTransform = targetTransform;
            lookAtConstraint.AddSource(constraintSource);
            constraintSource.weight = 1;
            lookAtConstraint.SetSource(0, constraintSource);
        }
        foreach(Image img in GetComponentsInChildren<Image>())
        {
            if(img.name == "Health")
                healthFill = img; 
            if(img.name == "Posture")
                postureFill = img; 
        }
        
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();

        SetRagdollRigidBody(false);
        SetRagdollColliders(false);
    }

    
    void FixedUpdate()
    {
        if (targetTransform == null)
        {
            targetTransform = PlayerHitResponder.Instance.transform;
        }

        RotateInDirection();

        currentState.FixedUpdateState(this);
        Friction();
        enemyVelocity = movementController.Move(enemyVelocity);

        if(text != null)
        {
            text.SetText("S: " + currentState + "\n" +
                        "A:" + canAttack);

            healthFill.fillAmount = (float)enemyHealth.GetHealth() / enemyHealth.GetMaxHealth();
            postureFill.fillAmount = (float)enemyHealth.GetPosture() / enemyHealth.GetMaxPosture();
        }
        
    }

    void LateUpdate() 
    {
        agent.nextPosition = transform.position;
        if(Vector3.Distance(agent.nextPosition, transform.position) > 2)
            agent.Warp(transform.position);
    }



    public void SwitchState(EnemyBaseState state)
    {
        if(currentState is EnemyDeadState)
            return;

        
        currentState.ExitState(this);
        currentState = state;
        state.EnterState(this);
    }

    public IEnumerator SwitchState(EnemyBaseState state, float x, float y)
    {
        yield return new WaitForSeconds(Random.Range(x, y));
        SwitchState(state);
    }


    public void GoToTarget(float speed)
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.destination = targetTransform.position;
            if(IsInvoking("GetRandomReachablePointOnNavMesh"))
                CancelInvoke("GetRandomReachablePointOnNavMesh");
            animancer.Play(enemyConfig.walkAnimation);
            GoInDirection(agent.desiredVelocity.normalized * speed + CalculateAvoidance());
            lookDir = agent.desiredVelocity;
        }
    }

    public void MoveToTarget(float speed)
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.destination = targetTransform.position;
            if(IsInvoking("GetRandomReachablePointOnNavMesh"))
                CancelInvoke("GetRandomReachablePointOnNavMesh");
            
            GoInDirection(agent.desiredVelocity.normalized * speed + CalculateAvoidance());
            lookDir = agent.desiredVelocity;
        }
    }


    public Vector3 GetVectorToTarget()
    {
        return targetTransform.position - transform.position;

    }

    private void Friction()
    {
        float control, drop, newspeed;

        float speed = enemyVelocity.magnitude;

        if (speed < 0.01)
        {
            enemyVelocity = Vector3.zero;
            return;
        }

        drop = 0;

        if (movementController.GroundCheck())
        {
            control = speed < 0.1f ? 0.1f : speed;
            drop += control * 6 * Time.deltaTime;
        }

        newspeed = speed - drop;
        if (newspeed < 0)
            newspeed = 0;
        newspeed /= speed;
        
        enemyVelocity[0] *= newspeed;
        enemyVelocity[1] *= newspeed;
        enemyVelocity[2] *= newspeed;
    }

    public bool IsTargetInSight()
    {
        if (Physics.Raycast(transform.position, targetTransform.position - transform.position, out RaycastHit hit, 100, layermask))
        {
            if (hit.transform == targetTransform)
            {
                return true;
            }
        }
        return false;
    }


    public void GoInDirection(Vector3 dir)
    {
        lookDir = dir;
        enemyVelocity = Vector3.Project(enemyVelocity, transform.up) + Vector3.ProjectOnPlane(dir, transform.up).normalized * dir.magnitude;
    }

    public void GoToDestination(Vector3 dest, float speed)
    {
        if((transform.position-dest).sqrMagnitude > 4)
        {
            animancer.Play(enemyConfig.walkAnimation);
            agent.destination = dest;
            GoInDirection(Vector3.ProjectOnPlane(agent.desiredVelocity, transform.up).normalized * speed + CalculateAvoidance());
        }
    }

    public void RotateInDirection()
    {
        Vector3 flatDir = Vector3.ProjectOnPlane(lookDir, transform.up);
        if (flatDir.sqrMagnitude < 0.001f) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(flatDir.normalized, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * enemyConfig.rotationSpeed);
    }

    public Vector3 GetRandomReachablePointOnNavMesh(float range = 10, int maxAttempts = 10)
    {

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * range;
            Vector3 randomPoint_ = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint_, out hit, range, NavMesh.AllAreas))
            {
                
                NavMeshHit pathHit;
                if (!NavMesh.Raycast(transform.position, hit.position, out pathHit, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }
        }

        return transform.position;
    }

    public IEnumerator ResetAttack(EnemyBaseState state = null)
    {
        yield return new WaitForSeconds(enemyConfig.attackCooldown);
        if(state != null)
            SwitchState(state);
        canAttack = true;
    }

    public void AirJuggle(float upForce)
    {
        Debug.Log("Juggle");
        enemyVelocity += transform.up * upForce;
        
    }

    private Vector3 CalculateAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        
        Collider[] nearbyAgents = Physics.OverlapSphere(transform.position, avoidanceRadius, enemyLayer);
        
        foreach (Collider col in nearbyAgents)
        {
            //Debug.Log("Name " + col.name);
            if (col.gameObject == gameObject) continue;
            
            Vector3 directionAway = transform.position - col.transform.position;
            float distance = directionAway.magnitude;
            
            if (distance > 0)
            {
                float strength = 1f - (distance / avoidanceRadius);
                avoidance += directionAway.normalized * strength * avoidanceForce;
            }
        }
        
        return avoidance;
    }

    

    public EnemyBaseState GetCurrentState()
    {
        return currentState;
    }

    

    public Vector3 GetPlayerPosition()
    {
        return targetTransform.transform.position;
    }
    
    public void SetRagdollRigidBody(bool state)
    {
        if(rigidbodies == null)
            return;
        foreach(Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !state;
        }
        
    }

    public void ForceRagdollRigidBody(Vector3 force)
    {
        if(rigidbodies == null)
            return;
        foreach(Rigidbody rb in rigidbodies)
        {
            rb.AddForce(force, ForceMode.Impulse);;
        }
        
    }

    public void SetRagdollColliders(bool state)
    {
        if(colliders == null)
            return;
        foreach(Collider col in colliders)
        {
            col.enabled = state;
        }
        capsuleCollider.enabled = !state;
    }

    public void SetNavmeshAgent(bool x)
    {
        agent.enabled = x;
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
        if(currentState is not EnemyDeadState)
            SwitchState(DeadState);
        Debug.Log("DIED!!!!!");
    }

    public void SetTarget(Transform target)
    {
        targetTransform = target; 
    }
}
