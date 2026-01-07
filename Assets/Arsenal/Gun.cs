using System;
using System.Collections;
using GravityGUN.Data;
using UnityEngine;

public class Gun : Item, IAmmoHandler
{
    public event Action<int> OnAmmoChanged;
    public Transform bulletStart;
    
    public GunConfig gunConfig;

    protected bool readyToShoot;
    protected bool shooting;
    Vector3 direction;

    protected Animator animator = null;
    protected Inventory inventory;

    protected CameraRecoil cameraRecoil;
    private Vector3 targetRecoil;
    private Vector3 currentRecoil;

    void Awake()
    {
        if (GetComponentInChildren<Animator>())
        {
            animator = GetComponentInChildren<Animator>();
        }
        readyToShoot = true;
    }

    void Start()
    {
        cameraRecoil = player.GetComponentInChildren<CameraRecoil>();
        direction = cameraPivot.forward;
    }

    void Update()
    {
        GunInput();
        if (shooting && readyToShoot && GetAmmo() > 0)
        {
            readyToShoot = false;
            
            if(gunConfig.timeBetweenShots == 0)
            {
                PreShoot();
                for(int i = 0; i < gunConfig.bulletsPerShot; i++)
                {
                    Shoot(SpreadDirection());
                }
                Knockback(direction);
            }
            else
            {
                StartCoroutine(TimedShooting());
            }
            
            Invoke("ResetShot", gunConfig.timeBetweenShooting);
            
        }
        
        CalculateRecoil();
    }

    

    IEnumerator TimedShooting()
    {
        for(int i = 0; i < gunConfig.bulletsPerShot; i++)
        {
            if(GetAmmo() > 0)
            {
                PreShoot();
                Shoot(SpreadDirection());
                Knockback(-direction);
                yield return new WaitForSeconds(gunConfig.timeBetweenShots);
            }
            
        }
    }

    private void GunInput()
    {
        if (gunConfig.allowButtonHold)
            shooting = Input.GetButton("Fire1");
        else
            shooting = Input.GetButtonDown("Fire1");

    }
    
    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void PreShoot()
    {
        if(itemName == "smg")
            SoundManager.PlaySound(SoundType.SMGSHOT, 0.3f);
        if(itemName == "shotgun")
            SoundManager.PlaySound(SoundType.SHOTGUNSHOT, 0.15f);
        
        cameraRecoil.ApplyRecoil();
        ApplyRecoil();
        SpawnMuzzleFlash();
        inventory.ConsumeAmmo(gunConfig.ammoType, 1);

        

        
    }

    private Vector3 SpreadDirection()
    {
        float x = UnityEngine.Random.Range(-gunConfig.spread, gunConfig.spread);
        float y = UnityEngine.Random.Range(-gunConfig.spread, gunConfig.spread);

        direction = cameraPivot.forward + new Vector3(x, y, 0);
        return direction;
    }

    protected virtual void Shoot(Vector3 dir)
    {

    }




    //Recoil

    void CalculateRecoil()
    {
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, gunConfig.returnSpeed * Time.deltaTime);
        currentRecoil = Vector3.Slerp(currentRecoil, targetRecoil, gunConfig.recoilSpeed * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRecoil);
    }

    

    public void ApplyRecoil()
    {
        cameraRecoil.recoilAmount = gunConfig.recoilAmountCamera;
        cameraRecoil.recoilSpeed = gunConfig.recoilSpeedCamera;
        cameraRecoil.returnSpeed = gunConfig.returnSpeedCamera;
        Debug.Log("Recoil");
        currentRecoil = Vector3.zero;
        transform.localRotation = Quaternion.Euler(currentRecoil);
        targetRecoil = new Vector3(-gunConfig.recoilAmount, 0, 0);
    }

    ///////////
    

    private void Knockback(Vector3 dir)
    {
        Debug.DrawRay(player.transform.position, dir*10, Color.cyan, 2f);
        player.GetComponent<MovementController>().addVelocity(dir * gunConfig.knockbackForce);
    }

    

    public IEnumerator SpawnBulletTrail(Vector3 target)
    {
        float time = 0;
        TrailRenderer trail = Instantiate(gunConfig.bulletTrail, bulletStart.position, Quaternion.identity);
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
        GameObject particles = Instantiate(gunConfig.muzzleFlashPrefab, bulletStart.position, transform.rotation);
        Destroy(particles, 0.5f);
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

    public void SetInventory(Inventory inv)
    {
        inventory = inv;
    }

    public void AmmoChanged(int amount)
    {
        OnAmmoChanged?.Invoke(amount);
    }
}
