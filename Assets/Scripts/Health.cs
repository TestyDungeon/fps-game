using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;
    private int maxHealth;
    [HideInInspector] public int health;
    [HideInInspector] public bool isAlive = true;
    private float maxPosture = 100;
    private float posture;
    

    void Awake()
    {
        health = maxHealth;
        posture = maxPosture;
    }

    public virtual void TakeDamage(int damageAmount, Vector3 damagePoint = default, Vector3 normal = default)
    {
        //Debug.Log("DAMAGE: " + damageAmount);
        //Debug.Log(name + " health: " + health);
        if (health <= 0)
            return;
        SetHealth(health - damageAmount);

        if (health <= 0 && isAlive)
            Death();
    }

    public void Heal(int healAmount)
    {
        //Debug.Log("Heal " + healAmount);
        SetHealth(health + Mathf.Clamp(healAmount, 0, maxHealth - health));
        
    }

    public void HealthChanged()
    {
        OnHealthChanged?.Invoke(health, maxHealth);
        
    }

    virtual protected void Death()
    {
        OnDeath?.Invoke();
        Debug.Log(name + " died.");
        //Destroy(gameObject);
    }




    public bool IsFullHealth()
    {
        Debug.Log(health >= maxHealth);
        return health >= maxHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetMaxHealth(int maxHealth_)
    {
        maxHealth = maxHealth_;
        health = maxHealth;
        HealthChanged();
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int health_)
    {
        health = health_;
        HealthChanged();
    }



    public float GetMaxPosture()
    {
        return maxPosture;
    }

    public void SetMaxPosture(int maxPosture_)
    {
        maxPosture = maxPosture_;
    }

    public float GetPosture()
    {
        return posture;
    }

    public void SetPosture(float posture_)
    {
        posture = posture_;
    }

    

    
}
