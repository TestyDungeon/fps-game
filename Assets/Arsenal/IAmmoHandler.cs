using System;
using GravityGUN.Data;

public interface IAmmoHandler
{
    public event Action<int, int> OnAmmoChanged;
    public int GetAmmo();
    public LootType GetAmmoType();
    public void AmmoChanged(int maxAmount, int amount);
}
