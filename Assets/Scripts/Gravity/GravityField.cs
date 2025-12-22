using System.Collections.Generic;
using UnityEngine;


public class GravityField : MonoBehaviour
{
    protected float rbGravity = 60f;


    void Start()
    {

    }

    void FixedUpdate()
    {
        if (CheckCollisionOverlap(out List<Rigidbody> rbs))
        {
            foreach (Rigidbody rb in rbs)
            {
                ApplyRigidbodyGravity(rb);
            }
        }
    }


    protected virtual bool CheckCollisionOverlap(out List<Rigidbody> rbs)
    {
        rbs = null;
        return false;
    }

    public virtual Vector3 CalculateGravityVector(Transform tr)
    {
        return Vector3.zero;
    }

    protected virtual void ApplyRigidbodyGravity(Rigidbody rb)
    {

    }
    //protected void SetInGravityField()
}
