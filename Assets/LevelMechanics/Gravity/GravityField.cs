using System.Collections.Generic;
using UnityEngine;


public class GravityField : MonoBehaviour
{
    protected float rbGravity = 9.81f;
    [SerializeField] private bool blendable = false;
    [HideInInspector] public static int gravityAffectedLM = 1 << 6 | 1 << 17;
    [HideInInspector] public static LayerMask gravityFieldLM = 1 << 9;
    private static readonly HashSet<Rigidbody> appliedThisStep = new HashSet<Rigidbody>();
    private static float lastStepTime = -1f;

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

    public bool GetIsBlendable()
    {
        return blendable;
    }


    protected virtual bool CheckCollisionOverlap(out List<Rigidbody> rbs)
    {
        rbs = null;
        return false;
    }

    public static Vector3 GetGravityAtPosition(Vector3 worldPos)
    {
        Collider[] hits = Physics.OverlapSphere(worldPos, 0.01f, gravityFieldLM);
        GravityField sphericalField = null;
        Vector3 finalVector = Vector3.zero;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("GravityField"))
            {
                if (hit.TryGetComponent<GravityField>(out GravityField field))
                {
                    if (field is GravityFieldCuboidal)
                    {
                        if(field.GetIsBlendable() == true)
                        {
                            finalVector += field.CalculateGravityVector();
                            finalVector = finalVector.normalized;
                        }
                        else
                        {
                            return field.CalculateGravityVector(worldPos);
                        }
                    }
                    if (field is GravityFieldSpherical)
                    {
                        sphericalField = field;
                    }
                    if (field is GravityFieldCylindrical)
                    {
                        sphericalField = field;
                    }
                }
            }
        }

        if (sphericalField != null && finalVector == Vector3.zero)
        {
            return sphericalField.CalculateGravityVector(worldPos);
        }

        finalVector = finalVector.normalized;
        

        return finalVector;
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
        if (Time.fixedTime != lastStepTime)
        {
            appliedThisStep.Clear();
            lastStepTime = Time.fixedTime;
        }

        if (!appliedThisStep.Add(rb))
            return;

        Vector3 gravityDir = GetGravityAtPosition(rb.worldCenterOfMass);
        rb.AddForce(gravityDir * rbGravity * rb.mass, ForceMode.Acceleration);
    }
}
