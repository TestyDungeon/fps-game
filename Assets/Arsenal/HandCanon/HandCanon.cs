using UnityEngine;

public class HandCanon : Gun
{
    [Header("Important Setup")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private GameObject explosionPrefab;

    [Header("Gun")]
    [SerializeField] private float rocketSpeed = 1f;
    [SerializeField] private float explosionRadius = 10;
    [SerializeField] private float explosionForceRB = 5;

    private GameObject newRocket;
    private Projectile projectileComp;

    protected override void GunInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            lastTime = Time.time;
            bufferShooting = Input.GetKeyDown(KeyCode.R);
        }
        if((Time.time - lastTime) >= bufferTime)
        {
            bufferShooting = false;
        }
            
        if(bufferShooting)
            StartCoroutine(DelayedShoot());
        //shooting = bufferShooting;
    }


    protected override void Shoot(Vector3 dir)
    {
        newRocket = Instantiate(rocketPrefab, cameraPivot.position, Quaternion.LookRotation(dir));
        //projectileComp =  newRocket.GetComponent<Projectile>();
        //projectileComp.projectileStart = cameraPivot;
        //projectileComp.damage = gunConfig.damage;
        //projectileComp.range = gunConfig.range;
        //projectileComp.explosionPrefab = explosionPrefab;
        //projectileComp.projectileSpeed = rocketSpeed;
        //projectileComp.explosionRadius = explosionRadius;
        //projectileComp.explosionForce = gunConfig.force;
        //projectileComp.explosionForceRB = explosionForceRB;
        newRocket.GetComponent<MeshRenderer>().enabled = false;
    }

    void OnAltStart()
    {
        if(newRocket != null)
            projectileComp.Detonate();
    }


}
