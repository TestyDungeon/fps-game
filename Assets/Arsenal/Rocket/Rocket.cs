using UnityEngine;

public class Rocket : MonoBehaviour
{
    //private Camera player_camera;
    [HideInInspector] public int damage;
    [HideInInspector] public float rocketSpeed = 1f;
    [HideInInspector] public float explosionRadius = 10;
    [HideInInspector] public float explosionForce = 5;
    [HideInInspector] public float explosionForceRB = 5;
    [HideInInspector] public GameObject explosionPrefab;
    [HideInInspector] public Transform rocketStart;
    [HideInInspector] public float range;
    private Vector3 origin; 
    private Vector3 pre_pos;

    int layerMask;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        origin = rocketStart.position;
        pre_pos = transform.position;
        layerMask = 1 << 3;
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    private void Update()
    {
        Debug.Log("OBJECT: ");
        if ((rocketStart.position - transform.position).magnitude > 1)
            GetComponent<MeshRenderer>().enabled = true;

        transform.Translate(transform.forward * rocketSpeed, Space.World);
        if(Vector3.Distance(pre_pos, origin) >= range || Physics.Raycast(pre_pos, transform.forward, Vector3.Distance(pre_pos, transform.position), layerMask, QueryTriggerInteraction.Ignore))
        {
            var surrounding_objects = Physics.OverlapSphere(transform.position, explosionRadius, ~0, QueryTriggerInteraction.Ignore);
            GameObject particles = Instantiate(explosionPrefab, transform.position-transform.forward*0.5f, Quaternion.identity);
            Destroy(particles, 2f);
            SoundManager.PlaySound(SoundType.ROCKETEXPLODE, transform.position, 100f);
            foreach (var obj in surrounding_objects)
            {
                MovementController mc = obj.GetComponent<MovementController>();
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (mc != null)
                {
                    mc.addVelocity(explosionForce * (obj.transform.position - transform.position).normalized);
                }

                if (obj.GetComponent<Health>() != null)
                {
                    Damage(obj.gameObject, damage);
                }

                if (rb != null)
                    rb.AddExplosionForce(explosionForceRB, transform.position, explosionRadius, 3, ForceMode.Impulse);

            }
            
            
            Debug.DrawRay(pre_pos, transform.forward, Color.white);
            Destroy(gameObject);
        }
    }

    private void Damage(GameObject victim, int dmg)
    {
        if (victim.GetComponent<Health>() != null)
        {
            victim.GetComponent<Health>().TakeDamage(dmg);
        }
    }

    private void LateUpdate()
    {
        pre_pos = transform.position;
    }
}
