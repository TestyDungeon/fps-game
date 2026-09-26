using System.Collections.Generic;
using UnityEngine;

public class GravityFieldCylindrical : GravityField
{
    [SerializeField] private bool Inversed = false;
    private float inner_radius;
    private float gravity_radius;
    CapsuleCollider capsuleCollider;
    Vector3 capsuleDirection;
    Vector3 point1;
    Vector3 point2;

    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        //outer_radius = colliders[0].radius * Mathf.Max(colliders[0].transform.lossyScale.x, colliders[0].transform.lossyScale.y, colliders[0].transform.lossyScale.z);
        //inner_radius = colliders[1].radius * Mathf.Max(colliders[1].transform.lossyScale.x, colliders[1].transform.lossyScale.y, colliders[1].transform.lossyScale.z);
        //gravity_radius = outer_radius - inner_radius;
    }

    protected override bool CheckCollisionOverlap(out List<Rigidbody> rbs)
    {
        bool collided = false;
        rbs = new List<Rigidbody>();
        
        // Calculate capsule end points in world space
        Vector3 capsuleCenter = capsuleCollider.transform.TransformPoint(capsuleCollider.center);
        capsuleDirection = Vector3.zero;
        
        switch (capsuleCollider.direction)
        {
            case 0: capsuleDirection = capsuleCollider.transform.right; break;
            case 1: capsuleDirection = capsuleCollider.transform.up; break;
            case 2: capsuleDirection = capsuleCollider.transform.forward; break;
        }
        
        float halfHeight = (capsuleCollider.height / 2f - capsuleCollider.radius) * capsuleCollider.transform.lossyScale.x;
        point1 = capsuleCenter + capsuleDirection * halfHeight;
        point2 = capsuleCenter - capsuleDirection * halfHeight;
        
        Collider[] hits = Physics.OverlapCapsule(point1, point2, capsuleCollider.radius);
        
        foreach (Collider x in hits)
        {
            if (x.attachedRigidbody != null)
            {
                rbs.Add(x.attachedRigidbody);
                collided = true;
            }
            Projectile proj = x.GetComponent<Projectile>();
            if(proj != null)
            {
                proj.gravityVec = CalculateGravityVector(x.transform);
            }
        }
        return collided;
    }

    public override Vector3 CalculateGravityVector(Vector3 position)
    {
        // Find closest point on the capsule axis line
        Vector3 lineVec = point2 - point1;
        Debug.DrawLine(point1, point2, Color.cyan);
        Vector3 pointVec = position - point1;
        
        float lineLength = lineVec.magnitude;
        float distanceAlongLine = Mathf.Clamp(Vector3.Dot(pointVec, lineVec.normalized), 0f, lineLength);
        Vector3 gravPoint = point1 + lineVec.normalized * distanceAlongLine;
        Vector3 vec = gravPoint - position;
        if (!Inversed)
            vec = vec.normalized;
        else
            vec = -vec.normalized;

        return vec;
    }

    public override Vector3 CalculateGravityVector(Transform tr = null)
    {
        if (tr != null)
            return CalculateGravityVector(tr.position);
        
        return Vector3.zero;
    }

    protected override void ApplyRigidbodyGravity(Rigidbody rb)
    {
        base.ApplyRigidbodyGravity(rb);
    }

    public float get_outer_radius()
    {
        return gravity_radius;
    }
    
    public float get_inner_radius()
    {
        return inner_radius;
    }
}
