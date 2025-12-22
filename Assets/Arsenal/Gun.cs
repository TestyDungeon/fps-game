using System;
using System.Collections;
using GravityGUN.Data;
using UnityEngine;

public class Gun : Item, IAmmoHandler
{
    public event Action<int> OnAmmoChanged;
    [Header("Important Setup")]
    [SerializeField] protected Transform bulletStart;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] protected TrailRenderer bulletTrail;

    [Header("Gun")]
    [SerializeField] private bool allowButtonHold;
    [SerializeField] protected int damage = 20;
    [SerializeField] private LootType ammoType;
    protected Inventory inventory;

    [SerializeField] private float timeBetweenShooting = 0.1f;
    [SerializeField] private int bulletsPerShot = 1;
    [SerializeField] private float timeBetweenShots = 0f;
    protected bool readyToShoot;
    protected bool shooting;
    protected Animator animator = null;
    protected WeaponRecoil weaponRecoil;
    protected CameraRecoil cameraRecoil;

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
        weaponRecoil = GetComponent<WeaponRecoil>();
    }

    void Update()
    {
        GunInput();
        if (shooting && readyToShoot && GetAmmo() > 0)
        {
            readyToShoot = false;
            
            if(timeBetweenShots == 0)
            {
                if(itemName == "smg")
                    SoundManager.PlaySound(SoundType.SMGSHOT, 0.3f);
                if(itemName == "shotgun")
                    SoundManager.PlaySound(SoundType.SHOTGUNSHOT, 0.15f);
                
                cameraRecoil.ApplyRecoil();
                weaponRecoil.ApplyRecoil();
                SpawnMuzzleFlash();
                inventory.ConsumeAmmo(ammoType, 1);

                for(int i = 0; i < bulletsPerShot; i++)
                    Shoot();
            }
            else
            {
                StartCoroutine(TimedShooting());
            }
            
            Invoke("ResetShot", timeBetweenShooting);
            
        }
    }

    IEnumerator TimedShooting()
    {
        for(int i = 0; i < bulletsPerShot; i++)
        {
            if(GetAmmo() > 0)
            {
                if(itemName == "smg")
                    SoundManager.PlaySound(SoundType.SMGSHOT, 0.3f);
                if(itemName == "shotgun")
                    SoundManager.PlaySound(SoundType.SHOTGUNSHOT, 0.15f);

                cameraRecoil.ApplyRecoil();
                weaponRecoil.ApplyRecoil();
                SpawnMuzzleFlash();
                inventory.ConsumeAmmo(ammoType, 1);

                Shoot();
                yield return new WaitForSeconds(timeBetweenShots);
            }
            
        }
    }

    private void GunInput()
    {
        if (allowButtonHold)
            shooting = Input.GetButton("Fire1");
        else
            shooting = Input.GetButtonDown("Fire1");

    }
    
    private void ResetShot()
    {
        readyToShoot = true;
    }

    protected virtual void Shoot()
    {

    }

    protected void SpawnMuzzleFlash()
    {
        GameObject particles = Instantiate(muzzleFlashPrefab, bulletStart.position, transform.rotation);
        Destroy(particles, 0.5f);
    }

    public int GetAmmo()
    {
        if(inventory == null)
            Debug.Log("INV IS NULL");
        return inventory != null ? inventory.GetAmmo(ammoType) : 0;
    }

    public LootType GetAmmoType()
    {
        return ammoType;
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
