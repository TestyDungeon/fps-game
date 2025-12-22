using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Config", menuName = "AI/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Stats")]
    public string enemyName;
    public int maxHealth;
    public float chaseSpeed = 7;
    public float idleSpeed = 7;
    public float rotationSpeed = 7;
    public float maxJumpDistance = 30;
    public float gravity = 30;

    [Header("Attack")]
    public int damage = 5;
    public float attackRange = 100;
    public float attackCooldown = 2;
    public LayerMask layerMask = 1 << 3;

    [Header("Weapon (for Ranged)")]
    public TrailRenderer bulletTrailPrefab;
    public GameObject muzzleFlashPrefab;
    public float force = 20;
    public float spread = 2;
    public int bulletsPerShot = 1;
    public float timeBetweenShots = 0f;

    [Header("Behaviors")]
    public AttackType attackType;

    public IAttackBehavior GetAttackBehavior()
    {
         switch(attackType)
        {
            case AttackType.Melee:
                return new MeleeAttack();
            case AttackType.Ranged:
                return new RangedAttack();
            default:
                return new MeleeAttack();
        }      
  
  }
}

public enum AttackType { Melee, Ranged }
