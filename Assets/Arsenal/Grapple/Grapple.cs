using System.Collections;
using UnityEngine;
using DG.Tweening;
using GravityGUN.Data;

public class Grapple : Item
{
    private LineRenderer lr;
    [SerializeField] private float range;
    [SerializeField] private float startSpeed = 10;
    [SerializeField] private float maxSpeed = 30;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float cooldown = 2f;
    private float lastTime = -1;
    [SerializeField] private float animSpeed;

    [SerializeField] private Transform grappleStart;
    private float g;
    private float currentSpeed;
    private bool grappling = false;
    private bool started = false;
    private Transform targetTransform = null;
    private Vector3 grapplePoint;
    private RaycastHit hit;
    private MovementController mc = null;
    private AudioSource audioSource = null;
    private LayerMask layerMask = (1 << 0 | 1 << 8);

    void Awake()
    {
        currentSpeed = startSpeed;
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
    }

    protected override void Start()
    {
        base.Start();
        g = player.GetComponent<MovementController>().getGravity();
        mc = player.GetComponent<MovementController>();
    } 

    void Update()
    {
        if (!started && Input.GetKeyDown(KeyCode.Q) && (Time.time - lastTime) >= cooldown)
        {
            StartGrapple();
            
        }

        if(grappling && 
        (Vector3.Distance(player.transform.position, grapplePoint) < 3 ||
        Input.GetKeyDown(KeyCode.LeftShift) || 
        Input.GetKeyDown(KeyCode.Space)))
        {
            StopGrapple();
        }

    }

    void FixedUpdate()
    {
        if(grappling)
        {
            UpdateGrapplePoint();
            if(currentSpeed < maxSpeed)
                currentSpeed *= Mathf.Pow(acceleration, Time.fixedDeltaTime);
            //Debug.Log("Current Speed: " + currentSpeed);
            mc.Dash((grapplePoint - player.transform.position).normalized, Vector3.Distance(grapplePoint, player.transform.position) - 3, currentSpeed);
        }
    }

    void LateUpdate()
    {
        if(grappling)
        {
            lr.SetPosition(1, grappleStart.position);
            UpdateGrapplePoint();
            lr.SetPosition(0, grapplePoint);
        }
    }

    


    private void StartGrapple()
    {
        Vector3 dir = cameraPivot.forward;
        if (Physics.Raycast(cameraPivot.position, dir, out hit, range, layerMask, QueryTriggerInteraction.Collide)
        || Physics.SphereCast(cameraPivot.position, 2, dir, out hit, range, layerMask, QueryTriggerInteraction.Collide))
        {
            if (!hit.transform.CompareTag("Enemy"))
                return;

            started = true;
            targetTransform = hit.transform;
            if(hit.transform.TryGetComponent(out EnemyStateManager esm))
                esm.SwitchState(esm.FalterState);

            audioSource = SoundManager.PlayLoop(SoundType.GRAPPLE, 0.4f);
            grapplePoint = hit.point;
            lr.enabled = true;
            
            StartCoroutine(AnimateLineOut(grappleStart.position));
        }
    }

    private void StopGrapple()
    {
        Vector3 dir = mc.GetDashDir();
        lastTime = Time.time;
        if(Input.GetKeyDown(KeyCode.Space))
        {
            mc.StopDash();
            mc.addVelocity(dir.normalized * currentSpeed);
            SoundManager.PlaySound(SoundType.GRAPPLE_END, 0.4f);
        }
        else if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            mc.StopDash();
        }
        else if(Vector3.Distance(player.transform.position, grapplePoint) < 3)
        {
            mc.StopDash();
            mc.addVelocity(dir.normalized * currentSpeed);
            SoundManager.PlaySound(SoundType.GRAPPLE_END, 0.4f);
        }
        CancelInvoke();
        SoundManager.StopLoop(audioSource, 1);
        started = false;
        targetTransform = null;
        inventory.AddAmmo(LootType.MeleeAmmo, 1);
        if (grappling)
        {
            grappling = false;
            //StartCoroutine(AnimateLineOut(lr.GetPosition(1)));
            lr.enabled = false;
        }
    }

    private void LaunchGrapple()
    {
        currentSpeed = Mathf.Max(Vector3.Dot((grapplePoint - player.transform.position).normalized, mc.getVelocity()), startSpeed);
        mc.resetVelocity();
        mc.Dash((grapplePoint - player.transform.position).normalized, Vector3.Distance(grapplePoint, player.transform.position) - 3, currentSpeed, currentSpeed / 2);
    }

    private IEnumerator AnimateLineOut(Vector3 start)
    {
        Vector3 currentEnd = start;
        while (currentEnd != grapplePoint)
        {
            UpdateGrapplePoint();
            lr.SetPosition(1, grappleStart.position);
            currentEnd = Vector3.MoveTowards(currentEnd, grapplePoint, animSpeed * Time.deltaTime);
            lr.SetPosition(0, currentEnd);

            yield return null;
        }
        LaunchGrapple();
        grappling = true;
    }


    private void UpdateGrapplePoint()
    {
        if(targetTransform != null)
        {
            grapplePoint = targetTransform.position;
            mc.SetDashDir((grapplePoint - player.transform.position).normalized);
        }
    }
    /*
    private void Swing()
    {
        Vector3 vel = mc.getVelocity();
        Vector3 swingVelocity = Vector3.ProjectOnPlane(vel, transform.position - hit.point).normalized;
        mc.setVelocityDir(swingVelocity);
        //mc.addVelocity(swingVelocity);
    }


    private void StopGrappling()
    {
        grappling = false;
        swinging = true;
    }

    private Vector3 CalculateVelocity(Vector3 start, Vector3 end)
    {
        Vector3 vel = end - start;
        vel = vel.normalized * Mathf.Clamp(vel.magnitude, minForce * range, maxForce * range) + transform.up * upForce;
        return vel; 
    }   


    
    private Vector3 CalculateVelocityOld(Vector3 start, Vector3 end)
    {
        Vector3 up = player.transform.up;

        // Bottom of the player (for reference)
        float playerHeight = player.GetComponent<CapsuleCollider>().height;
        Vector3 lowestPoint = player.transform.position - up * (playerHeight / 2f);

        // Vertical displacement from player bottom to grapple point
        float grappleY = Vector3.Dot(end - lowestPoint, up);

        // Determine the apex of the arc
        float apexHeight = grappleY + upForce; // desired arc above grapple point

        // Make sure apex is always above player's bottom
        apexHeight = Mathf.Max(apexHeight, 0.5f); // minimum 0.5 meters to avoid NaN

        // Total displacement
        Vector3 displacement = end - start;

        // Vertical and horizontal components
        float displacementY = Vector3.Dot(displacement, up); // vertical
        Vector3 displacementXZ = Vector3.ProjectOnPlane(displacement, up); // horizontal

        // Gravity
         // positive number, magnitude of downward acceleration

        // Vertical velocity to reach apex
        float velocityY = Mathf.Sqrt(2f * g * apexHeight);

        // Time to reach apex
        float timeUp = velocityY / g;

        // Time to fall from apex to target
        float fallHeight = apexHeight - displacementY;
        float timeDown = Mathf.Sqrt(Mathf.Max(2f * fallHeight / g, 0.01f)); // avoid sqrt(0)

        float totalTime = timeUp + timeDown;

        // Horizontal velocity needed
        Vector3 velocityXZ = displacementXZ / totalTime;

        // Final velocity
        return 1.1f*(velocityXZ + up * velocityY);
    }
    */
}
