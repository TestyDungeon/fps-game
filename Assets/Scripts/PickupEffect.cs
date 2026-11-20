using GravityGUN.Data;
using UnityEngine;

public enum PickupEffectType
{
    Health,
    Armor,
    Ammo,
    Weapon, 
    Key
}

[System.Serializable]
public struct PickupEffectData
{
    public PickupEffectType type;
    public int amount;
    public LootType ammoType;
    public string itemName;

    public bool CanCollect(PickupCollector c)
    {
        return type switch
        {
            PickupEffectType.Health => !c.Health.IsFullHealth(),
            PickupEffectType.Armor => !c.Health.IsFullArmor(),
            PickupEffectType.Ammo => c.Inventory.GetAmmo(ammoType) < c.Inventory.GetMaxAmmo(ammoType),
            PickupEffectType.Weapon => true,
            PickupEffectType.Key => true,
            _ => false
        };
    }

    public void Apply(PickupCollector c)
    {
        switch (type)
        {
            case PickupEffectType.Health:
                c.Health.Heal(amount);
                break;
            case PickupEffectType.Armor:
                c.Health.SetArmor(c.Health.GetArmor() + amount);
                break;
            case PickupEffectType.Ammo:
                c.Inventory.AddAmmo(ammoType, amount);
                break;
            case PickupEffectType.Weapon:
                c.Inventory.EnableItem(itemName);
                break;
            case PickupEffectType.Key:
                break;
        }
    }
}