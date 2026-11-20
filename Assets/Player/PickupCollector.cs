using UnityEngine;

public class PickupCollector : MonoBehaviour, ICustomTriggerReceiver
{
    public Health Health { get; private set; }
    public Inventory Inventory { get; private set; }

    const int PickupLayerMask = 1 << 10;

    void Start()
    {
        Health = GetComponent<Health>();
        Inventory = GetComponent<Inventory>();
    }

    public void OnCustomTriggerEnter(Collider other)
    {
        foreach (var pickup in other.GetComponents<Pickup>())
            pickup.Collect(this);
    }
    public void OnCustomTriggerStay(Collider other) { }
    public void OnCustomTriggerExit(Collider other) { }

    void FixedUpdate()
    {
        foreach (var hit in Physics.OverlapSphere(transform.position, 5, PickupLayerMask))
        {
            if (!hit.TryGetComponent(out Rigidbody rb)) continue;
            if (!hit.TryGetComponent(out Pickup pickup)) continue;   // your old code threw here
            if (pickup.IsAttractedTo(this))
                rb.AddForce((transform.position - hit.transform.position) * 60, ForceMode.Acceleration);
        }
    }
}