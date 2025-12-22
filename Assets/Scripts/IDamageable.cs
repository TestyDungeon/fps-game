using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(int damageAmount, Vector3 damagePoint, Vector3 normal);
}
