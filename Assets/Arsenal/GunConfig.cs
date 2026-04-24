using UnityEngine;
using Animancer;

[CreateAssetMenu(fileName = "Gun Config", menuName = "Guns/Gun Config")]
public class GunConfig : ScriptableObject
{
    [Header("Important Setup")]
    public GameObject muzzleFlashPrefab;
    public TrailRenderer bulletTrail;
    public LayerMask layerMask = ~(1 << 3);
    
    
    public AudioClip[] shootSFX;
    public float volume = 1;
    public AudioClip[] altStartSFX;
    
    [Header("Animations")]
    public ClipTransition shootAnimation;

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
    public float projectileSpeed = 1f;
    public float projectileRicoshet = 0;
    public float gravityMult = 0f;
    public float explosionRadius = 10;
    public float explosionForceRB = 5;
    public float explosionDelay = 0;
    public float ricoshetSpeedLoss = 0;

    [Header("RigidBody")]
    public float mass = 1;

    [Header("Alt")]
    public GunConfig altConfig;
}

