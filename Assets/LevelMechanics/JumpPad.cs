using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float force;
    private BoxCollider boxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponentsInChildren<BoxCollider>()[1];
        
        //Debug.Log(boxCollider.name);
    }

    void FixedUpdate()
    {
        if (CheckCollisionOverlap(out List<Collider> customColliders, out List<Rigidbody> rbs))
        {
            foreach (Collider c in customColliders)
            {
                if (c != null)
                {
                    c.gameObject.GetComponent<MovementController>().resetNegativeVerticalVelocity();
                    c.gameObject.GetComponent<MovementController>().addVelocity(force * transform.up);
                }
            }
            foreach (Rigidbody rb in rbs)
            {
                
            }
        }
    }

    private bool CheckCollisionOverlap(out List<Collider> customColliders, out List<Rigidbody> rbs)
    {
        bool collided = false;
        customColliders = new List<Collider>();
        rbs = new List<Rigidbody>();

        Collider[] hits = Physics.OverlapBox(boxCollider.transform.TransformPoint(boxCollider.center), Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale), boxCollider.transform.rotation);
        foreach (Collider x in hits)
        {
            if (x.GetComponent<MovementController>() != null)
            {
                customColliders.Add(x);
                collided = true;
            }
            else if (x.attachedRigidbody != null)
            {
                rbs.Add(x.attachedRigidbody);
                collided = true;
            }
        }
        return collided;
    }
}
