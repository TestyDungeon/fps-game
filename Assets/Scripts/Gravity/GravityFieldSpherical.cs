using System.Collections.Generic;
using UnityEngine;

public class GravityFieldSpherical : GravityField
{
    [SerializeField] private bool Inversed = false;
    private float inner_radius;
    private float gravity_radius;
    SphereCollider sphereCollider;
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        //outer_radius = colliders[0].radius * Mathf.Max(colliders[0].transform.lossyScale.x, colliders[0].transform.lossyScale.y, colliders[0].transform.lossyScale.z);
        //inner_radius = colliders[1].radius * Mathf.Max(colliders[1].transform.lossyScale.x, colliders[1].transform.lossyScale.y, colliders[1].transform.lossyScale.z);
        //gravity_radius = outer_radius - inner_radius;
    }

    protected override bool CheckCollisionOverlap(out List<Rigidbody> rbs)
    {
        bool collided = false;
        rbs = new List<Rigidbody>();
        Collider[] hits = Physics.OverlapSphere(transform.position, sphereCollider.radius * Mathf.Max(sphereCollider.transform.lossyScale.x, sphereCollider.transform.lossyScale.y, sphereCollider.transform.lossyScale.z));
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
        Vector3 vec = new Vector3();
        if (!Inversed)
            vec = (transform.position - position).normalized;
        else
            vec = -(transform.position - position).normalized;

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
