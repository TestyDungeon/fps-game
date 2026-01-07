using UnityEngine;

[CreateAssetMenu(fileName = "Gun Config", menuName = "Guns/Gun Config")]
public class GunConfig : ScriptableObject
{
    [Header("Important Setup")]
    public GameObject muzzleFlashPrefab;
    public TrailRenderer bulletTrail;

    [Header("Weapon Recoil")]
    public float recoilAmount;
    public float recoilSpeed;
    public float returnSpeed;
    
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
    public float timeBetweenShots;
}

