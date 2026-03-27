using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GunConfig gunConfig;
    private float vertical = 0;

    [HideInInspector] public Transform projectileStart;
    private Vector3 origin; 
    private Vector3 pre_pos;

    private Vector3 direction;
    int layerMask;

    private float ricoshetSpeedLoss = 1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        direction = transform.forward;
        origin = projectileStart.position;
        pre_pos = transform.position;
        layerMask = 1 << 3;
        layerMask = ~layerMask;
    }

    // Update is called once per frame
    private void Update()
    {
        Invoke("EnableMesh", 0.01f);
        //transform.localRotation.eulerAngles.Set(vertical, 0, 0);
        direction = direction.normalized * gunConfig.projectileSpeed * ricoshetSpeedLoss + -transform.up * vertical;
        transform.Translate(direction, Space.World);
        //transform.forward = transform.forward - transform.up * vertical;
        RaycastHit hit;
        bool didHit = Physics.Raycast(pre_pos, transform.position - pre_pos, out hit, Vector3.Distance(pre_pos, transform.position), layerMask, QueryTriggerInteraction.Ignore);

        if(Vector3.Distance(pre_pos, origin) >= gunConfig.range || didHit)
        {
            //if(gunConfig.projectileRicoshet > 0 && didHit)
            //{
            //    Ricoshet(hit.normal);
            //}
            ricoshetSpeedLoss = 0;
            Invoke("Detonate", gunConfig.explosionDelay);
        }
        vertical += gunConfig.gravityMult * Time.deltaTime;
    }

    private void Ricoshet(Vector3 normal)
    {
        ricoshetSpeedLoss *= 0.5f;
        transform.forward = Vector3.Reflect(transform.forward, normal);
    }

    public void Detonate()
    {
        Explosion explosion = Instantiate(gunConfig.explosionPrefab, pre_pos, Quaternion.identity).GetComponent<Explosion>();
        explosion.gunConfig = gunConfig;
        explosion.transform.localScale = Vector3.one * gunConfig.explosionRadius;
        
        Destroy(gameObject);
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
