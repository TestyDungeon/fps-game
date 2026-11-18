using UnityEngine;

[CreateAssetMenu(menuName = "Game/Projectile Config")]
public class ProjectileConfig : ScriptableObject
{
    [Header("Flight")]
    public float speed = 30f;
    public float radius = 0.1f;
    public float gravity = 0f;           // 0 = straight line
    public float maxRange = 100f;
    public float fuse = 0f;              // seconds, 0 = no timer

    [Header("Bounce / Homing")]
    public int bounces = 0;              // 0 = detonates on first contact
    public float bounceSpeedKeep = 0.5f;
    public float homingTurnRate = 0f;    // radians per second, 0 = no homing
    public float homingDecay = 0.5f;

    [Header("On hit")]
    public int damage = 0;               // direct-hit damage, 0 = none
    public bool parryable = false;
    public GameObject explosionPrefab;   // empty = no explosion
    public float explosionRadius = 3f;
}