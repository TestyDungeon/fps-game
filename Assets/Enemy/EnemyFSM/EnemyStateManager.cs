using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.Animations;
using UnityEngine.UI;

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
    private Vector3 lastSeenTargetPosition;
    [HideInInspector] public Vector3 lookDir = Vector3.forward;

    [HideInInspector] public AudioSource audioSource;
    //[HideInInspector] public float sight;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Vector3 enemyVelocity = Vector3.zero;
    private int layermask = (1 << 0 | 1 << 3 | 1 << 14);

    [HideInInspector] public int playerLayer = 1 << 3;
    private int enemyLayer = 1 << 8;

    private float avoidanceRadius = 2;
    private float avoidanceForce = 1;

    [HideInInspector] public Rigidbody[] rigidbodies;
    private Collider[] colliders;

    private Health enemyHealth;
    [HideInInspector] public EnemyHitResponder ehr;


    private TextMeshProUGUI text;
    private Image healthFill;
    private Image postureFill;
    private LookAtConstraint lookAtConstraint;
    
    [HideInInspector] public bool canAttack = true;


    //[HideInInspector] public AnimancerComponent animancer;
    [HideInInspector] public CodeAnimationEvents animEvents;
    
    [HideInInspector] public SkinnedMeshRenderer[] smrs;
    [HideInInspector] public Material staggerMat;

    [HideInInspector] public float lastDamage = 0;
    [HideInInspector] public Vector3 lastDamageVector = Vector3.zero;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        ehr = GetComponentInChildren<EnemyHitResponder>();
        smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        //animancer = GetComponentInChildren<AnimancerComponent>();
        animEvents = GetComponentInChildren<CodeAnimationEvents>();
        audioSource = GetComponentInChildren<AudioSource>();
        attackBehavior = enemyConfig.GetAttackBehavior();
        traversalBehavior = enemyConfig.GetTraversalBehavior();

        enemyHealth = GetComponent<Health>();
        enemyHealth.SetMaxHealth(enemyConfig.maxHealth);
        enemyHealth.SetHealth(enemyConfig.maxHealth);
        enemyHealth.SetMaxPosture(enemyConfig.maxHealth);
        enemyHealth.SetPosture(enemyConfig.maxHealth);

        animator = GetComponentInChildren<Animator>();

        capsuleCollider = GetComponent<CapsuleCollider>();
        height = capsuleCollider.height;
        //sight = GetComponentInChildren<SphereCollider>().radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        
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

        //SetRagdollRigidBody(false);
        //SetRagdollColliders(false);
    }

    
    void FixedUpdate()
    {
        if (targetTransform == null)
        {
            targetTransform = PlayerMovement.Instance.transform;
        }

        

        RotateInDirection();

        currentState.FixedUpdateState(this);
        Friction();
        enemyVelocity = movementController.Move(enemyVelocity);

        if(text != null)
        {
            text.SetText("S: " + currentState + "\n" +
                        "A:" + canAttack + "\n" +
                        "IsOnNav:" + IsOnUsableNavMesh());

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

        //Debug.Log("Switching to " + state + " from " + currentState);
        StopAllCoroutines();
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
        if (IsOnUsableNavMesh())
        {
            agent.destination = targetTransform.position;
            if(IsInvoking("GetRandomReachablePointOnNavMesh"))
                CancelInvoke("GetRandomReachablePointOnNavMesh");
            animator.Play("Walk");
            GoInDirection(agent.desiredVelocity.normalized * speed + CalculateAvoidance());
            lookDir = agent.desiredVelocity;
        }
        else if (movementController.GroundCheck() && !IsTargetReachable())
        {
            animator.Play("Walk");
            GoInDirection(Vector3.ProjectOnPlane(GetVectorToTarget(), transform.up).normalized * speed);
        }
    }

    public void MoveToTarget(float speed)
    {
        if (IsOnUsableNavMesh())
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
        //Debug.DrawLine(targetTransform.position, transform.position, Color.cyan);
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
        if (Physics.SphereCast(transform.position, 0.5f, targetTransform.position - transform.position, out RaycastHit hit, 100, layermask))
        {
            //Debug.Log("SIGHT " + hit.transform.name + " TARGET " + targetTransform.name);
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
            animator.Play("Walk");
            if (IsOnUsableNavMesh())
            {
                agent.destination = dest;
                GoInDirection(Vector3.ProjectOnPlane(agent.desiredVelocity, transform.up).normalized * speed + CalculateAvoidance());
            }
            else
            {
                GoInDirection(Vector3.ProjectOnPlane(dest - transform.position, transform.up).normalized * speed);
            }
        }
    }

    public bool IsOnUsableNavMesh(float sampleRadius = 0.6f, float maxFootHeightDelta = 1.25f)
    {
        if (!agent.enabled || !agent.isOnNavMesh)
            return false;

        Vector3 capsuleCenterWorld = capsuleCollider != null
            ? transform.TransformPoint(capsuleCollider.center)
            : transform.position;

        float feetOffset = capsuleCollider != null
            ? Mathf.Max(0f, capsuleCollider.height * 0.5f - capsuleCollider.radius)
            : 0.9f;

        Vector3 feetPosition = capsuleCenterWorld - transform.up * feetOffset;

        // updatePosition is disabled, so validate navmesh proximity against the feet, not pivot.
        if (!NavMesh.SamplePosition(feetPosition, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            return false;

        Vector3 delta = hit.position - feetPosition;
        Vector3 planarDelta = Vector3.ProjectOnPlane(delta, transform.up);
        float verticalDelta = Vector3.Dot(delta, transform.up);

        return planarDelta.sqrMagnitude <= sampleRadius * sampleRadius
               && Mathf.Abs(verticalDelta) <= maxFootHeightDelta;
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

            
            if (NavMesh.SamplePosition(randomPoint_, out NavMeshHit hit, range, NavMesh.AllAreas))
            {
                if (!NavMesh.Raycast(transform.position, hit.position, out NavMeshHit pathHit, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }
        }

        return transform.position;
    }

    public bool IsTargetReachable()
    {
        if (targetTransform == null) return false;

        NavMeshPath path = new NavMeshPath();
        if (agent.isActiveAndEnabled && IsOnUsableNavMesh())
        {
            if (agent.CalculatePath(targetTransform.position, path))
            {
                return path.status == NavMeshPathStatus.PathComplete;
            }
        }
        else
        {
            // Fallback if agent is disabled or not on mesh
            if (NavMesh.CalculatePath(transform.position, targetTransform.position, NavMesh.AllAreas, path))
            {
                return path.status == NavMeshPathStatus.PathComplete;
            }
        }
        return false;
    }

    public IEnumerator ResetAttack(EnemyBaseState state = null)
    {
        yield return new WaitForSeconds(enemyConfig.attackCooldown);
        if(state != null)
            SwitchState(state);
        canAttack = true;
    }

    public void Kicked(Vector3 dir)
    {
        //Debug.Log("Juggle");
        movementController.resetVelocity();
        movementController.Dash(dir, 2f, 12.5f, 5);
        SwitchState(FalterState);
    }

    public void DeathKnockback(Vector3 dir, float damage)
    {
        damage = Mathf.Max(damage, 70);
        //lastDamage += damage;
        Debug.Log("Last Damage: " + damage);
        //if (movementController.GetIsDashing())
        //{
            movementController.resetVelocity();
            movementController.StopDash();  
            
            movementController.Dash(dir, 2f * damage / 120, 12.5f * damage / 120, 5 * damage / 120);
        //}
        //else
        //{
        //    movementController.Dash(dir, 2f * lastDamage / 20, 12.5f * lastDamage / 20, 5 * lastDamage / 20);
        //}
        
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

    public void SpawnCorpse()
    {
        GameObject x = Object.Instantiate(enemyConfig.deathParticle, transform.position - transform.up * (capsuleCollider.height / 2.1f), Quaternion.LookRotation(transform.forward, transform.up));
        Object.Destroy(x, 30);
    }
}
