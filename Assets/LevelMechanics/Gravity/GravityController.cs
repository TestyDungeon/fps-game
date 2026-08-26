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
                 other.GetComponent<GravityFieldSpherical>() == null))
            {
                prioritizedField = other;
            }

            if (addedNewField)
            {
                mc.setInGravityField(true);
                mc.setGravityAlignSpeed(mc.GetGravityAlignSpeedOnFieldChange());
                mc.setGravityVec(CalculateIntersectingVectors(prioritizedField ?? other));
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
        if (other.CompareTag("GravityField"))
        {
            gravityFields.Remove(other);

            if (other == prioritizedField)
            {
                // Switch to another field if available
                if (gravityFields.Count > 0)
                {
                    // Find the first non-spherical field
                    prioritizedField = null;
                    foreach (Collider col in gravityFields)
                    {
                        if (col.GetComponent<GravityFieldSpherical>() == null)
                        {
                            prioritizedField = col;
                            break;
                        }
                    }

                    // If no non-spherical field exists, pick the first one from the list (the spherical one)
                    if (prioritizedField == null)
                    {
                        prioritizedField = gravityFields[0];
                    }

                    //Debug.Log("Switching priority to GravityField at " + prioritizedField.transform.position);
                    mc.setGravityAlignSpeed(mc.GetGravityAlignSpeedOnFieldChange());
                    mc.setGravityVec(prioritizedField.GetComponent<GravityField>().CalculateGravityVector(transform));
                }
                else
                {
                    prioritizedField = null;
                    mc.setInGravityField(false);
                    mc.setGravityAlignSpeed(mc.GetGravityAlignSpeedOnFieldChange());
                    //Debug.Log("Exited all GravityFields");
                }
            }
        }
    }

    private Vector3 CalculateIntersectingVectors(Collider other)
    {
        Vector3 finalVector = Vector3.zero;
        foreach(Collider col in gravityFields)
        {
            if(col.TryGetComponent<GravityField>(out GravityField gf) && gf.GetIsBlendable())
                finalVector += gf.CalculateGravityVector(transform);
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
