using UnityEngine;
using System.Collections;

public class EnemyHitscanWeapon : MonoBehaviour
{
    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private Transform bulletStart;
    [SerializeField] private GameObject muzzleFlashPrefab;

    [SerializeField] private float timeBetweenShooting = 0.1f;
    [SerializeField] private int bulletsPerShot = 1;
    [SerializeField] private float timeBetweenShots = 0f;
    
    [SerializeField] private int damage = 5;
    [SerializeField] private float range = 300;
    [SerializeField] private float force = 20;
    [SerializeField] private float spread = 20;


    protected bool readyToShoot = true;
    protected bool shooting;
    
    
    private int layerMask = ~(1 << 8);

    void Start()
    {
        bulletStart = transform;
    }

    public void StartShoot(Vector3 target)
    {
        shooting = true;
        if (shooting && readyToShoot)
        {
            readyToShoot = false; 
            if(timeBetweenShots == 0)
            {
                
                SpawnMuzzleFlash();

                for(int i = 0; i < bulletsPerShot; i++)
                    Shoot(target);
            }
            else
            {
                StartCoroutine(TimedShooting(target));
            }
            Invoke("ResetShot", timeBetweenShooting);
        }
    }

    IEnumerator TimedShooting(Vector3 target)
    {
        for(int i = 0; i < bulletsPerShot; i++)
        {
            SpawnMuzzleFlash();
            Shoot(target);
            yield return new WaitForSeconds(timeBetweenShots);
            
        }
    }

    public void Shoot(Vector3 target)
    {
        

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 direction = (target - transform.position) + new Vector3(x, y, 0);
        
        if (Physics.Raycast(bulletStart.position, direction, out RaycastHit hit, range, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Shoot");
            target = hit.point;
            if (hit.rigidbody)
            {
                Debug.Log("HIT " + hit.transform.gameObject.name);
                hit.rigidbody.AddForceAtPosition(transform.forward * force, hit.point, ForceMode.Impulse);
            }
            if (hit.transform.gameObject.GetComponent<Health>() != null)
            {
                Debug.Log("HIT " + hit.transform.gameObject.name);
                hit.transform.gameObject.GetComponent<Health>().TakeDamage(damage);
            }
        }
        StartCoroutine(SpawnBulletTrail(target));
        //enemyAttack.SetDirection(direction);
        //enemyAttack.Attack();   
    }

    private void SpawnMuzzleFlash()
    {
        GameObject particles = Instantiate(muzzleFlashPrefab, bulletStart.position, transform.rotation);
        Destroy(particles, 0.5f);
    }

    IEnumerator SpawnBulletTrail(Vector3 target)
    {
        float time = 0;
        TrailRenderer trail = Instantiate(bulletTrail, bulletStart.position, Quaternion.identity);
        while (time < 1)
        {
            trail.transform.position += (target - trail.transform.position).normalized * 200 * Time.deltaTime;
            time += Time.deltaTime / trail.time;
            yield return null;
        }
        trail.transform.position = target;

        Destroy(trail.gameObject, trail.time);
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }
}
