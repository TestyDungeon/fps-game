using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(Transform source, int damageAmount, Vector3 damagePoint = default, Vector3 normal = default);
}
