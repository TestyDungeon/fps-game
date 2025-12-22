using UnityEngine;

public class MovementController : MonoBehaviour
{

    //private Transform transform;
    private CapsuleCollider capsuleCollider;
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

    Vector3 gravityVelocity;
    Vector3 externalVelocity = Vector3.zero;
    Vector3 vel = Vector3.zero;
    Vector3 gravityVec = Vector3.down;
    Vector3 changedDir = Vector3.zero;

    int layerMaskEnemy = ~(1 << 6);
    int layerMaskPlayer = ~(1 << 3 | 1 << 6);
    int layerMask;

    //public MovementController(Transform transform_, CapsuleCollider capsuleCollider_)
    //{
    //    transform = transform_;
    //    capsuleCollider = capsuleCollider_;
    //}

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
    }

    public Vector3 Move(Vector3 velocity)
    {
        if (changedDir != Vector3.zero)
        {
            velocity = changedDir * velocity.magnitude;
            changedDir = Vector3.zero;
        }
        if (GravityEnabled)
        {
            // 1. Apply gravity (acceleration integration)
            if (!InGravityField && GlobalGravityEnabled)
                gravityVec = Vector3.down;
            if (!InGravityField && !GlobalGravityEnabled)
                gravityVec = Vector3.zero;
                
            //if (InGravityField)
            velocity += gravityVec * gravity * Time.fixedDeltaTime;

            // 2. Rotate character toward gravity vector
            GravityOrientation();
            ResolvePenetration();
        }

        velocity += externalVelocity;
        externalVelocity = Vector3.zero;
        // 3. Split displacement into lateral and vertical parts
        Vector3 displacement = velocity * Time.fixedDeltaTime;

        // Separate into components relative to "up" (gravity opposite)
        Vector3 up = transform.up;
        Vector3 verticalDisp = Vector3.Project(displacement, up);
        Vector3 lateralDisp = displacement - verticalDisp;

        // 4. First move laterally (player velocity along ground)
        recursionDepth = 0;
        Vector3 resolvedLateral = CollideAndSlide(transform.position, lateralDisp, false);


        // 5. Then move vertically (gravity / jump)
        recursionDepth = 0;
        Vector3 resolvedVertical = CollideAndSlide(transform.position + resolvedLateral, verticalDisp, true);

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

        transform.position += resolvedVertical + resolvedLateral + pusherVelocity;

        // 6. Compute final velocity based on actual movement
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
            pos + transform.up * (capsuleCollider.height / 2 - capsuleCollider.radius),
            pos - transform.up * (capsuleCollider.height / 2 - capsuleCollider.radius),
            capsuleCollider.radius, vel.normalized, out RaycastHit hit, dist,
            layerMask, QueryTriggerInteraction.Ignore))
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

    private void ResolvePenetration()
    {

        Collider[] overlap = Physics.OverlapCapsule(transform.position + transform.up * 0.5f, transform.position - transform.up * 0.5f, 0.5f, layerMask, QueryTriggerInteraction.Ignore);
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
                    transform.position += dir * (dis + 0.01f);
                }
            }
        }
    }

    


    public bool GroundCheck()
    {
        if (Physics.SphereCast(transform.position, capsuleCollider.radius, -transform.up, out RaycastHit hit, 0.6f, layerMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        return false;
    }
    public bool GroundCheck(out RaycastHit hit)
    {
        if (Physics.SphereCast(transform.position, capsuleCollider.radius, -transform.up, out hit, 0.6f, layerMask, QueryTriggerInteraction.Ignore))
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

    public void resetVerticalVelocity()
    {
        externalVelocity -= Vector3.Project(vel, transform.up);
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
}
