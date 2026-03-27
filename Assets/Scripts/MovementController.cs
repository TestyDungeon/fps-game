using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MovementController : MonoBehaviour
{

    //private Transform transform;
    private CapsuleCollider capsuleCollider;
    private float capsuleColliderRadius;
    private float capsuleColliderHeight;
    [SerializeField] private bool GravityEnabled = true;
    [SerializeField] private bool GlobalGravityEnabled = true;
    [SerializeField] private float gravity;
    private float gravityAlignSpeed = 0.05f;
    [SerializeField] private float gravityAlignStep = 0.01f;
    
    [SerializeField] private float maxClimbAngle = 55;
    private bool InGravityField = false;
    private int maxRecursion = 3;
    private int recursionDepth;
    float offset = 0.01f;
    private float stepHeight = 0.25f;
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
    public int layerMask;

    

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
            Vector3 dashMove = CollideAndSlide(transform.position, dashDir * dashSpeed * Time.fixedDeltaTime, false);
            Collider[] cols = Physics.OverlapCapsule(
            transform.position + transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            transform.position - transform.up * (capsuleColliderHeight / 2 - capsuleColliderRadius),
            capsuleColliderRadius,
            dashing ? layerMaskPlayerDash : layerMask, QueryTriggerInteraction.Ignore);

            foreach(Collider col in cols)
            {
                
            }
        
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
        

        recursionDepth = 0;
        Vector3 resolvedVertical = CollideAndSlide(transform.position + resolvedLateral, verticalDisp, true);
        ResolvePenetration();
        
        

        Vector3 pusherVelocity = Vector3.zero;

        Collider[] overlap = Physics.OverlapCapsule(transform.position + transform.up * 0.5f, transform.position - transform.up * 0.5f, 0.6f, 1, QueryTriggerInteraction.Ignore);

        if (overlap.Length > 0)
        {
            foreach (Collider x in overlap)
            {
                if (x.CompareTag("Pusher"))
                {
                    pusherVelocity = CollideAndSlide(transform.position + resolvedLateral + resolvedVertical, x.gameObject.GetComponent<Pusher>().getDelta(), false);
                }
            }
        }
        Vector3 slopeSticking = Vector3.zero;

        

        transform.position += resolvedVertical + resolvedLateral + pusherVelocity + slopeSticking;

        
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
        if (gravityAlignSpeed != 0.5)
        {
            gravityAlignSpeed = Mathf.Lerp(gravityAlignSpeed, 0.5f, gravityAlignStep);
        }

        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityVec) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, gravityAlignSpeed);
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

    public void setGravityAlignSpeed(float x)
    {
        gravityAlignSpeed = x;
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

    public void SetDashDir(Vector3 dir)
    {
        dashDir = dir;
    }

    public void Dash(Vector3 dir, float dist, float speed, float postDashSpeed = 0)
    {
        SetDashDir(dir.normalized);
        StartCoroutine(Dash(dist/speed, postDashSpeed));
        dashSpeed = speed;
    }
}
