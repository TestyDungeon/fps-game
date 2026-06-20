using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MovementController : MonoBehaviour
{

    //private Transform transform;
    private CapsuleCollider capsuleCollider;
    private float capsuleColliderRadius;
    private float capsuleColliderHeight;
    [Header("Gravity")]
    [SerializeField] private bool GravityEnabled = true;
    [SerializeField] private bool GlobalGravityEnabled = true;
    [SerializeField] private float gravity;
    private float currentGravityAlignSpeed = 0.5f;
    [SerializeField] private float targetGravityAlignSpeed = 0.5f;
    [SerializeField] private float gravityAlignSpeedOnFieldChange = 0.02f;
    [SerializeField] private float gravityAlignStep = 0.01f;
    
    [Header("Parameters")]
    [SerializeField] private float maxClimbAngle = 55;
    [SerializeField] private float stepHeight = 0.25f;
    private bool InGravityField = false;
    private int maxRecursion = 3;
    private int recursionDepth;
    float offset = 0.01f;
    private float stepOffset;
    
    private Vector3 dashDir;
    private float dashSpeed;
    private bool dashing = false;

    Vector3 externalVelocity = Vector3.zero;
    Vector3 vel = Vector3.zero;
    Vector3 gravityVec = Vector3.down;
    Vector3 changedDir = Vector3.zero;

    int layerMaskEnemy = ~(1 << 6 | 1 << 12 | 1 << 10);
    int layerMaskPlayer = ~(1 << 3 | 1 << 6 | 1 << 12 | 1 << 10);
    int layerMaskPlayerDash = ~(1 << 3 | 1 << 6 | 1 << 12 | 1 << 10 | 1 << 8);
    [HideInInspector] public int layerMask;

    Coroutine dashCoroutine;

    void Awake()
    {
        if (tag == "Player")
        {
            layerMask = layerMaskPlayer;
        }
        else if (tag == "Enemy")
        {
            layerMask = layerMaskEnemy;
        }
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleColliderRadius = capsuleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        capsuleColliderHeight = capsuleCollider.height * transform.lossyScale.y;
        stepOffset = -(capsuleColliderHeight / 2) + stepHeight;
    }

    public Vector3 Move(Vector3 velocity)
    {
        bool wasGrounded = GroundCheck();
        if (dashing)
        {
            if(GroundCheck(out RaycastHit hit))
            {
                if(Vector3.Angle(hit.normal, dashDir) >= 90 && Vector3.Angle(transform.up, hit.normal) < maxClimbAngle)
                    dashDir = mathlib.ProjectOnPlaneOblique(dashDir, hit.normal, -transform.up);
            }
            Debug.Log("Dash speed: " + dashSpeed);
            Vector3 dashMove = CollideAndSlide(transform.position, dashDir * dashSpeed * Time.fixedDeltaTime, false);
            //Collider[] cols = Physics.OverlapCapsule(
            //transform.position + transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            //transform.position - transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            //capsuleColliderRadius,
            //dashing ? layerMaskPlayerDash : layerMask, QueryTriggerInteraction.Ignore);
        
            transform.position += dashMove;
            externalVelocity = Vector3.zero;
            vel = Vector3.zero;

            return vel;
        }

        if (changedDir != Vector3.zero)
        {
            velocity = changedDir;
            changedDir = Vector3.zero;
        }
        if (GravityEnabled)
        {
            if (!InGravityField && GlobalGravityEnabled)
                gravityVec = Vector3.down;
            if (!InGravityField && !GlobalGravityEnabled)
                gravityVec = Vector3.zero;
                
            //if (InGravityField)
            velocity += gravityVec * gravity * Time.fixedDeltaTime;

            GravityOrientation();
            ResolvePenetration();
        }

        velocity += externalVelocity;
        externalVelocity = Vector3.zero;
        
        Vector3 displacement = velocity * Time.fixedDeltaTime;

        Vector3 up = transform.up;
        Vector3 verticalDisp = Vector3.Project(displacement, up);
        Vector3 lateralDisp = displacement - verticalDisp;

        

        recursionDepth = 0;
        Vector3 resolvedLateral = CollideAndSlide(transform.position, lateralDisp, false);
        
        Vector3 stepUp = Vector3.zero;
        if(resolvedLateral.sqrMagnitude < lateralDisp.sqrMagnitude)
        {
            recursionDepth = 0;
            Vector3 resolvedLateralStepUp = CollideAndSlide(transform.position, transform.up * stepHeight, true);
            recursionDepth = 0;
            Vector3 resolvedLateralStepForward = CollideAndSlide(transform.position + resolvedLateralStepUp, lateralDisp, true);
            if(resolvedLateralStepForward.sqrMagnitude > resolvedLateral.sqrMagnitude && Mathf.Approximately(resolvedLateralStepUp.sqrMagnitude, stepHeight * stepHeight))
            {
                recursionDepth = 0;
                Vector3 resolvedLateralStepDown = CollideAndSlide(transform.position + resolvedLateralStepUp * 2 + resolvedLateralStepForward, -transform.up * stepHeight * 2, true);

                Vector3 onStepPosition = (transform.position + resolvedLateralStepUp * 2) + resolvedLateralStepForward + resolvedLateralStepDown;
                Vector3 onStepVector = onStepPosition - transform.position;

                if(Vector3.ProjectOnPlane(onStepVector, transform.up).sqrMagnitude > resolvedLateral.sqrMagnitude)
                {
                    resolvedLateral = Vector3.ProjectOnPlane(onStepVector, transform.up);
                    stepUp = Vector3.Project(onStepVector, transform.up);
                }
            }
            
        }
        

        recursionDepth = 0;
        Vector3 resolvedVertical = CollideAndSlide(transform.position + resolvedLateral + stepUp, verticalDisp, true);
        //ResolvePenetration();
        
        

        Vector3 pusherVelocity = Vector3.zero;

        if (GroundCheck(out RaycastHit hit1) && hit1.transform.CompareTag("Pusher"))
        {
            Debug.Log("Pusher");
            pusherVelocity = CollideAndSlide(transform.position + resolvedLateral + resolvedVertical, hit1.transform.gameObject.GetComponent<Pusher>().getDelta(), false);
        }
        
        

        

        transform.position += resolvedLateral + stepUp + resolvedVertical + pusherVelocity;

        
        Vector3 totalResolved = resolvedLateral + resolvedVertical;
        vel = totalResolved / Time.fixedDeltaTime;
        return vel;
    }



    private Vector3 CollideAndSlide(Vector3 pos, Vector3 vel, bool GravityPass)
    {
        if (recursionDepth > maxRecursion)
            return Vector3.zero;

        float dist = vel.magnitude + offset;
        
        if (Physics.CapsuleCast(
            pos + transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            pos - transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            capsuleColliderRadius, vel.normalized, out RaycastHit hit, dist,
            dashing ? layerMaskPlayerDash : layerMask, QueryTriggerInteraction.Ignore))
        {

            Vector3 newVel = vel.normalized * (hit.distance - offset);
            float angle = Vector3.Angle(transform.up, hit.normal);

            if (newVel.magnitude <= offset)
                newVel = Vector3.zero;


            Vector3 newPos = pos + newVel;

            Vector3 vecOnPlane = Vector3.ProjectOnPlane(vel - newVel, hit.normal);

            if (GravityPass && angle < maxClimbAngle)
                return newVel;

            recursionDepth++;
            return newVel + CollideAndSlide(newPos, vecOnPlane, GravityPass);
        }
        return vel;
    }


    private void GravityOrientation()
    {
        if (currentGravityAlignSpeed != targetGravityAlignSpeed)
        {
            currentGravityAlignSpeed = Mathf.MoveTowards(currentGravityAlignSpeed, targetGravityAlignSpeed, gravityAlignStep * Time.fixedDeltaTime);
        }

        //f (gravityAlignStep != 0.1)
        //
        //   gravityAlignStep = Mathf.MoveTowards(gravityAlignStep, 0.1f, 0.005f * Time.fixedDeltaTime);
        //

        Vector3 lowestPoint = transform.position - transform.up * (capsuleColliderHeight / 2);
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityVec) * transform.rotation;
        
        Quaternion currentRotation = transform.rotation;
        Quaternion newRotation = Quaternion.Slerp(currentRotation, targetRotation, currentGravityAlignSpeed);
        
        // Calculate the offset from the lowest point
        Vector3 offset = transform.position - lowestPoint;
        
        // Rotate the offset according to the rotation change
        Quaternion deltaRotation = newRotation * Quaternion.Inverse(currentRotation);
        Vector3 rotatedOffset = deltaRotation * offset;
        
        // Apply new rotation and adjusted position
        transform.rotation = newRotation;
        transform.position = lowestPoint + rotatedOffset;
    }

    private void ResolvePenetration(int recursion_ = 0)
    {
        int recursion = recursion_;
        if(recursion >= 10)
            return;

        Collider[] overlap = Physics.OverlapCapsule(
            transform.position + transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            transform.position - transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            capsuleColliderRadius,
            layerMask, QueryTriggerInteraction.Ignore);
            
        if (overlap.Length > 0)
        {
            foreach (Collider x in overlap)
            {
                if (x == capsuleCollider)
                    continue;
                //Debug.Log(x.name + x);
                if (Physics.ComputePenetration(
                    capsuleCollider, transform.position, transform.rotation,
                    x, x.transform.position, x.transform.rotation,
                    out Vector3 dir, out float dis))
                {
                    transform.position += dir * (dis + 0.1f);
                    Debug.DrawRay(transform.position, dir * (dis + 0.1f), Color.cyan, 1);
                }
            }
            ResolvePenetration(recursion + 1);
        }
    }

    public IEnumerator Dash(float dur, float postDashSpeed = 0)
    {
        dashing = true;

        yield return new WaitForSeconds(dur);
        dashing = false;
        addVelocity(dashDir * postDashSpeed);
    }


    public bool GroundCheck()
    {
        if (Physics.SphereCast(transform.position, capsuleColliderRadius, -transform.up, out RaycastHit hit, capsuleColliderHeight/4 + 0.3f, layerMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        return false;
    }
    public bool GroundCheck(out RaycastHit hit)
    {
        if (Physics.SphereCast(transform.position, capsuleColliderRadius, -transform.up, out hit, capsuleColliderHeight/4 + 0.3f, layerMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        return false;
    }

    public void addVelocity(Vector3 x)
    {
        externalVelocity += x;
    }


    public Vector3 getVelocity()
    {
        return vel;
    }

    public void setVelocityDir(Vector3 x)
    {
        changedDir = x;
    }

    public void SetVelocitySpeed(float x)
    {
        vel = vel.normalized * x;
    }

    public void resetVerticalVelocity()
    {
        externalVelocity -= Vector3.Project(vel, transform.up);
    }

    public void resetNegativeVerticalVelocity()
    {
        externalVelocity -= Vector3.Dot(vel, transform.up) < 0 
        ? Vector3.Project(vel, transform.up) 
        : Vector3.zero;
    }

    public void resetVelocity()
    {
        externalVelocity -= vel;
    }

    public void setGravityVec(Vector3 x)
    {
        gravityVec = x;
    }

    public float GetGravityAlignSpeed()
    {
        return currentGravityAlignSpeed;
    }

    public float GetGravityAlignSpeedOnFieldChange()
    {
        return gravityAlignSpeedOnFieldChange;
    }

    public void setGravityAlignSpeed(float x)
    {
        currentGravityAlignSpeed = x;
    }

    public float GetGravityAlignStep()
    {
        return gravityAlignStep;
    }

    public void SetGravityAlignStep(float x)
    {
        gravityAlignStep = x;
    }

    public float getGravity()
    {
        return gravity;
    }

    public void SetGravity(float gravity_)
    {
        gravity = gravity_;
    }

    public void setInGravityField(bool x)
    {
        InGravityField = x;
    }

    public bool getInGravityField()
    {
        return InGravityField;
    }

    public float GetMaxClimbAngle()
    {
        return maxClimbAngle;
    }

    public bool GetIsDashing()
    {
        return dashing;
    }

    public Vector3 GetDashDir()
    {
        return dashDir;
    }

    public void SetDashDir(Vector3 dir)
    {
        dashDir = dir;
    }

    public float GetDashSpeed()
    {
        return dashSpeed;
    }

    public void SetDashSpeed(float speed)
    {
        dashSpeed = speed;
    }

    public void Dash(Vector3 dir, float dist, float speed, float postDashSpeed = 0)
    {
        StopDash();
        SetDashDir(dir.normalized);
        dashCoroutine = StartCoroutine(Dash(dist/speed, postDashSpeed));
        dashSpeed = speed;
    }

    public void StopDash()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
            dashing = false;
            dashCoroutine = null;
        }
    }
}
