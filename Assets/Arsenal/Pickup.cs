using UnityEngine;
using GravityGUN.Data;

public class Pickup : MonoBehaviour
{
    [Header("Loot")]
    public LootType lootType;
    public int amount;
    public void Collect(PickupCollector collector)
    {
        if(collector.ApplyCollected(lootType, amount))
            Destroy(gameObject);
    }

}
