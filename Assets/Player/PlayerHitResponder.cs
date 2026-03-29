using System;
using UnityEngine;

public class PlayerHitResponder : MonoBehaviour, IDamageable
{
    [SerializeField] private Transform playerPivot;
    private Health health;
    private GameManager gameManager;
    private Kick melee;

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
        melee = FindAnyObjectByType<Kick>();
    }

    public void TakeDamage(Transform source, int damageAmount, Vector3 damagePoint = default, Vector3 normal = default)
    {
        if (health.GetHealth() <= 0)
            return;
        
        if(melee.GetBlocking())
        {
            bool isInFront = Vector3.Dot(playerPivot.forward, (damagePoint - transform.position).normalized) > 0;
            if(isInFront)
            {
                if (!melee.GetParrying())
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
