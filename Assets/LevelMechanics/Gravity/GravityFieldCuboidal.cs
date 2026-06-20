using UnityEngine;
using System.Collections.Generic;

public class GravityFieldCuboidal : GravityField
{
    private BoxCollider boxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    protected override bool CheckCollisionOverlap(out List<Rigidbody> rbs)
    {
        bool collided = false;
        rbs = new List<Rigidbody>();

        Collider[] hits = Physics.OverlapBox(boxCollider.transform.TransformPoint(boxCollider.center), Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale), boxCollider.transform.rotation);
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
                proj.gravityVec = CalculateGravityVector();
            }
        }
        return collided;
    }

    protected override void ApplyRigidbodyGravity(Rigidbody rb)
    {
        base.ApplyRigidbodyGravity(rb);
    }

    public override Vector3 CalculateGravityVector(Vector3 position)
    {
        return -transform.up;
    }

    public override Vector3 CalculateGravityVector(Transform tr = null)
    {
        return CalculateGravityVector(Vector3.zero); // For cuboidal it's uniform
    }
}
