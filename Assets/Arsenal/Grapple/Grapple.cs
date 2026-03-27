using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Grapple : Item
{
    private LineRenderer lr;
    [SerializeField] private float range;
    [SerializeField] private float timeWindow;
    [SerializeField] private float grappleDelay;
    [SerializeField] private float minForce;
    [SerializeField] private float maxForce;
    [SerializeField] private float upForce;
    [SerializeField] private float animSpeed;

    [SerializeField] private Transform grappleStart;
    private float g;
    private float speed = 5;
    private float currentSpeed;
    private bool grappling = false;
    private bool swinging;
    private Vector3 grapplePoint;
    Vector3 velocity;
    private RaycastHit hit;
    private MovementController mc = null;
    private LayerMask layerMask = ~(1 << 9);

    void Awake()
    {
        currentSpeed = speed;
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
    }

    void Start()
    {
        g = player.GetComponent<MovementController>().getGravity();
        mc = player.GetComponent<MovementController>();
    } 

    void Update()
    {
        //Debug.Log(swinging);
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //grappling = true;
            StartGrapple();
            //player.GetComponent<MovementController>().resetVelocity();
            velocity = mc.getVelocity();
            currentSpeed = Vector3.Dot((grapplePoint - player.transform.position).normalized, velocity);
        }
        
        if (Input.GetKey(KeyCode.Q))
        {
            LaunchGrapple();
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            CancelInvoke();
            if (grappling)
            {
                grappling = false;
                StartCoroutine(AnimateLineOut(lr.GetPosition(1), grappleStart.position));
                lr.enabled = false;
                //Invoke(nameof(LaunchGrapple), grappleDelay);
            }
            //if (swinging)
            //{
            //    swinging = false;
            //    StartCoroutine(AnimateLineOut(grapplePoint, grappleStart.position));
            //    lr.enabled = false;
            //}
        }

        //if (swinging)
        //{
        //    Swing();
        //}
        
    }

    void LateUpdate()
    {
        //if (grappling || swinging)
        //{
           lr.SetPosition(0, grappleStart.position);
        //}
    }

    private void Swing()
    {
        Vector3 vel = mc.getVelocity();
        Vector3 swingVelocity = Vector3.ProjectOnPlane(vel, transform.position - hit.point).normalized;
        mc.setVelocityDir(swingVelocity);
        //mc.addVelocity(swingVelocity);
    }


    private void StartGrapple()
    {
        Vector3 dir = cameraPivot.forward;
        if (Physics.Raycast(cameraPivot.position, dir, out hit, range, layerMask, QueryTriggerInteraction.Ignore))
        {
            SoundManager.PlaySound(SoundType.GRAPPLE, 0.6f);
            grapplePoint = hit.point;
            lr.enabled = true;
            grappling = true;
            //Invoke("StopGrappling", timeWindow);
            StartCoroutine(AnimateLineOut(grappleStart.position, grapplePoint));

            
            //Invoke(nameof(StopGrappling), 0.2f);

        }

    }

    private void LaunchGrapple()
    {
        //player.GetComponent<PlayerMovement>().append_vel(Vector3.up * 10);\
        //CancelInvoke();
        //player.GetComponent<MovementController>().resetVelocity();
        //player.GetComponent<MovementController>().addVelocity((grapplePoint - player.transform.position).normalized * 0.6f - (grapplePoint - player.transform.position) * 0.6f);
        //player.GetComponent<MovementController>().SetVelocitySpeed(15);
        currentSpeed += 20 * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, (grapplePoint - player.transform.position).normalized * currentSpeed, Time.deltaTime * 10);
        player.GetComponent<MovementController>().setVelocityDir(velocity);

    }

    private IEnumerator AnimateLineOut(Vector3 start, Vector3 end)
    {
        float t = 0f;
        Vector3 currentEnd = start;
        while ((currentEnd - end).sqrMagnitude > 0.25)
        {
            t += Time.deltaTime * animSpeed;
            //Debug.Log("OUT");
            currentEnd += (end - currentEnd).normalized * Time.deltaTime * animSpeed;
            //currentEnd = Vector3.Lerp(currentEnd, end, Time.deltaTime * animSpeed);
            lr.SetPosition(1, currentEnd);

            yield return null;
        }
        if(!grappling)
            lr.enabled = false;
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

}
