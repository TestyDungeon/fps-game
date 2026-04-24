using System;
using System.Linq;
using System.Collections.Generic;
using GravityGUN.Data;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public event Action<Item> OnSlotChanged;
    [Header("Important setup")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform mountPoint;

    [Header("Slots")]
    [SerializeField] private Item[] slots;
    [SerializeField] private Item[] alwaysOnSlots;
    private int currentSlot = -1;

    [Header("Ammo")]
    [SerializeField] private int maxLightAmmo;
    [SerializeField] private int maxHeavyAmmo;
    [SerializeField] private int maxShellAmmo;
    [SerializeField] private int maxRocketAmmo;
    [SerializeField] private int maxMeleeAmmo;

    [SerializeField] private int upgradePoints;

    private int totalMaxAmmo;

    private Dictionary<LootType, int> maxAmmoCount = new();
    private Dictionary<LootType, int> ammoCount = new();

    


    void Awake()
    {
        maxAmmoCount[LootType.BulletAmmo] = maxLightAmmo;
        maxAmmoCount[LootType.ShellAmmo] = maxShellAmmo;
        maxAmmoCount[LootType.RocketAmmo] = maxRocketAmmo;
        maxAmmoCount[LootType.MeleeAmmo] = maxMeleeAmmo;

        
        totalMaxAmmo = maxAmmoCount.Values.Sum() - maxAmmoCount[LootType.MeleeAmmo];

        ammoCount[LootType.BulletAmmo] = 0;
        ammoCount[LootType.ShellAmmo] = 0;
        ammoCount[LootType.RocketAmmo] = 0;
        ammoCount[LootType.MeleeAmmo] = 0;
        upgradePoints = 0;

        

        if(slots.Length > 0)
            EquipItem(0);
        
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].player = gameObject;
            slots[i].cameraPivot = cameraPivot;
        }
        
        for (int i = 0; i < alwaysOnSlots.Length; i++)
        {
            alwaysOnSlots[i].player = gameObject;
            alwaysOnSlots[i].cameraPivot = cameraPivot;
        }

    }

    void Start()
    {
        AddAmmo(LootType.BulletAmmo, maxAmmoCount[LootType.BulletAmmo] / 1);
        AddAmmo(LootType.ShellAmmo, maxAmmoCount[LootType.ShellAmmo] / 1);
        AddAmmo(LootType.RocketAmmo, maxAmmoCount[LootType.RocketAmmo] / 1);
        AddAmmo(LootType.MeleeAmmo, maxAmmoCount[LootType.MeleeAmmo]);
    }

    void Update()
    {

        for (int i = 0; i < slots.Length; i++)
        {
            // Number keys start at Alpha1 = "1"
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipItem(i);
            }
        }
    }

    private void EquipItem(int slot)
    {
        if (slot == currentSlot || slot < 0 || slot > slots.Length || !slots[slot].transform.parent.gameObject.activeSelf)
            return;

        Vector3 pos = Vector3.zero;
        if (currentSlot >= 0)
        {
            pos = slots[currentSlot].transform.localPosition;
            slots[currentSlot].OnUnequip();
        }

        slots[slot].OnEquip();
        currentSlot = slot;
        slots[currentSlot].transform.localPosition = pos;
        
        
        if (slots[currentSlot] is Gun gun)
        {
            gun.SetInventory(this);
        }
        
        OnSlotChanged?.Invoke(slots[currentSlot]);
    }

    public void EnableItem(string name)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].name == name)
            {
                slots[i].transform.parent.gameObject.SetActive(true);
                EquipItem(i);
                return;
            }
        }
    }

    public Item GetCurrent()
    {
        if(slots.Length <= 0)
            return null;
        if(slots[currentSlot] != null)
            return slots[currentSlot];
        else
            return null;
    }

    public Item[] GetAlwaysOn()
    {
        if(alwaysOnSlots.Length <= 0)
            return null;
        return alwaysOnSlots;
    }

    

    public void AddAmmo(LootType lootType, int amount)
    {
        if (ammoCount.ContainsKey(lootType))
        {
            ammoCount[lootType] += Mathf.Clamp(amount, 0, maxAmmoCount[lootType] - ammoCount[lootType]);
            NotifyAmmoChanged(lootType);
        }
    }

    public void ConsumeAmmo(LootType lootType, int amount)
    {
        if (ammoCount.ContainsKey(lootType))
        {
            ammoCount[lootType] = Mathf.Max(0, ammoCount[lootType] - amount);
            NotifyAmmoChanged(lootType);
        }
    }

    public int GetAmmo(LootType lootType)
    {
        return ammoCount.ContainsKey(lootType) ? ammoCount[lootType] : 0;
    }

    public int GetTotalAmmo()
    {
        return ammoCount.Values.Sum() - ammoCount[LootType.MeleeAmmo];
    }

    public bool IsAmmoFull()
    {
        return GetTotalAmmo() >= totalMaxAmmo;
    }

    public int GetMaxAmmo(LootType lootType)
    {
        return maxAmmoCount.ContainsKey(lootType) ? maxAmmoCount[lootType] : 0;
    }

    private void NotifyAmmoChanged(LootType lootType)
    {
        foreach (var slot in slots)
        {
            if (slot is IAmmoHandler gun && gun.GetAmmoType() == lootType)
            {
                //Debug.Log("Ammo: "+ammoCount[lootType]);
                gun.AmmoChanged(maxAmmoCount[lootType], ammoCount[lootType]);
            }
        }
        foreach (var slot in alwaysOnSlots)
        {
            if (slot is IAmmoHandler gun && gun.GetAmmoType() == lootType)
            {
                //Debug.Log("Ammo: "+ammoCount[lootType]);
                gun.AmmoChanged(maxAmmoCount[lootType], ammoCount[lootType]);
            }
        }
    }



    public void AddUpgradePoint(int amount)
    {
        upgradePoints += amount;
    }

    public void UseUpgradePoint(int amount)
    {
        upgradePoints = Mathf.Max(0, upgradePoints - amount);
    }

    public int GetUpgradePoint()
    {
        return upgradePoints;
    }
}
