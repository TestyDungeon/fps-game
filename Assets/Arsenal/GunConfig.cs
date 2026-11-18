using UnityEngine;

[CreateAssetMenu(fileName = "Gun Config", menuName = "Guns/Gun Config")]
public class GunConfig : ScriptableObject
{
    [Header("Important Setup")]
    public GameObject muzzleFlashPrefab;
    public GameObject bulletCasePrefab = null;
    public float bulletCaseDelay = 0.3f;
    public TrailRenderer bulletTrail;
    
    
    [Header("SFX")]
    public AudioClip equipSFX;
    public float equipVolume = 1;
    public AudioClip[] shootSFX;
    public float volume = 1;
    public AudioClip[] altStartSFX;
    public AudioClip[] caseFallSFX;
    public float caseFallVolume = 1;

    [Header("Camera Recoil")]
    public float recoilAmountCamera;
    public float recoilSpeedCamera;
    public float returnSpeedCamera;

    [Header("Gun")]
    public bool allowButtonHold;
    public int damage;
    public GravityGUN.Data.LootType ammoType;
    public float range = 300;
    public float force = 20;
    public float knockbackForce = 0.1f;
    public float spread = 20;
    public float timeBetweenShooting;
    public int bulletsPerShot;
    public int ammoPerShot = 1;
    public float timeBetweenShots;
    public float inputDelay = 0f;
    public int ricochet = 0;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public GameObject explosionPrefab;
    

    [Header("Alt")]
    public GunConfig altConfig;
}

