using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [HideInInspector] public Transform projectileStart;
    [SerializeField] private GameObject particles;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private int damage = 20;
    [SerializeField] private float knockback;
    private Vector3 pos; 
    private Vector3 prePos;
    private SphereCollider sphereCollider;
    private float radius;

    private Vector3 direction;

    int layerMask = ~(1 << 8 | 1 << 6);


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        //player_camera = GameObject.Find("Camera").GetComponent<Camera>();
        direction = transform.forward;
        prePos = transform.position;
        //layer_mask = ~layer_mask;
        sphereCollider = GetComponent<SphereCollider>();
        radius = sphereCollider.radius * Mathf.Max(sphereCollider.transform.lossyScale.x, sphereCollider.transform.lossyScale.y, sphereCollider.transform.lossyScale.z);
        
        InvokeRepeating("SpawnParticles", 0, 0.05f);
        Destroy(gameObject, 2f);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        //SpawnParticles();
        transform.Translate(direction * projectileSpeed * Time.fixedDeltaTime, Space.World);
        if(Physics.SphereCast(prePos, radius, transform.forward, out RaycastHit hit, Vector3.Distance(prePos, transform.position), layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                Damage(hit.collider.gameObject);
                hit.collider.gameObject.GetComponent<MovementController>().addVelocity(-hit.normal * knockback);
            }
            Debug.Log("Name: " + hit.transform.name);
            Destroy(gameObject);
            //direction = Vector3.ProjectOnPlane(direction, hit.normal).normalized;
        }
    }

    private void Damage(GameObject victim)
    {
        if (victim.GetComponent<Health>() != null)
        {
            victim.GetComponent<Health>().TakeDamage(damage);
        }
    }

    private void LateUpdate()
    {
        prePos = transform.position;
    }

    private void SpawnParticles()
    {
        Instantiate(particles, transform.position, Quaternion.identity);
    }
}
