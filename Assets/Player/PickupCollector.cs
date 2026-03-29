using GravityGUN.Data;
using UnityEngine;

public class PickupCollector : MonoBehaviour
{
    private CapsuleCollider capsuleCollider;
    private int layerMask = 1 << 10;
    private float capsuleHalfHeight;
    private Health playerHealth;
    private Inventory inventory;
    private GameManager gameManager;
    

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        playerHealth = GetComponent<Health>();
        inventory = GetComponent<Inventory>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleHalfHeight = capsuleCollider.height / 2 - capsuleCollider.radius;
    }

    void FixedUpdate()
    {
        Collider[] hits1 = Physics.OverlapSphere(
            transform.position, 5, layerMask
        );
        if(hits1.Length > 0)
        {
            foreach(Collider hit in hits1)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if(rb != null)
                {
                    Pickup pckp = hit.GetComponent<Pickup>();
                    if(playerHealth.GetHealth() < playerHealth.GetMaxHealth() && pckp.lootType == LootType.Health)
                    {
                        rb.AddForce((transform.position - hit.transform.position) * 60, ForceMode.Acceleration);
                    }
                    if(!inventory.IsAmmoFull() 
                    && (pckp.lootType == LootType.LightAmmo || pckp.lootType == LootType.HeavyAmmo || pckp.lootType == LootType.ShellAmmo || pckp.lootType == LootType.RocketAmmo)
                    && pckp.lootType != LootType.MeleeAmmo)
                    {
                        rb.AddForce((transform.position - hit.transform.position) * 60, ForceMode.Acceleration);
                    }
                }
            }
        }
        
        

        Collider[] hits = Physics.OverlapCapsule(
            transform.position + transform.up * capsuleHalfHeight, 
            transform.position - transform.up * capsuleHalfHeight, 
            capsuleCollider.radius, layerMask, QueryTriggerInteraction.Collide);
        if(hits.Length > 0)
        {
            foreach(Collider hit in hits)
            {
                foreach (Pickup pickup in hit.GetComponents<Pickup>())
                {
                    pickup.Collect(this);
                }
            }
        }
    }

    public bool ApplyCollected(LootType lootType, int amount)
    {
        switch (lootType)
        {
            case LootType.LightAmmo:
                if(inventory.GetAmmo(lootType) != inventory.GetMaxAmmo(lootType))
                {
                    SoundManager.PlaySound(SoundType.PICKUP_MEDKIT, 0.2f);
                    inventory.AddAmmo(lootType, amount);
                    return true;
                }
                break;
            case LootType.ShellAmmo:
                if(inventory.GetAmmo(lootType) != inventory.GetMaxAmmo(lootType))
                {
                    SoundManager.PlaySound(SoundType.PICKUP_AMMO, 0.2f);
                    inventory.AddAmmo(lootType, amount);
                    return true;
                }
                break;
            case LootType.RocketAmmo:
                if(inventory.GetAmmo(lootType) != inventory.GetMaxAmmo(lootType))
                {
                    SoundManager.PlaySound(SoundType.PICKUP_AMMO, 0.2f);
                    inventory.AddAmmo(lootType, amount);
                    return true;
                }
                break;
            case LootType.HeavyAmmo:
                if(inventory.GetAmmo(lootType) != inventory.GetMaxAmmo(lootType))
                {
                    SoundManager.PlaySound(SoundType.PICKUP_AMMO, 0.2f);
                    inventory.AddAmmo(lootType, amount);
                    return true;
                }
                break;
            case LootType.MeleeAmmo:
                if(inventory.GetAmmo(lootType) != inventory.GetMaxAmmo(lootType))
                {
                    SoundManager.PlaySound(SoundType.PICKUP_AMMO, 0.2f);
                    inventory.AddAmmo(lootType, amount);
                    return true;
                }
                break;
            case LootType.Health:
                if (!playerHealth.IsFullHealth())
                {
                    SoundManager.PlaySound(SoundType.PICKUP_MEDKIT, 0.4f);
                    playerHealth.Heal(amount);
                    return true;
                }      
                break;

            case LootType.Key:
                {
                    SoundManager.PlaySound(SoundType.PICKUP_KEY, 1f);
                    return true;
                }

            case LootType.Grapple:
                SoundManager.PlaySound(SoundType.PICKUP_GRAPPLE, 1);
                gameManager.EnableGrapple();
                return true;
            
            case LootType.Guns:
                SoundManager.PlaySound(SoundType.PICKUP_GUNS, 1);
                gameManager.EnableUI();
                gameManager.EnableItems();
                return true;  
        }
        return false;  
    }

    public bool ApplyCollected(GameObject item)
    {
        SoundManager.PlaySound(SoundType.PICKUP_GUNS, 0.3f);
        gameManager.EnableUI();
        gameManager.EnableItem(item.name);
        return true;
    }

}
