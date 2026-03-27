using Unity.Mathematics;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [HideInInspector] public Transform projectileStart;

    //[SerializeField] private GameObject particles;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private int damage = 20;
    [SerializeField] private float targetMagnetism = 1;
    [SerializeField] private float knockback;
    private Vector3 pos; 
    private Vector3 prePos;
    private BoxCollider boxCollider;
    private Vector3 size;
    private Transform target;
    private Transform parent;
    private bool deflected = false;
    
    private bool parryable = false;
    private ParticleSystem[] particles;
    private Vector3 direction;

    int layerMask;
    int playerLayerMask = ~(1 << 6 | 1 << 2);
    int deflectedLayerMask = 1 << 8;


    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
        layerMask = playerLayerMask;
        //player_camera = GameObject.Find("Camera").GetComponent<Camera>();
        direction = transform.forward;
        prePos = transform.position;
        //layer_mask = ~layer_mask;
        Destroy(gameObject, 5);
        boxCollider = GetComponent<BoxCollider>();
        size = boxCollider.size * Mathf.Max(boxCollider.transform.lossyScale.x, boxCollider.transform.lossyScale.y, boxCollider.transform.lossyScale.z);
        Collider[] overlap = Physics.OverlapBox(prePos, size/2, Quaternion.identity, layerMask, QueryTriggerInteraction.Ignore);
        if(overlap.Length > 0)
        {
            foreach(Collider col in overlap)
            {
                if(col.transform == parent && !deflected)
                    return;

                if (col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("Enemy"))
                {
                    Damage(col);
                    col.gameObject.GetComponent<MovementController>().addVelocity(transform.forward * knockback);
                }
                Debug.Log("(Overlap) Name: " + col.transform.name);
                Destroy(gameObject);
            }
            
        }
        
        //InvokeRepeating("SpawnParticles", 0, 0.05f);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if(!deflected)
            targetMagnetism = Mathf.Lerp(targetMagnetism, 0, 1f * Time.fixedDeltaTime);
        if(target != null)
            direction += (target.position - transform.position).normalized * targetMagnetism * Time.fixedDeltaTime;
        direction = direction.normalized;
        //SpawnParticles();
        transform.Translate(direction * projectileSpeed * Time.fixedDeltaTime, Space.World);
        Debug.DrawLine(prePos, transform.position, Color.cyan, 5);
        if(Physics.BoxCast(prePos, size/2, direction, out RaycastHit hit, Quaternion.identity, Vector3.Distance(prePos, transform.position), layerMask, QueryTriggerInteraction.Ignore))
        {
            
            if(hit.collider.transform == parent && !deflected)
                return;

            if (hit.collider.gameObject.CompareTag("Player") || hit.collider.gameObject.CompareTag("Enemy"))
            {
                Damage(hit);
                hit.collider.gameObject.GetComponent<MovementController>().addVelocity(-hit.normal * knockback);
            }
            Debug.Log("Name: " + hit.transform.name);
            Destroy(gameObject);
            //direction = Vector3.ProjectOnPlane(direction, hit.normal).normalized;
        }
    }

    private void Damage(RaycastHit hit)
    {
        if (hit.collider.GetComponent<IDamageable>() != null)
        {
            hit.collider.GetComponent<IDamageable>().TakeDamage(parent, damage, hit.point, hit.normal);
        }
    }

    private void Damage(Collider col)
    {
        if (col.GetComponent<IDamageable>() != null)
        {
            col.GetComponent<IDamageable>().TakeDamage(parent, damage);
        }
    }

    private void LateUpdate()
    {
        prePos = transform.position;
    }

    public void SetParent(Transform par)
    {
        parent = par;

    }

    public void Deflected(Vector3 deflector)
    {
        if(deflected || !parryable)
            return;
        target = parent;
        parent = PlayerHitResponder.Instance.transform;
        SoundManager.PlaySound(SoundType.DEFLECT, 0.3f);
        layerMask = deflectedLayerMask;
        deflected = true;
        direction = (target.position - deflector).normalized;
        targetMagnetism = 10;
        projectileSpeed = 30;
        damage *= 5;
    }

    public void Deflected(Vector3 deflector, Vector3 dir)
    {
        if(deflected)
            return;
        target = parent;
        parent = PlayerHitResponder.Instance.transform;
        SoundManager.PlaySound(SoundType.DEFLECT, 0.3f);
        layerMask = deflectedLayerMask;
        deflected = true;
        direction = dir;
        targetMagnetism = 0;
        projectileSpeed = 40;
    }

    public Transform GetParent()
    {
        return parent;
    }

    public void SetTarget(Transform tar)
    {
        target = tar;

    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir;;
        
    }

    public void ChangeParryable(bool state)
    {
        parryable = state;
        if (parryable)
        {
            targetMagnetism = 10;
            foreach(ParticleSystem par in particles)
            {
                var main = par.main;
                main.startColor = Color.magenta;
            }
        }
    }

}
