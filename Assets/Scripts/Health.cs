using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnArmorChanged;
    public event Action OnDeath;
    [SerializeField] private int maxHealth;
    [HideInInspector] public int health;
    [HideInInspector] public bool isAlive = true;
    private float maxArmor;
    private float armor;
    

    void Awake()
    {
        SetHealth(maxHealth);
        
        SetMaxArmor(maxHealth);
        SetArmor(maxArmor);
    }

    public virtual void TakeDamage(int damageAmount, Vector3 damagePoint = default, Vector3 normal = default)
    {
        //Debug.Log("DAMAGE: " + damageAmount);
        //Debug.Log(name + " health: " + health);
        //if (health <= 0)
        //    return;
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

    public void ArmorChanged()
    {
        OnArmorChanged?.Invoke(armor, maxArmor);
        
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
        health = Mathf.Max(health_, 0);
        HealthChanged();
    }



    public float GetMaxArmor()
    {
        return maxArmor;
    }

    public void SetMaxArmor(int maxArmor_)
    {
        maxArmor = maxArmor_;
    }

    public float GetArmor()
    {
        return armor;
    }

    public void SetArmor(float armor_)
    {
        armor = Mathf.Max(armor_, 0);
        ArmorChanged();
    }

    

    
}
