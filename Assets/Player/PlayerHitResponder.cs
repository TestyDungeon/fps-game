using System;
using UnityEngine;

public class PlayerHitResponder : MonoBehaviour, IDamageable
{
    [SerializeField] private Transform playerPivot;
    private Health health;
    private GameManager gameManager;
    private Shield shield;

    public static PlayerHitResponder Instance { get; private set; }
    
    void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


    void Start()
    {
        health = GetComponent<Health>();
        gameManager = FindAnyObjectByType<GameManager>();
        shield = FindAnyObjectByType<Shield>();
    }

    public void TakeDamage(Transform source, int damageAmount, Vector3 damagePoint = default, Vector3 normal = default)
    {
        //Debug.Log("DAMAGE: " + damageAmount);
        //Debug.Log(name + " health: " + currentHealth);
        if (health.GetHealth() <= 0)
            return;
        
        if(shield.GetBlocking())
        {
            bool isInFront = Vector3.Dot(playerPivot.forward, (damagePoint - transform.position).normalized) > 0;
            if(isInFront)
            {
                if (shield.GetParrying())
                {
                    
                }
                else
                    SoundManager.PlaySound(SoundType.SHIELD_BLOCK, 1);
                return;
            }
        }
        SoundManager.PlaySound(SoundType.HURT, 0.3f);
        health.SetHealth(health.GetHealth() - damageAmount);
        health.HealthChanged();

        if (health.GetHealth() <= 0 && health.isAlive)
            Death();
    }

    

    protected void Death()
    {
        health.isAlive = false;
        Debug.Log("Player " + name + " died.");
        gameManager.GameOver();
    }

    public Health GetPlayerHealth()
    {
        return health;
    }
}
