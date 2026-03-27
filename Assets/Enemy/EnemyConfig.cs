using UnityEngine;
using Animancer;

[CreateAssetMenu(fileName = "Enemy Config", menuName = "AI/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Animations")]
    public ClipTransition idleAnimation;
    public ClipTransition walkAnimation;
    public ClipTransition attackAnimation;

    [Header("Stats")]
    public string enemyName;
    public int maxHealth;
    public float chaseSpeed = 7;
    public float wanderSpeed = 7;
    public float rotationSpeed = 7;
    public float maxJumpDistance = 30;
    public float gravity = 30;
    public float falterDuration = 1.5f;
    public GameObject falterParticlesPrefab;

    [Header("Attack")]
    public int damage = 5;
    public float attackRange = 100;
    public float startAttackRange = 81;
    public float endAttackRange = 132;
    public float meleeAttackRange = 3;
    public float attackCooldown = 2;
    public LayerMask layerMask = 1 << 3 << 8;
    public GameObject projectile;

    [Header("Weapon (for Ranged)")]
    public TrailRenderer bulletTrailPrefab;
    public GameObject muzzleFlashPrefab;
    public GameObject deathParticle;
    public float force = 20;
    public float spread = 2;
    public int bulletsPerShot = 1;
    public float timeBetweenShots = 0f;

    [Header("Behaviors")]
    public AttackType attackType;
    public TraversalType traversalType;

    [Header("Sounds")]
    public AudioClip[] preAttackSFX;
    public AudioClip[] attackSFX;
    public AudioClip[] hurtSFX;
    public AudioClip[] deathSFX;

    [Header("Materials")]
    public Material staggerMaterial;

    [Header("Drops")]
    public GameObject healthOrb;
    public GameObject ammoOrb;

    public IAttackBehavior GetAttackBehavior()
    {
         switch(attackType)
        {
            case AttackType.Melee:
                return new MeleeAttack();
            case AttackType.Ranged:
                return new RangedAttack();
            case AttackType.Borov:
                return new BorovAttack();
            case AttackType.Piggy:
                return new PiggyAttack();
            default:
                return new MeleeAttack();
        }      
  
    }

    public ITraversalBehavior GetTraversalBehavior()
    {
         switch(traversalType)
        {
            case TraversalType.Melee:
                return new MeleeTraversal();
            case TraversalType.Ranged:
                return new RangedTraversal();
            default:
                return new MeleeTraversal();
        }      
  
    }
}

public enum AttackType { Melee, Ranged, Borov, Piggy }
public enum TraversalType { Melee, Ranged }
