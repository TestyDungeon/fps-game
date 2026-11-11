using UnityEngine;
using System.Collections.Generic;

public class GravityController : MonoBehaviour, ICustomTriggerReceiver
{
    MovementController mc = null;
    List<Collider> gravityFields = new List<Collider>();
    Collider prioritizedField = null;


    void Awake()
    {
        mc = GetComponent<MovementController>();
        SyncGravityFromCurrentPosition();
    }


    void Start()
    {
        SyncGravityFromCurrentPosition();
    }

    public void OnCustomTriggerEnter(Collider other)
    {
        if (other.CompareTag("GravityField"))
        {
            bool addedNewField = false;
            if (!gravityFields.Contains(other))
            {
                gravityFields.Add(other);
                addedNewField = true;
            }

            // If we don't have a field yet, or the current one is spherical (low priority)
            // and the new one is non-spherical (high priority), swap it.
            if (prioritizedField == null || 
                (prioritizedField.GetComponent<GravityFieldSpherical>() != null && 
                 other.GetComponent<GravityFieldSpherical>() == null) || 
                (prioritizedField.GetComponent<GravityFieldCylindrical>() != null && 
                 other.GetComponent<GravityFieldCylindrical>() == null))
            {
                prioritizedField = other;
            }

            if (addedNewField)
            {
                Vector3 newGravityVector = CalculateIntersectingVectors(prioritizedField ?? other);  
                mc.setInGravityField(true);
                if(Vector3.Angle(mc.GetGravityVec(), newGravityVector) >  5)
                    mc.setGravityAlignSpeed(mc.GetGravityAlignSpeedOnFieldChange());
                
                mc.setGravityVec(newGravityVector);
            }

        }
    }

    public void OnCustomTriggerStay(Collider other)
    {
        if (other.CompareTag("GravityField"))
        {
            if (other == prioritizedField)
            {
                mc.setInGravityField(true);
                mc.setGravityVec(CalculateIntersectingVectors(other));
            }
        }
    }

    public void OnCustomTriggerExit(Collider other)
    {
        if (!other.CompareTag("GravityField"))
            return;
    
        if (!gravityFields.Remove(other))
            return;
    
        if (other == prioritizedField)
        {
            prioritizedField = null;
            foreach (Collider col in gravityFields)
            {
                if (col.GetComponent<GravityFieldSpherical>() == null && col.GetComponent<GravityFieldCylindrical>() == null)
                {
                    prioritizedField = col;
                    break;
                }
            }
            if (prioritizedField == null && gravityFields.Count > 0)
                prioritizedField = gravityFields[0];
        }
    
        // Always fire — leaving any field changes the blend, not just the prioritized one.
        Vector3 newGravityVector = CalculateIntersectingVectors(prioritizedField);  
        if(Vector3.Angle(mc.GetGravityVec(), newGravityVector) >  5)
            mc.setGravityAlignSpeed(mc.GetGravityAlignSpeedOnFieldChange());
    
        if (prioritizedField != null)
        {
            mc.setInGravityField(true);
            mc.setGravityVec(newGravityVector);
        }
        else
        {
            mc.setInGravityField(false);
        }
    }

    private Vector3 CalculateIntersectingVectors(Collider other)
    {
        Vector3 finalVector = Vector3.zero;
        foreach(Collider col in gravityFields)
        {
            if(col.TryGetComponent<GravityField>(out GravityField gf) && gf.GetIsBlendable())
            {
                finalVector += gf.CalculateGravityVector(transform);
                finalVector = finalVector.normalized;
            }
        }
        finalVector = finalVector.normalized;
        return finalVector != Vector3.zero ? finalVector : other.GetComponent<GravityField>().CalculateGravityVector(transform);
    }

    private void SyncGravityFromCurrentPosition()
    {
        if (mc == null)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, 0.01f, GravityField.gravityLayerMask);
        gravityFields.Clear();
        prioritizedField = null;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("GravityField"))
                continue;

            gravityFields.Add(hit);

            if (prioritizedField == null ||
                (prioritizedField.GetComponent<GravityFieldSpherical>() != null &&
                 hit.GetComponent<GravityFieldSpherical>() == null))
            {
                prioritizedField = hit;
            }
        }

        if (prioritizedField != null)
        {
            mc.setInGravityField(true);
            mc.setGravityVec(CalculateIntersectingVectors(prioritizedField));
        }
    }
}
