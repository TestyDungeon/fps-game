using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float force;
    private BoxCollider boxCollider;
    private int layerMask = (1 << 3) | (1 << 8);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        
        //Debug.Log(boxCollider.name);
    }

    void FixedUpdate()
    {
        if (CheckCollisionOverlap(out List<MovementController> customColliders, out List<Rigidbody> rbs))
        {
            foreach (MovementController mc in customColliders)
            {
                if (mc != null)
                {
                    mc.resetNegativeVerticalVelocity();
                    if (mc.GetIsDashing())
                    {
                        mc.StopDash();
                        mc.addVelocity(mc.GetDashDir() * (mc.GetDashSpeed() / 4));
                    }
                    mc.addVelocity(force * transform.up);
                    SoundManager.PlaySound(SoundType.JUMP_PAD, transform.position, 0.6f, 1f);
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
}
