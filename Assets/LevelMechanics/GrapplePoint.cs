using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [SerializeField] private float activationRange = 35;
    private MeshRenderer mr;
    private Collider col;
    [SerializeField] private bool isEnabled = true;
    private bool doOnce = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
        mr.enabled = false;
        col.enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isEnabled)
        {
            if(Vector3.Distance(PlayerHitResponder.Instance.transform.position, transform.position) < activationRange)
            {
                if(doOnce == false)
                {
                    SoundManager.PlaySound(SoundType.GRAPPLE_POINT, transform.position, 0.95f, 0.75f);
                    doOnce = true;
                    mr.enabled = true;
                    col.enabled = true;
                }
                
            }
            else
            {
                doOnce = false;
                mr.enabled = false;
                col.enabled = false;
            }
        }
        
    }

    public void Enable()
    {
        isEnabled = true;
    }
}
