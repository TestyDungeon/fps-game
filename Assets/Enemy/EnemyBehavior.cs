using Unity.VisualScripting;
using UnityEngine;

public interface IAttackBehavior
{
    public void ExecuteAttack(EnemyStateManager enemy, Vector3 target);
    public bool IsInAttackRange(EnemyStateManager enemy, Vector3 target);
    public bool CanAttack(EnemyStateManager enemy);
}

public class MeleeAttack : IAttackBehavior
{
    float lastAttackTime;
    public void ExecuteAttack(EnemyStateManager enemy, Vector3 target)
    {
        if(!CanAttack(enemy))
            return;

        if(enemy.animator != null)
        {
            enemy.animator.speed = 2;
            enemy.animator.CrossFade("Attack", 0.2f, 0, 0);
        }
        if(Physics.Raycast(enemy.transform.position, enemy.transform.forward, out RaycastHit hit, enemy.enemyConfig.startAttackRange, 1 << 3, QueryTriggerInteraction.Ignore))
        {
            
            hit.transform.GetComponent<PlayerHealth>().TakeDamage(enemy.enemyConfig.damage);
            lastAttackTime = Time.time;
        }
    }


    public bool IsInAttackRange(EnemyStateManager enemy, Vector3 target)
    {
        return Vector3.Distance(enemy.transform.position, target) <= enemy.enemyConfig.attackRange;
    }

    public bool CanAttack(EnemyStateManager enemy)
    {
        return (Time.time - lastAttackTime) >= enemy.enemyConfig.attackCooldown;
    }
}

public class RangedAttack : IAttackBehavior
{
    float lastAttackTime;

    public void ExecuteAttack(EnemyStateManager enemy, Vector3 target)
    {
        //Debug.DrawLine(enemy.transform.position, target, Color.white);
        if(!CanAttack(enemy))
            return;

        if(enemy.enemyConfig.timeBetweenShots == 0)
        {
            
            SpawnMuzzleFlash(enemy);
            for(int i = 0; i < enemy.enemyConfig.bulletsPerShot; i++)
                enemy.StartCoroutine(DelayedAttack(enemy, target, 0.3f));
        }
        else
        {
            
            if(enemy.animator != null)
            {
                enemy.animator.speed = 1;
                enemy.animator.CrossFade("Attack", 0.2f, 0, 0);
            }
            enemy.StartCoroutine(TimedShooting(enemy, target));
        }
        //enemy.enemyHitscanWeapon.StartShoot(target);
        lastAttackTime = Time.time;
    }

    //public void AnimateAttack(EnemyStateManager enemy)
    //{
    //    //Debug.DrawLine(enemy.transform.position, target, Color.white);
    //    if(!CanAttack(enemy))
    //        return;
//
    //    if(enemy.enemyConfig.timeBetweenShots == 0)
    //    {
    //        
    //    }
    //    else
    //    {
    //        
    //        if(enemy.animator != null)
    //        {
    //            enemy.animator.speed = 1;
    //            enemy.animator.CrossFade("Attack", 0.2f, 0, 0);
    //        }
    //    }
    //}

    public bool IsInAttackRange(EnemyStateManager enemy, Vector3 target)
    {
        return Vector3.Distance(enemy.transform.position, target) <= enemy.enemyConfig.attackRange;
    }

    public bool CanAttack(EnemyStateManager enemy)
    {
        return (Time.time - lastAttackTime) >= enemy.enemyConfig.attackCooldown;
    }

    public void Shoot(EnemyStateManager enemy, Vector3 target)
    {
        SoundManager.PlaySound(SoundType.FLYING_ENEMY_ATTACK, 0.05f);

        float x = Random.Range(-enemy.enemyConfig.spread, enemy.enemyConfig.spread);
        float y = Random.Range(-enemy.enemyConfig.spread, enemy.enemyConfig.spread);

        Vector3 direction = target - enemy.bulletStart.position + new Vector3(x, y, 0);
        
        Object.Instantiate(enemy.projectile, enemy.bulletStart.position, Quaternion.LookRotation(direction));

        //if (Physics.Raycast(enemy.transform.position, direction, out RaycastHit hit, enemy.enemyConfig.attackRange, enemy.enemyConfig.layerMask, QueryTriggerInteraction.Ignore))
        //{
        //    target = hit.point;
        //    if (hit.rigidbody)
        //    {
        //        hit.rigidbody.AddForceAtPosition(enemy.transform.forward * enemy.enemyConfig.force, hit.point, ForceMode.Impulse);
        //    }
        //    if (hit.transform.gameObject.GetComponent<Health>() != null)
        //    {
        //        hit.transform.gameObject.GetComponent<Health>().TakeDamage(enemy.enemyConfig.damage);
        //        Debug.Log("Damage????");
        //    }
        //}
        //enemy.StartCoroutine(SpawnBulletTrail(enemy, target));
    }

    System.Collections.IEnumerator TimedShooting(EnemyStateManager enemy, Vector3 target)
    {
        for(int i = 0; i < enemy.enemyConfig.bulletsPerShot; i++)
        {
            //SpawnMuzzleFlash(enemy);
            enemy.StartCoroutine(DelayedAttack(enemy, target, 0.3f));
            yield return new WaitForSeconds(enemy.enemyConfig.timeBetweenShots);
            
        }
    }

    private void SpawnMuzzleFlash(EnemyStateManager enemy)
    {
        GameObject particles = Object.Instantiate(enemy.enemyConfig.muzzleFlashPrefab, enemy.bulletStart.position, enemy.bulletStart.rotation);
        Object.Destroy(particles, 0.5f);
    }

    System.Collections.IEnumerator SpawnBulletTrail(EnemyStateManager enemy, Vector3 target)
    {
        float time = 0;
        TrailRenderer trail = Object.Instantiate(enemy.enemyConfig.bulletTrailPrefab, enemy.bulletStart.position, Quaternion.identity);
        while (time < 1)
        {
            trail.transform.position += (target - trail.transform.position).normalized * 200 * Time.deltaTime;
            time += Time.deltaTime / trail.time;
            yield return null;
        }
        trail.transform.position = target;

        Object.Destroy(trail.gameObject, trail.time);
    }

    System.Collections.IEnumerator DelayedAttack(EnemyStateManager enemy, Vector3 target, float delay)
    {
        yield return new WaitForSeconds(delay);
        enemy.UpdateLastPlayerPosition();
        target = enemy.GetPlayerPosition();
        Shoot(enemy, target);
    }
}
