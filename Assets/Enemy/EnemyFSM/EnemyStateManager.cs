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

    [HideInInspector] public Vector3 idleDir = Vector3.zero;
    

    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public Vector3 lookDir = Vector3.forward;
    private Vector3 lastTargetPosition = Vector3.zero;

    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public float sight;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Vector3 randomPosition;
    [HideInInspector] public Vector3 enemyVelocity = Vector3.zero;
    [HideInInspector] public Vector3 enemyExternalVelocity = Vector3.zero;
    private int layermask = ~((1 << 6) | (1 << 9) | (1 << 11) | (1 << 2) | (1 << 13) | (1 << 12) | (1 << 10));

    [HideInInspector] public int playerLayer = 1 << 3;
    private int enemyLayer = 1 << 8;

    private float avoidanceRadius = 2;
    private float avoidanceForce = 1;

    private Rigidbody[] rigidbodies;
    private Collider[] colliders;

    private Health enemyHealth;

    

    private TextMeshProUGUI text;
    private Image healthFill;
    private Image postureFill;
    private LookAtConstraint lookAtConstraint;

    private string currentAnimation;
    
    private bool isTraversingLink = false;
    [HideInInspector] public bool canAttack = true;


    [HideInInspector] public AnimancerComponent animancer;
    [HideInInspector] public SkinnedMeshRenderer[] smrs;
    public Material staggerMat;

    Vector3 randomPoint;

    [HideInInspector] public Vector3 lastDamageVector = Vector3.zero;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        //staggerMat = Resources.Load<Material>("StaggerMaterial");
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
        //InvokeRepeating("UpdateLastPlayerPosition", 1, 0.6f);
    }

    
    void FixedUpdate()
    {
        if (targetTransform == null)
        {
            targetTransform = PlayerHitResponder.Instance.transform;
        }

        //if (isTraversingLink) return;
        
        
        if (agent.isOnOffMeshLink)
        {
            
            //SwitchState(FalterState);
            StartCoroutine(TraverseLink());
        }
        //Debug.Log("StateInfo " + animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        //if(!(currentState is EnemyDeadState))
        //    UpdateLastPlayerPosition();
        RotateInDirection();

        currentState.FixedUpdateState(this);
        Friction();
        enemyVelocity = movementController.Move(enemyVelocity);
        //Debug.DrawRay(transform.position, agent.desiredVelocity, Color.red, 2f);
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


    public void GoToTarget(float speed)
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.destination = targetTransform.position;
            // Check if the agent can reach the destination
            //if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            //{
            //    // Wander randomly if no valid path
            //    if(!IsInvoking("GetRandomReachablePointOnNavMesh"))
            //        InvokeRepeating("GetRandomReachablePointOnNavMesh", 0, 0.5f); // 10f is wander radius, adjust as needed
            //    agent.destination = randomPoint;
            //    GoInDirection((randomPoint - transform.position).normalized * speed + CalculateAvoidance());
            //    lookDir = randomPoint - transform.position;
            //}
            //else
            //{
                if(IsInvoking("GetRandomReachablePointOnNavMesh"))
                    CancelInvoke("GetRandomReachablePointOnNavMesh");
                animancer.Play(enemyConfig.walkAnimation);
                GoInDirection(agent.desiredVelocity.normalized * speed + CalculateAvoidance());
                lookDir = agent.desiredVelocity;
            //}
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


    public Vector3 GetVectorToLastTargetPosition()
    {
        Debug.DrawRay(transform.position, CalculateAvoidance(), Color.cyan, 1);
        //return agent.steeringTarget - transform.position;
        return agent.desiredVelocity + CalculateAvoidance();


    }

    public Vector3 GetVectorToTarget()
    {
        return targetTransform.position - transform.position;

    }

    private void Friction()
    {
        //ref float vel;
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
            //Debug.DrawLine(hit.point, transform.position, Color.white, 1f);
            if (hit.transform == targetTransform)
            {
                lastTargetPosition = hit.transform.position;
                
                return true;
            }
        }
        return false;
    }


    public void InvokeRandomDirection()
    {
        InvokeRepeating("GetRandomDirection", 0, 2);
    }

    public void GoInDirection(Vector3 dir)
    {
        
        
        
        lookDir = dir;
        //RotateInDirection(Vector3.ProjectOnPlane(agent.desiredVelocity, transform.up).normalized);
        enemyVelocity = Vector3.Project(enemyVelocity, transform.up) + Vector3.ProjectOnPlane(dir, transform.up).normalized * dir.magnitude;
    }

    public void GoToDestination(Vector3 dest, float speed)
    {
        if((transform.position-dest).sqrMagnitude > 4)
        {
            animancer.Play(enemyConfig.walkAnimation);
            //Debug.DrawRay(transform.position, agent.desiredVelocity, Color.red, 2f);
            agent.destination = dest;
            GoInDirection(Vector3.ProjectOnPlane(agent.desiredVelocity, transform.up).normalized * speed + CalculateAvoidance());
            //RotateInDirection(Vector3.ProjectOnPlane(agent.desiredVelocity, transform.up).normalized);
        }
    }

    public void RotateInDirection()
    {
        Vector3 flatDir = Vector3.ProjectOnPlane(lookDir, transform.up);
        if (flatDir.sqrMagnitude < 0.001f) return; // Skip rotation if direction is too small
        
        Quaternion targetRotation = Quaternion.LookRotation(flatDir.normalized, transform.up);
        //Debug.DrawRay(transform.position, dir*4, Color.red, 2f);
        //Debug.Log("Angle: " + Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * enemyConfig.rotationSpeed) + "Rot: " + transform.rotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * enemyConfig.rotationSpeed);
    }

    public void FlyInDirection(Vector3 dir)
    {
        Quaternion targetRotation = Quaternion.LookRotation(dir);

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
                    return randomPoint = hit.position;
                }
            }
        }

        return randomPoint = transform.position;
    }

    IEnumerator TraverseLink()
    {
        isTraversingLink = true;
        
        
        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;
        
        if(endPos.y > startPos.y)
        {
            
            enemyVelocity += Vector3.ProjectOnPlane(endPos - transform.position, transform.up).normalized * 0.3f;

            //JumpTo(endPos);

            yield return null;
            while (!movementController.GroundCheck())
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        
        agent.CompleteOffMeshLink();
        
        isTraversingLink = false;
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
        
        // Find nearby agents
        Collider[] nearbyAgents = Physics.OverlapSphere(transform.position, avoidanceRadius, enemyLayer);
        
        foreach (Collider col in nearbyAgents)
        {
            //Debug.Log("Name " + col.name);
            if (col.gameObject == gameObject) continue;
            
            Vector3 directionAway = transform.position - col.transform.position;
            float distance = directionAway.magnitude;
            
            if (distance > 0)
            {
                // Stronger avoidance when closer
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

    public IEnumerator ChangeState(EnemyBaseState state, float x, float y)
    {
        yield return new WaitForSeconds(Random.Range(x, y));
        SwitchState(state);
    }

    public Vector3 GetRandomDirection()
    {
        idleDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        idleDir = Vector3.ProjectOnPlane(idleDir, transform.up).normalized;
        return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
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
