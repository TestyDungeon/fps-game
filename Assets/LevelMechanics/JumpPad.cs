using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float force;
    private BoxCollider boxCollider;
    private readonly int layerMask = (1 << 3) | (1 << 8);

    private readonly HashSet<MovementController> launchedControllers = new HashSet<MovementController>();
    private readonly HashSet<Rigidbody> launchedRigidbodies = new HashSet<Rigidbody>();

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    void FixedUpdate()
    {
        if (!CheckCollisionOverlap(out List<MovementController> customColliders, out List<Rigidbody> rbs))
        {
            launchedControllers.Clear();
            launchedRigidbodies.Clear();
            return;
        }

        HashSet<MovementController> currentControllers = new HashSet<MovementController>();
        foreach (MovementController mc in customColliders)
        {
            if (mc == null)
                continue;

            currentControllers.Add(mc);
            if (!launchedControllers.Add(mc))
                continue;

            mc.resetNegativeVerticalVelocity();
            if (mc.GetIsDashing())
            {
                mc.StopDash();
                mc.addVelocity(mc.GetDashDir() * (mc.GetDashSpeed() / 4));
            }

            mc.addVelocity(force * transform.up);
            SoundManager.PlaySound(SoundType.JUMP_PAD, transform.position, 0.6f, 1f);
        }

        launchedControllers.RemoveWhere(mc => mc == null || !currentControllers.Contains(mc));

        HashSet<Rigidbody> currentRigidbodies = new HashSet<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            if (rb == null)
                continue;

            currentRigidbodies.Add(rb);
            if (!launchedRigidbodies.Add(rb))
                continue;

            rb.AddForce(force * transform.up, ForceMode.VelocityChange);
        }

        launchedRigidbodies.RemoveWhere(rb => rb == null || !currentRigidbodies.Contains(rb));
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
