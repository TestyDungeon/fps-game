using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [SerializeField] private float activationRange = 35;
    private MeshRenderer mr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Vector3.Distance(PlayerHitResponder.Instance.transform.position, transform.position) < activationRange)
        {
            mr.enabled = true;
        }
        else
        {
            mr.enabled = false;
        }
    }
}
