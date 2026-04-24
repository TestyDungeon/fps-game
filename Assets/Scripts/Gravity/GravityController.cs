using UnityEngine;
using System.Collections.Generic;

public class GravityController : MonoBehaviour, ICustomTriggerReceiver
{
    MovementController mc = null;
    List<Collider> gravityFields = new List<Collider>();
    Collider prioritizedField = null;


    void Start()
    {
        mc = GetComponent<MovementController>();
    }

    public void OnCustomTriggerEnter(Collider other)
    {
        if (other.CompareTag("GravityField"))
        {
            if (!gravityFields.Contains(other))
                gravityFields.Add(other);

                // If we don't have a field yet, or the current one is spherical (low priority)
                // and the new one is non-spherical (high priority), swap it.
                if (prioritizedField == null || 
                    (prioritizedField.GetComponent<GravityFieldSpherical>() != null && 
                     other.GetComponent<GravityFieldSpherical>() == null))
                {
                    prioritizedField = other;
                    mc.setInGravityField(true);
                    mc.setGravityAlignSpeed(0.02f);
                    mc.setGravityVec(other.GetComponent<GravityField>().CalculateGravityVector(transform));
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
                mc.setGravityVec(other.GetComponent<GravityField>().CalculateGravityVector(transform));
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

                    Debug.Log("Switching priority to GravityField at " + prioritizedField.transform.position);
                    mc.setGravityAlignSpeed(0.02f);
                    mc.setGravityVec(prioritizedField.GetComponent<GravityField>().CalculateGravityVector(transform));
                }
                else
                {
                    prioritizedField = null;
                    mc.setInGravityField(false);
                    mc.setGravityAlignSpeed(0.02f);
                    Debug.Log("Exited all GravityFields");
                }
            }
        }
    }
}
