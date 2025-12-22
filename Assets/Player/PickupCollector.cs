using GravityGUN.Data;
using UnityEngine;

public class PickupCollector : MonoBehaviour
{
    private CapsuleCollider capsuleCollider;
    private int layerMask = 1 << 10;
    private float capsuleHalfHeight;
    private PlayerHealth playerHealth;
    private Inventory inventory;
    private GameManager gameManager;
    

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        playerHealth = GetComponent<PlayerHealth>();
        inventory = GetComponent<Inventory>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleHalfHeight = capsuleCollider.height / 2 - capsuleCollider.radius;
    }

    void FixedUpdate()
    {
        Collider[] hits = Physics.OverlapCapsule(
            transform.position + transform.up * capsuleHalfHeight, 
            transform.position - transform.up * capsuleHalfHeight, 
            capsuleCollider.radius, layerMask, QueryTriggerInteraction.Collide);
        if(hits.Length > 0)
        {
            Debug.Log("Pickup");
            foreach(Collider hit in hits)
            {
                hit.GetComponent<Pickup>()?.Collect(this);
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
                    inventory.AddAmmo(lootType, amount);
                    return true;
                }
                break;
            case LootType.ShellAmmo:
                if(inventory.GetAmmo(lootType) != inventory.GetMaxAmmo(lootType))
                {
                    inventory.AddAmmo(lootType, amount);
                    return true;
                }
                break;
            case LootType.RocketAmmo:
                if(inventory.GetAmmo(lootType) != inventory.GetMaxAmmo(lootType))
                {
                    inventory.AddAmmo(lootType, amount);
                    return true;
                }
                break;
            case LootType.Health:
                if (!playerHealth.IsFullHealth())
                {
                    playerHealth.Heal(amount);
                    return true;
                }      
                break;
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
}
