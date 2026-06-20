using UnityEngine;

public class ForceField : MonoBehaviour
{
    [SerializeField] private float force = 10;
    private BoxCollider boxCollider;
    int layerMask = (1 << 3) | (1 << 8);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Collider[] hits = Physics.OverlapBox(boxCollider.transform.TransformPoint(boxCollider.center), Vector3.Scale(boxCollider.size * 0.5f, boxCollider.transform.lossyScale), boxCollider.transform.rotation, layerMask);
        foreach (Collider x in hits)
        {
            if (x.TryGetComponent(out MovementController mc))
            {
                mc.addVelocity(transform.GetChild(0).transform.right * force * Time.fixedDeltaTime);
            }
        }
    }
}
