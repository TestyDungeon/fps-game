using System.Collections.Generic;
using GravityGUN.Data;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerStatsConfig playerStats;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float healthMod = 1;
    [SerializeField] private float speedMod = 1;
    [SerializeField] private float airControlMod = 1;
    private Dictionary<LootType, float> damageMods = new Dictionary<LootType, float>();

    private Health playerHealth;
    private PlayerMovement playerMovement;
    private Inventory inventory;

    void Awake()
    {
        // Initialize multipliers for each ammo type
        damageMods[LootType.BulletAmmo] = 1;
        damageMods[LootType.ShellAmmo]  = 1;
        damageMods[LootType.RocketAmmo] = 1;
    }

    void Start()
    {
        inventory = GetComponent<Inventory>();
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<Health>();
        Debug.Log("Health: " + Mathf.RoundToInt(playerStats.maxHealth * healthMod));
        playerHealth.SetMaxHealth(Mathf.RoundToInt(playerStats.maxHealth * healthMod));
    }

    public void SetHealthMod(float x)
    {
        if(!Upgrade())
            return;
        healthMod += x;
        playerHealth.SetMaxHealth(Mathf.RoundToInt(playerStats.maxHealth * healthMod));
    }

    public void SetSpeedMod(float x)
    {
        if(!Upgrade())
            return;
        speedMod += x;
        playerMovement.SetSpeedMultiplier(playerStats.speedMultiplier * speedMod);
    } 

    public void SetAirControlMod(float x)
    {
        if(!Upgrade())
            return;
        airControlMod += x;
        playerMovement.SetAirControlMultiplier(playerStats.airControlMultiplier * airControlMod);
    } 

    public float GetDamageMultiplier(LootType lootType)
    {
        if (damageMods != null && damageMods.TryGetValue(lootType, out float mult))
            return mult;
        
        return 1f;
    }  

    public void SetShellDamageMod(float x)
    {
        if(!Upgrade())
            return;
        damageMods[LootType.ShellAmmo] += x;
    } 

    public void SetBulletDamageMod(float x)
    {
        if(!Upgrade())
            return;
        damageMods[LootType.BulletAmmo] += x;
    } 

    private bool Upgrade()
    {
        if(inventory.GetUpgradePoint() > 0)
        {
            inventory.UseUpgradePoint(1);
            return true;
        }
        return false;
    }
}
