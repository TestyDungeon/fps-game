using System.Collections.Generic;
using UnityEngine;


public class GravityField : MonoBehaviour
{
    protected float rbGravity = 20f;
    public static int gravityLayerMask = 1 << 9;

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

    public static Vector3 GetGravityAtPosition(Vector3 worldPos)
    {
        Collider[] hits = Physics.OverlapSphere(worldPos, 0.01f, gravityLayerMask);
        GravityField sphericalField = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("GravityField"))
            {
                GravityField field = hit.GetComponent<GravityField>();
                if (field != null)
                {
                    if (field is GravityFieldCuboidal)
                    {
                        return field.CalculateGravityVector(worldPos);
                    }
                    if (field is GravityFieldSpherical)
                    {
                        sphericalField = field;
                    }
                }
            }
        }

        if (sphericalField != null)
        {
            return sphericalField.CalculateGravityVector(worldPos);
        }

        return Vector3.down;
    }

    public virtual Vector3 CalculateGravityVector(Vector3 position)
    {
        return Vector3.zero;
    }

    public virtual Vector3 CalculateGravityVector(Transform tr = null)
    {
        if (tr != null)
            return CalculateGravityVector(tr.position);
            
        return Vector3.zero;
    }

    protected virtual void ApplyRigidbodyGravity(Rigidbody rb)
    {
        // Now unified priority!
        Vector3 gravityDir = GetGravityAtPosition(rb.worldCenterOfMass);
        rb.AddForce(gravityDir * rbGravity * rb.mass);
    }
}
