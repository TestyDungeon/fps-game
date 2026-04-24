using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public GunConfig gunConfig;
    private float vertical = 0;

    [HideInInspector] public Transform projectileStart;
    private Vector3 origin; 
    private Vector3 pre_pos;

    private SphereCollider sphereCollider;

    private Vector3 direction;
    int layerMask = ~(0 << 3);

    [HideInInspector] public Vector3 gravityVec = Vector3.down;

    private int ricochetCount = 0;
    private float ricoshetSpeedLoss = 1;

    private Rigidbody rb = null;
    private ParticleSystem ps = null;
    private MeshRenderer mr = null;
    private bool detonated = false;
    private bool collided = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        ps = GetComponentInChildren<ParticleSystem>();
        mr = GetComponent<MeshRenderer>();
        direction = transform.forward;
        origin = projectileStart.position;
        pre_pos = transform.position;
        ricochetCount = gunConfig.ricochet;
        Invoke("EnableMesh", 0.01f);
        rb = GetComponent<Rigidbody>();

        if(rb != null)
        {
            rb.mass = gunConfig.mass;
            RigidBodyProjectile();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if(rb == null)
            FakeProjectile();
    }

    private void FakeProjectile()
    {
        //transform.localRotation.eulerAngles.Set(vertical, 0, 0);
        direction = direction.normalized * gunConfig.projectileSpeed * ricoshetSpeedLoss + gravityVec * vertical;
        //transform.forward = transform.forward - transform.up * vertical;
        if(!collided || (gunConfig.ricochet > 0 && ricochetCount > 0))
            transform.Translate(direction * Time.deltaTime, Space.World);
        RaycastHit hit;
        bool didHit = Physics.SphereCast(pre_pos, sphereCollider.radius * transform.lossyScale.x, transform.position - pre_pos,  out hit, Vector3.Distance(pre_pos, transform.position), layerMask, QueryTriggerInteraction.Ignore);

        vertical += gunConfig.gravityMult * Time.deltaTime;
        if(Vector3.Distance(pre_pos, origin) >= gunConfig.range || didHit)
        {
            collided = true;
            if(ricochetCount > 0 && didHit)
            {
                Ricoshet(hit.normal);
            }
            Invoke("Detonate", gunConfig.explosionDelay);
            
        }
    }

    private void RigidBodyProjectile()
    {
        rb.AddForce(direction * gunConfig.projectileSpeed);
    }


    void OnCollisionEnter(Collision collision)
    {
        if(rb != null && !collided)
        {
            collided = true;
            Invoke("Detonate", gunConfig.explosionDelay);
        }
    }

    private void Ricoshet(Vector3 normal)
    {
        ricochetCount--;
        ricoshetSpeedLoss *= 0.5f;
        direction = Vector3.Reflect(direction, normal);
    }

    public void Detonate()
    {
        Explosion explosion = Instantiate(gunConfig.explosionPrefab, pre_pos, Quaternion.identity).GetComponent<Explosion>();
        explosion.gunConfig = gunConfig;
        explosion.transform.localScale = Vector3.one * gunConfig.explosionRadius;
        
        mr.enabled = false;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if(rb != null)
            rb.isKinematic = true;
        sphereCollider.enabled = false;
        Destroy(gameObject, ps.main.startLifetime.constantMax);
        Debug.Log("BOOM: " + ps.main.startLifetime.constantMax);
    }

    private void EnableMesh()
    {
        GetComponent<MeshRenderer>().enabled = true;
    }

    private void LateUpdate()
    {
        pre_pos = transform.position;
    }
}
