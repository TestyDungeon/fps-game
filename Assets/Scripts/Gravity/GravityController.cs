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

            // Prioritize the first field entered
            if (prioritizedField == null || prioritizedField.GetComponent<GravityFieldSpherical>() != null)
            {
                prioritizedField = other;
                mc.setInGravityField(true);
                mc.setGravityAlignSpeed(0.02f);
                mc.setGravityVec(other.GetComponent<GravityField>().CalculateGravityVector(transform));
                //Debug.Log("Prioritizing GravityField at " + other.transform.position);
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
                    prioritizedField = gravityFields[0];
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
