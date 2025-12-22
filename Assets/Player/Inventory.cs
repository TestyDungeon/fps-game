using System;
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
    [SerializeField] private int maxShellAmmo;
    [SerializeField] private int maxRocketAmmo;

    private Dictionary<LootType, int> maxAmmoCount = new();
    private Dictionary<LootType, int> ammoCount = new();

    


    void Awake()
    {
        maxAmmoCount[LootType.LightAmmo] = maxLightAmmo;
        maxAmmoCount[LootType.ShellAmmo] = maxShellAmmo;
        maxAmmoCount[LootType.RocketAmmo] = maxRocketAmmo;

        ammoCount[LootType.LightAmmo] = 0;
        ammoCount[LootType.ShellAmmo] = 0;
        ammoCount[LootType.RocketAmmo] = 0;

        

        if(slots.Length > 0)
            EquipItem(0);
        //OnSlotChanged?.Invoke(slots[currentSlot]);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].player = gameObject;
            slots[i].cameraPivot = cameraPivot;
        }
        Debug.Log("ALWAYS: "+alwaysOnSlots.Length);
        for (int i = 0; i < alwaysOnSlots.Length; i++)
        {
            alwaysOnSlots[i].player = gameObject;
            alwaysOnSlots[i].cameraPivot = cameraPivot;
        }

    }

    void Start()
    {
        AddAmmo(LootType.LightAmmo, maxAmmoCount[LootType.LightAmmo]);
        AddAmmo(LootType.ShellAmmo, maxAmmoCount[LootType.ShellAmmo]);
        AddAmmo(LootType.RocketAmmo, maxAmmoCount[LootType.RocketAmmo]);
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
        if (slot == currentSlot || slot < 0 || slot > slots.Length)
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
        
        // Set inventory reference for guns
        if (slots[currentSlot] is Gun gun)
        {
            gun.SetInventory(this);
        }
        
        OnSlotChanged?.Invoke(slots[currentSlot]);
    }

    public Item GetCurrent()
    {
        if(slots.Length <= 0)
            return null;
        return slots[currentSlot];
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
                gun.AmmoChanged(ammoCount[lootType]);
            }
        }
    }
}
