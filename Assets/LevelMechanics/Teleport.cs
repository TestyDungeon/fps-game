using UnityEngine;
using System.Collections.Generic;

public class Teleport : MonoBehaviour
{
    [SerializeField] private Teleport teleport;
    [HideInInspector] public List<MovementController> inTeleport = new List<MovementController>();
    private List<MovementController> removeFromInTeleport = new List<MovementController>();
    private BoxCollider boxCollider;
    private int layerMask = (1 << 3) | (1 << 8);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.Log(name + " " + inTeleport);
        bool check = CheckCollisionOverlap(out List<MovementController> customColliders, out List<Rigidbody> rbs);
        foreach(MovementController mc in inTeleport)
        {
            foreach(MovementController col in customColliders)
            {
                if(col.GetComponent<MovementController>() == mc)
                {
                    
                }
                else
                {
                    removeFromInTeleport.Add(mc);
                    
                }
            }
            
        }
        foreach(MovementController mc in removeFromInTeleport)
        {
            inTeleport.Remove(mc);
            
        }

        if (check)
        {
            foreach (MovementController mc in customColliders)
            {
                if (mc != null && inTeleport.Contains(mc) == false)
                {
                    SoundManager.PlaySound(SoundType.JUMP_PAD, transform.position, 0.6f, 0f);
                    DoTeleport(mc);
                }
            }
            foreach (Rigidbody rb in rbs)
            {
                
            }
        }
    }

    private bool CheckCollisionOverlap(out List<MovementController> mcs, out List<Rigidbody> rbs)
    {
        bool collided = false;
        mcs = new List<MovementController>();
        rbs = new List<Rigidbody>();

        Collider[] hits = Physics.OverlapBox(boxCollider.transform.TransformPoint(boxCollider.center), Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale), boxCollider.transform.rotation, layerMask);
        foreach (Collider x in hits)
        {
            if (x.TryGetComponent(out MovementController mc))
            {
                mcs.Add(mc);
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

    private void DoTeleport(MovementController mc)
    {
        Debug.Log("MC: " + mc.name);
        teleport.inTeleport.Add(mc);
        mc.transform.position = teleport.transform.position;
    }
}
