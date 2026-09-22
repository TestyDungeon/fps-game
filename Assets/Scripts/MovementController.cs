using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float stepClearance = 0.02f;
    private bool InGravityField = false;
    private int maxRecursion = 3;
    private int recursionDepth;
    float offset = 0.01f;
    
    private Vector3 dashDir;
    private float dashSpeed;
    private bool dashing = false;

    Vector3 externalVelocity = Vector3.zero;
    Vector3 vel = Vector3.zero;
    Vector3 gravityVec = Vector3.down;
    Vector3 changedDir = Vector3.zero;


    private bool hitWall;
    private RaycastHit wallHit;

    private float pendingStepSmooth = 0;

    int layerMaskEnemy = 1 << 0 | 1 << 3 | 1 << 8 | 1 << 14;
    int layerMaskEnemyStep = 1 << 0 | 1 << 3;
    int layerMaskEnemyDead = ~(1 << 3 | 1 << 6 | 1 << 12 | 1 << 10);
    int layerMaskPlayer = ~(1 << 3 | 1 << 6 | 1 << 12 | 1 << 10);
    int layerMaskPlayerDash = ~(1 << 3 | 1 << 6 | 1 << 12 | 1 << 10 | 1 << 8);
    [HideInInspector] public int layerMask;
    [HideInInspector] public int layerMaskStep;


    Coroutine dashCoroutine;

    void Awake()
    {
        if (tag == "Player")
        {
            layerMask = layerMaskPlayer;
            layerMaskStep = layerMaskPlayer;
        }
        else if (tag == "Enemy")
        {
            layerMask = layerMaskEnemy;
            layerMaskStep = layerMaskEnemyStep;
            
        }
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleColliderRadius = capsuleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        capsuleColliderHeight = capsuleCollider.height * transform.lossyScale.y;
    }

    public Vector3 Move(Vector3 velocity)
    {
        //Debug.DrawRay(transform.position, gravityVec * 5, Color.cyan);
        bool wasGrounded = GroundCheck();

        velocity += externalVelocity;
        

        if (dashing)
        {
            if(GroundCheck(out RaycastHit hit))
            {
                if(Vector3.Angle(hit.normal, dashDir) >= 90 && Vector3.Angle(transform.up, hit.normal) < maxClimbAngle)
                    dashDir = mathlib.ProjectOnPlaneOblique(dashDir, hit.normal, -transform.up);
            }
            //Debug.Log("Dash speed: " + dashSpeed);
            recursionDepth = 0;
            Vector3 dashMove = CollideAndSlide(transform.position, ((dashDir * dashSpeed) + velocity) * Time.fixedDeltaTime, false);
            //Collider[] cols = Physics.OverlapCapsule(
            //transform.position + transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            //transform.position - transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            //capsuleColliderRadius,
            //dashing ? layerMaskPlayerDash : layerMask, QueryTriggerInteraction.Ignore);
        
            transform.position += dashMove;
            externalVelocity = Vector3.zero;
            vel = Vector3.zero;

            return velocity * 0.95f;
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

        
        externalVelocity = Vector3.zero;
        
        Vector3 displacement = velocity * Time.fixedDeltaTime;

        Vector3 up = transform.up;
        Vector3 verticalDisp = Vector3.Project(displacement, up);
        Vector3 lateralDisp = displacement - verticalDisp;

        

        recursionDepth = 0;
        hitWall = false;
        Vector3 resolvedLateral = CollideAndSlide(transform.position, lateralDisp, false);

        Vector3 stepUp = Vector3.zero;
        if (hitWall)
            Debug.Log($"[{tag}] hitWall={hitWall} wasGrounded={wasGrounded} — attempting TryStep");
        if (hitWall && wasGrounded && TryStep(lateralDisp, wallHit, out float lift))
        {
            stepUp = transform.up * lift;
            recursionDepth = 0;
            resolvedLateral = CollideAndSlide(transform.position + stepUp, lateralDisp, false);
            pendingStepSmooth += lift;   // for the camera, below
            //Player.Instance.CameraRecoil.StepSmooth(lift);
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

        StepDownSnap(wasGrounded, velocity, stepUp);

        return vel;
    }



    private Vector3 CollideAndSlide(Vector3 pos, Vector3 vel, bool GravityPass)
    {
        if (recursionDepth > maxRecursion)
            return Vector3.zero;

        float dist = vel.magnitude + offset;
        
        CapsulePoints(pos, out Vector3 p1, out Vector3 p2);

        if (Physics.CapsuleCast(
            p1,
            p2,
            capsuleColliderRadius, vel.normalized, out RaycastHit hit, dist,
            dashing && tag == "Player" ? layerMaskPlayerDash : layerMask, QueryTriggerInteraction.Ignore))
        {
            //if(tag == "Player")
            //{
            //    Debug.Log("Collision: " + hit.transform.name);
            //}
            Vector3 newVel = vel.normalized * (hit.distance - offset);
            float angle = Vector3.Angle(transform.up, hit.normal);
            if (!GravityPass)
                Debug.Log($"[{tag}] cast hit {hit.collider.name} angle={angle:F1} normal={hit.normal} climbLimit={maxClimbAngle} depth={recursionDepth}");
            if (!GravityPass && !hitWall && angle > maxClimbAngle)
            {
                hitWall = true;
                wallHit = hit;
            }

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

    

    private void CapsulePoints(Vector3 pos, out Vector3 p1, out Vector3 p2)
    {
        Vector3 h = transform.up * (capsuleColliderHeight * 0.5f - capsuleColliderRadius);
        p1 = pos + h;
        p2 = pos - h;
    }

    private bool TryStep(Vector3 lateralDisp, RaycastHit wall, out float lift)
    {
        lift = 0f;
        Vector3 up   = transform.up;
        Vector3 dir  = Vector3.ProjectOnPlane(lateralDisp, up).normalized;
        Vector3 foot = transform.position - up * (capsuleColliderHeight * 0.5f);

        // wall point brought down to foot height, nudged just past the face
        Vector3 onFace   = wall.point - Vector3.Project(wall.point - foot, up);
        Vector3 rayStart = onFace + dir * (offset * 2f) + up * (stepHeight + offset);

        RaycastHit[] treads = Physics.RaycastAll(rayStart, -up, stepHeight + offset * 2f,
                     layerMaskStep, QueryTriggerInteraction.Ignore);

        if (treads.Length <= 0)
        {
            Debug.Log($"[{tag}] TryStep: no tread found from {rayStart}");
            return false;                                   // ledge, nothing to stand on
        }
        else
        {
            foreach(RaycastHit tread in treads)
            {
                if(tread.transform == transform)
                    continue;

                if (Vector3.Angle(up, tread.normal) > maxClimbAngle)
                {
                    Debug.Log($"[{tag}] TryStep: tread too steep ({Vector3.Angle(up, tread.normal):F1})");
                    return false;                                   // too steep to stand on
                }

                lift = Vector3.Dot(tread.point - foot, up);
                if (lift <= offset || lift > stepHeight)
                {
                    Debug.Log($"[{tag}] TryStep: lift {lift:F3} out of range (stepHeight={stepHeight})");
                    return false;
                }

                lift += stepClearance;                              // clear the edge with the round bottom

                CapsulePoints(transform.position + up * lift, out Vector3 a, out Vector3 b);
                bool blocked = Physics.CheckCapsule(a, b, capsuleColliderRadius - 0.02f,
                                             layerMaskStep, QueryTriggerInteraction.Ignore);
                if (blocked)
                    Debug.Log($"[{tag}] TryStep: capsule blocked after lift {lift:F3}");
                return !blocked;
            }
            return false;
        }

        
    }

    public float ConsumeStepSmooth()
    {
        float x = pendingStepSmooth;
        pendingStepSmooth = 0;
        return x;
    }

    private void StepDownSnap(bool wasGrounded, Vector3 velocity, Vector3 stepUp)
    {
        if (!wasGrounded || stepUp != Vector3.zero) return;
        if (Vector3.Dot(velocity, transform.up) > 0f) return;   // jumping/being launched — let it fly

        Vector3 up = transform.up;
        CapsulePoints(transform.position, out Vector3 a, out Vector3 b);

        if (Physics.CapsuleCast(a, b, capsuleColliderRadius, -up, out RaycastHit g,
                                 stepHeight + offset, layerMask, QueryTriggerInteraction.Ignore)
            && Vector3.Angle(up, g.normal) <= maxClimbAngle)
        {
            float drop = Mathf.Max(g.distance - offset, 0f);
            if (drop > 0f)
            {
                transform.position -= up * drop;
                pendingStepSmooth -= drop;   // camera eases the drop the same way it eases a rise
            }
        }
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
                    //Debug.DrawRay(transform.position, dir * (dis + 0.1f), Color.cyan, 1);
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
        if (Physics.SphereCast(transform.position, capsuleColliderRadius - 0.01f, -transform.up, out RaycastHit hit, capsuleColliderHeight/4 + 0.3f, layerMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        return false;
    }
    public bool GroundCheck(out RaycastHit hit)
    {
        if (Physics.SphereCast(transform.position, capsuleColliderRadius - 0.01f, -transform.up, out hit, capsuleColliderHeight/4 + 0.3f, layerMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        return false;
    }

    public void addVelocity(Vector3 x)
    {
        externalVelocity += x;
        //Debug.Log("Vel: " + x.magnitude);
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

    public float GetVerticalSpeed()
    {
        return Vector3.Dot(vel, transform.up);
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

    public Vector3 GetGravityVec()
    {
        return gravityVec;
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

    public void SetEnemyLayerMaskToDead()
    {
        layerMask = layerMaskEnemyDead;
        
    }
}
