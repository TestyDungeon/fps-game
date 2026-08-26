using System;
using System.Collections;
using GravityGUN.Data;
using UnityEngine;

public class Gun : Item, IAmmoHandler
{
    public event Action<int, int> OnAmmoChanged;
    public Transform bulletStart;
    public Transform bulletCaseStart;
    private Light[] lights;
    
    public GunConfig gunConfig;
    
    private float damageMultiplier = 1;

    protected bool readyToShoot;
    protected bool readyToShootAlt;
    protected bool shooting;
    protected bool bufferShooting;


    protected bool alt = false;
    private bool altStart;
    private bool altEnd;
    protected bool altEndButtonCheck;
    protected float bufferTime = 0.15f;
    protected float lastTime;
    Vector3 direction;

    protected Animator animator = null;
    //protected AnimancerComponent animancer = null;

    private CameraRecoil cameraRecoil;

    protected GunConfig originalConfig;

    private GameObject newProjectile;
    private Projectile projectileComponent;

    private PlayerStats playerStats;

    [SerializeField] protected bool IsReadyToShoot => alt ? readyToShootAlt : readyToShoot;

    

    protected override void Awake()
    {
        base.Awake();
        originalConfig = gunConfig;
        animator = GetComponentInChildren<Animator>();
        lights = GetComponentsInChildren<Light>();
        
        readyToShoot = true;
        readyToShootAlt = true;
    }

    void OnEnable()
    {
        if(animator != null)
            animator.Play("Shoot", 0, 1);
        
        SetLights(false);
        //
    }

    void OnDisable()
    {
        OnAltEnd();
    }

    protected override void Start()
    {
        base.Start();
        playerStats = player.GetComponent<PlayerStats>();
        cameraRecoil = player.GetComponentInChildren<CameraRecoil>();
        lights = GetComponentsInChildren<Light>();
        Debug.Log(lights.Length);
        if(lights.Length > 0)
            SetLights(false);
        direction = cameraPivot.forward;
    }

    void Update()
    {
        if(GameManager.Instance.GetInMenu() || GameManager.Instance.GetIsGameOver())
            return;
            
        GunInput();
        if(altStart)
            OnAltStart();
        else if(altEnd)
            OnAltEnd();    
        if (shooting && IsReadyToShoot && (gunConfig.ammoPerShot == 0 || GetAmmo() >= gunConfig.ammoPerShot))
        {
            if(!alt)
                readyToShoot = false;
            else
                readyToShootAlt = false;               
            if(gunConfig.timeBetweenShots == 0)
            {
                PreShoot();
                for(int i = 0; i < gunConfig.bulletsPerShot; i++)
                {
                    Shoot(SpreadDirection());
                }
                Knockback(direction);
                if(lights.Length > 0)
                    StartCoroutine(Light());
            }
            else
            {
                StartCoroutine(TimedShooting());
            }
            
            if(!alt)
                Invoke("ResetShot", gunConfig.timeBetweenShooting);
            else
                Invoke("ResetAltShot", gunConfig.timeBetweenShooting);
            
        }
        
    }

    protected IEnumerator DelayedShoot()
    {
        if (animator != null)
            animator.Play("Shoot", 0, 0);

        yield return new WaitForSeconds(gunConfig.inputDelay);

        shooting = true;
    }

    

    IEnumerator TimedShooting()
    {
        for(int i = 0; i < gunConfig.bulletsPerShot; i++)
        {
            if(GetAmmo() >= gunConfig.ammoPerShot)
            {
                PreShoot();
                Shoot(SpreadDirection());
                Knockback(-direction);
                if(lights.Length > 0)
                    StartCoroutine(Light());
                yield return new WaitForSeconds(gunConfig.timeBetweenShots);
            }
            
        }
    }

    protected virtual void GunInput()
    {
        

        altStart = false;
        altEnd = false;

        if (Input.GetButtonDown("Fire2"))
        {
            altStart = Input.GetButtonDown("Fire2");
            shooting = false;
            altEndButtonCheck = true;
        }
        else if (Input.GetButton("Fire2") && !alt)
        {
            altStart = true;
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            altEnd = Input.GetButtonUp("Fire2");
            shooting = false;
            altEndButtonCheck = true;
        }

        
        if(!altEndButtonCheck)
        {
            if (gunConfig.allowButtonHold)
                shooting = Input.GetButton("Fire1");
            else
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    lastTime = Time.time;
                    bufferShooting = Input.GetButtonDown("Fire1");
                }
                if((Time.time - lastTime) >= bufferTime)
                {
                    bufferShooting = false;
                }

                if(bufferShooting && gunConfig.inputDelay > 0)
                    StartCoroutine(DelayedShoot());
                else
                    shooting = bufferShooting;
            }
        }
        if(!Input.GetButton("Fire1"))
            altEndButtonCheck = false;


        if (!canUse)
        {
            shooting = false;
            return;
        }
    }

    protected void InputDelay()
    {
        shooting = true;
    }
    
    private void ResetShot()
    {
        readyToShoot = true;
        if(gunConfig.inputDelay > 0)
            shooting = false;
    }

    protected void ResetAltShot()
    {
        readyToShootAlt = true;
    }

    private void PreShoot()
    {
        //if(gunConfig.shootSFX.audio != null)
        SoundManager.PlaySound(gunConfig.shootSFX, gunConfig.volume);
        //if(animancer != null && gunConfig.inputDelay == 0)
        //{
        //    animancer.Play(gunConfig.shootAnimation).Time = 0;
        //}
        if(animator != null && gunConfig.inputDelay == 0)
        {
            animator.Play("Shoot", 0, 0);
        }
        cameraRecoil.ApplyRecoil(gunConfig.recoilAmountCamera, gunConfig.recoilSpeedCamera, gunConfig.returnSpeedCamera);
        SpawnMuzzleFlash();
        StartCoroutine(SpawnBulletCase());
        if(gunConfig.ammoPerShot != 0) 
            inventory.ConsumeAmmo(gunConfig.ammoType, gunConfig.ammoPerShot);
    }


    private Vector3 SpreadDirection()
    {
        float x = UnityEngine.Random.Range(-gunConfig.spread, gunConfig.spread);
        float y = UnityEngine.Random.Range(-gunConfig.spread, gunConfig.spread);

        direction = cameraPivot.forward + cameraPivot.up * y + cameraPivot.right * x;
        return direction;
    }

    virtual protected void Shoot(Vector3 dir)
    {
        damageMultiplier = playerStats.GetDamageMultiplier(gunConfig.ammoType);

        if(gunConfig.projectilePrefab == null)
        {
            HitscanShoot(cameraPivot.position, dir, gunConfig.ricochet);
        }
        else
        {
            ProjectileShoot(dir);
        }
    }

    private void HitscanShoot(Vector3 start, Vector3 dir, int ricochetCount = 0)
    {
        bool hitSurface = false;
        float distance = gunConfig.range;
        Vector3 target = transform.position + dir * distance;

        RaycastHit hit;

        if (Physics.Raycast(start, dir, out hit, gunConfig.range, (1 << 0 | 1 << 15), QueryTriggerInteraction.Collide))
        {
            Debug.Log("HIT: " + hit.transform.name);
            target = hit.point;
            distance = hit.distance;
            if (hit.rigidbody)
            {
                hit.rigidbody.AddForceAtPosition(transform.forward * gunConfig.force, hit.point, ForceMode.Impulse);
            }

            if (hit.transform.gameObject.TryGetComponent<IDamageable>(out IDamageable victim))
            {
                victim.TakeDamage(player.transform, Mathf.RoundToInt(gunConfig.damage * damageMultiplier), hit.point, hit.normal);
            }
            else if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Default") || hit.transform.gameObject.layer == LayerMask.NameToLayer("Interactable"))
            {
                Destroy(Instantiate(GameManager.Instance.decalParticles, hit.point, Quaternion.LookRotation(hit.normal)), 20f);
                hitSurface = true;

            }
        }
        if(gunConfig.bulletTrail != null)
            StartCoroutine(SpawnBulletTrail(start, target));
        
        if(ricochetCount > 0 && hitSurface)
        {
            ricochetCount--;
            HitscanShoot(hit.point, Vector3.Reflect(dir, hit.normal), ricochetCount);
        }
    }

    private void ProjectileShoot(Vector3 dir)
    {
        newProjectile = Instantiate(gunConfig.projectilePrefab, cameraPivot.position, Quaternion.LookRotation(dir));
        projectileComponent = newProjectile.GetComponent<Projectile>();
        projectileComponent.gunConfig = gunConfig;
        projectileComponent.projectileStart = cameraPivot;
        newProjectile.GetComponent<MeshRenderer>().enabled = false;
    }

    protected virtual void AltShoot()
    {

    }

    private void OnAltStart()
    {
        altEndButtonCheck = true;
        SoundManager.PlaySound(gunConfig.altStartSFX, gunConfig.volume);

        var pm = player.GetComponent<PlayerMovement>();
        pm.SetFOV(0.85f);
        gunConfig = gunConfig.altConfig;
        alt = true;
    }

    private void OnAltEnd()
    {
        altEndButtonCheck = true;
        alt = false;
        shooting = false;
        pm.SetFOV(1);
        gunConfig = originalConfig;
    }

    private IEnumerator Light()
    {
        SetLights(true);
        yield return new WaitForSeconds(0.08f);
        SetLights(false);
    }

    private void SetLights(bool state)
    {
        if (lights == null) return;
        foreach(Light light in lights)
        {
            if (light != null)
                light.enabled = state;
        }
    }
    

    private void Knockback(Vector3 dir)
    {
        Debug.DrawRay(player.transform.position, dir*10, Color.cyan, 2f);
        player.GetComponent<MovementController>().addVelocity(dir * gunConfig.knockbackForce);
    }

    

    public IEnumerator SpawnBulletTrail(Vector3 start, Vector3 target)
    {
        float time = 0;
        TrailRenderer trail = Instantiate(gunConfig.bulletTrail, start, Quaternion.identity);
        while (time < 1)
        {
            trail.transform.position += (target - trail.transform.position).normalized * 200 * Time.deltaTime;
            time += Time.deltaTime / trail.time;
            yield return null;
        }
        trail.transform.position = target;

        Destroy(trail.gameObject, trail.time);
    }


    protected void SpawnMuzzleFlash()
    {
        GameObject particles = Instantiate(gunConfig.muzzleFlashPrefab, bulletStart.position, transform.rotation, transform);
        Destroy(particles, 0.5f);
    }

    protected IEnumerator SpawnBulletCase()
    {
        yield return new WaitForSeconds(gunConfig.bulletCaseDelay);
        if(gunConfig.bulletCasePrefab != null)
        {
            GameObject particles = Instantiate(gunConfig.bulletCasePrefab, bulletCaseStart.position, transform.rotation, transform);
            Destroy(particles, 2f);
            yield return new WaitForSeconds(0.3f);
            SoundManager.PlaySound(gunConfig.caseFallSFX, gunConfig.caseFallVolume);
        }
    }

    public int GetAmmo()
    {
        if(inventory == null)
            Debug.Log("INV IS NULL");
        return inventory != null ? inventory.GetAmmo(gunConfig.ammoType) : 0;
    }

    public LootType GetAmmoType()
    {
        return gunConfig.ammoType;
    }

    

    public void AmmoChanged(int maxAmount, int amount)
    {
        OnAmmoChanged?.Invoke(maxAmount, amount);
    }
}
