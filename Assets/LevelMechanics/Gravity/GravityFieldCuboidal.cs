using UnityEngine;
using System.Collections.Generic;

public class GravityFieldCuboidal : GravityField
{
    private BoxCollider boxCollider;
    private readonly Collider[] overlapBuffer = new Collider[16];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    protected override bool CheckCollisionOverlap(out List<Rigidbody> rbs)
    {
        bool collided = false;
        rbs = new List<Rigidbody>();

        int hitCount = Physics.OverlapBoxNonAlloc(
            boxCollider.transform.TransformPoint(boxCollider.center),
            Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale),
            overlapBuffer,
            boxCollider.transform.rotation,
            gravityAffectedLM);
        for (int i = 0; i < hitCount; i++)
        {
            Collider x = overlapBuffer[i];
            if (x.attachedRigidbody != null)
            {
                rbs.Add(x.attachedRigidbody);
                collided = true;
            }
            if(x.TryGetComponent<Projectile>(out Projectile proj))
            {
                proj.SetGravityDir(CalculateGravityVector());
            }
        }
        return collided;
    }

    //protected override void ApplyRigidbodyGravity(Rigidbody rb)
    //{
    //    base.ApplyRigidbodyGravity(rb);
    //}

    public override Vector3 CalculateGravityVector(Vector3 position)
    {
        return -transform.up;
    }

    public override Vector3 CalculateGravityVector(Transform tr = null)
    {
        return CalculateGravityVector(Vector3.zero); // For cuboidal it's uniform
    }
}
