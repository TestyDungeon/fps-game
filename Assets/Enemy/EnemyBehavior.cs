using System.Collections;
using UnityEngine;


////////////////////////////////////////////////////////////////////
// ATTACK BEHAVIOR
////////////////////////////////////////////////////////////////////

public interface IAttackBehavior
{
    public void ExecuteAttack(EnemyStateManager enemy);
    public bool IsInAttackRange(EnemyStateManager enemy);
}


public class MeleeAttack : IAttackBehavior
{
    float lastAttackTime = 0;
    
    public void ExecuteAttack(EnemyStateManager enemy)
    {
        
        //if(!canAttack)
        //    yield break;
        Debug.Log("Attack attempt");

        enemy.lookDir = enemy.GetVectorToTarget();
        enemy.GoToTarget(15);

        enemy.canAttack = false;
        enemy.animator.Play("Attack", 0, 0f);

        var events = enemy.animEvents.GetEvents("Attack");

        events.Clear();

        events.Add(0.25f, () =>
        {
            enemy.MoveToTarget(8);
        });

        events.Add(0.35f, () =>
        {
            Hit(enemy);

            enemy.lookDir = enemy.GetVectorToTarget();
            enemy.MoveToTarget(8);
        });

        events.Add(0.5f, () =>
        {
            enemy.MoveToTarget(8);
        });

        events.Add(0.65f, () =>
        {
            Hit(enemy);
            enemy.MoveToTarget(8);
        });

        events.Add(0.8f, () =>
        {
            enemy.StartCoroutine(enemy.ResetAttack());
        });

        
    }

    private void Hit(EnemyStateManager enemy)
    {
        SoundManager.PlaySound(enemy.enemyConfig.attackSFX, enemy.transform.position, 0.3f);
        Debug.DrawRay(enemy.meleeAttackCollider.transform.position,
            enemy.meleeAttackCollider.bounds.size * enemy.transform.lossyScale.x / 2, Color.cyan, 2);
        //Object.Instantiate(enemy.enemyConfig.falterParticlesPrefab, enemy.meleeAttackCollider.transform.position, Quaternion.identity);
        Collider[] cols = Physics.OverlapBox(
            enemy.meleeAttackCollider.transform.position,
            enemy.meleeAttackCollider.size * enemy.transform.lossyScale.x / 2,
            enemy.meleeAttackCollider.transform.rotation,
            1 << 15 | 1 << 16,
            QueryTriggerInteraction.Collide
        );
        
        foreach(Collider col in cols)
        {
            if(col.transform.parent == enemy.transform)
                continue;

            if(col.transform != enemy.transform)
                col.transform.GetComponent<IDamageable>().TakeDamage(enemy.transform, enemy.enemyConfig.damage, col.transform.position, enemy.transform.position - col.transform.position);
            //if(col.transform.TryGetComponent<MovementController>(out MovementController mc))
            //{
            //    mc.addVelocity((col.transform.position - enemy.transform.position).normalized * 10f);
            //}
        }
    }


    public bool IsInAttackRange(EnemyStateManager enemy)
    {
        return Vector3.Distance(enemy.transform.position, enemy.targetTransform.position) <= enemy.enemyConfig.attackRange;
    }

    public bool CanAttack(EnemyStateManager enemy)
    {
        //return (Time.time - lastAttackTime) >= enemy.enemyConfig.attackCooldown;
        return enemy.canAttack;
    }

    
}

public class RangedAttack : IAttackBehavior
{
    float lastAttackTime = -Time.time;
    private int bulletStartInd;
    private Vector3 target;
    private int attackCount = 0;

    public void ExecuteAttack(EnemyStateManager enemy)
    {
        enemy.canAttack = false;
         // Set immediately to prevent duplicate calls from FixedUpdate
        SoundManager.PlaySound(enemy.enemyConfig.preAttackSFX, enemy.transform.position, 1, 0.96f);
        attackCount++;
        target = enemy.targetTransform.position;
        enemy.animator.Play("Attack", 0, 0f);

        var events = enemy.animEvents.GetEvents("Attack");

        events.Clear();

        events.Add(0.5f, () =>
        {
            //SpawnMuzzleFlash(enemy);
            SoundManager.PlaySound(enemy.enemyConfig.attackSFX, enemy.transform.position, 1, 0.96f);
            bulletStartInd = 0;
            enemy.StartCoroutine(TimedShooting(enemy));
        });

        // Reset when animation ends
        events.Add(1f, () => 
        {
            
            enemy.StartCoroutine(enemy.ResetAttack(enemy.WanderState));
            enemy.animator.Play("Attack");

        });


    }


    public bool IsInAttackRange(EnemyStateManager enemy)
    {
        return Vector3.Distance(enemy.transform.position, enemy.targetTransform.position) <= enemy.enemyConfig.attackRange;
    }


    public void Shoot(EnemyStateManager enemy)
    {
        float x = Random.Range(-enemy.enemyConfig.spread, enemy.enemyConfig.spread);
        float y = Random.Range(-enemy.enemyConfig.spread, enemy.enemyConfig.spread);

        Vector3 direction = target - enemy.bulletStart[bulletStartInd].position + new Vector3(x, y, 0);
        
        EnemyProjectile projectile = Object.Instantiate(enemy.enemyConfig.projectile, enemy.bulletStart[bulletStartInd].position, Quaternion.LookRotation(direction)).GetComponent<EnemyProjectile>();
        if(attackCount >= 3)
        {
            projectile.ChangeParryable(true);
            attackCount = 0;
        }
        projectile.SetTarget(enemy.targetTransform);
        projectile.SetParent(enemy.transform);
    }

    IEnumerator TimedShooting(EnemyStateManager enemy)
    {
        for(int i = 0; i < enemy.enemyConfig.bulletsPerShot; i++)
        {
            //SpawnMuzzleFlash(enemy);
            Shoot(enemy);
            yield return new WaitForSeconds(enemy.enemyConfig.timeBetweenShots);
            
        }
    }

    private void SpawnMuzzleFlash(EnemyStateManager enemy)
    {
        GameObject particles = Object.Instantiate(enemy.enemyConfig.muzzleFlashPrefab, enemy.bulletStart[0].position, enemy.bulletStart[0].rotation);
        Object.Destroy(particles, 0.5f);
    }

    
}

public class BorovAttack : IAttackBehavior
{
    float lastAttackTime = -Time.time;
    private int bulletStartInd;
    private Vector3 target;
    private int attackCount = 0;

    public void ExecuteAttack(EnemyStateManager enemy)
    {
        if(attackCount >= 6)
        {
            attackCount = 0;
            enemy.SwitchState(enemy.WanderState);
            return;
        }

        

        enemy.lookDir = enemy.GetVectorToTarget();
        target = enemy.targetTransform.position;
        enemy.canAttack = false;
        enemy.animator.Play("Attack", 0, 0f);

        var events = enemy.animEvents.GetEvents("Attack");

        events.Clear();

        events.Add(0.25f, () =>
        {
            attackCount++;
            bulletStartInd = 0;
            Shoot(enemy);
        });

        

        // Second projectile at 60% from bulletStart[1]
        events.Add(0.7f, () =>
        {
            attackCount++;
            bulletStartInd = 1;
            Shoot(enemy);
        });

        // Reset when animation ends
        events.Add(1f, () => 
        {
            enemy.StartCoroutine(enemy.ResetAttack());
            enemy.animator.Play("Attack");
        });
    }

    public bool IsInAttackRange(EnemyStateManager enemy)
    {
        return Vector3.Distance(enemy.transform.position, enemy.targetTransform.position) <= enemy.enemyConfig.attackRange;
    }


    public void Shoot(EnemyStateManager enemy)
    {
        enemy.lookDir = enemy.GetVectorToTarget();
        SoundManager.PlaySound(enemy.enemyConfig.attackSFX, enemy.transform.position, 1, 0.96f);
        target = enemy.targetTransform.position;


        float x = Random.Range(-enemy.enemyConfig.spread, enemy.enemyConfig.spread);
        float y = Random.Range(-enemy.enemyConfig.spread, enemy.enemyConfig.spread);

        Vector3 direction = target - enemy.bulletStart[bulletStartInd].position + new Vector3(x, y, 0);
        
        EnemyProjectile projectile = Object.Instantiate(enemy.enemyConfig.projectile, enemy.bulletStart[bulletStartInd].position, Quaternion.LookRotation(direction)).GetComponent<EnemyProjectile>();
        if(attackCount == 3 || attackCount == 6)
            projectile.ChangeParryable(true);
        projectile.SetTarget(enemy.targetTransform);
        projectile.SetParent(enemy.transform);
    }

    IEnumerator TimedShooting(EnemyStateManager enemy)
    {
        for(int i = 0; i < enemy.enemyConfig.bulletsPerShot; i++)
        {
            //SpawnMuzzleFlash(enemy);
            Shoot(enemy);
            yield return new WaitForSeconds(enemy.enemyConfig.timeBetweenShots);
            
        }
    }

    private void SpawnMuzzleFlash(EnemyStateManager enemy)
    {
        GameObject particles = Object.Instantiate(enemy.enemyConfig.muzzleFlashPrefab, enemy.bulletStart[0].position, enemy.bulletStart[0].rotation);
        Object.Destroy(particles, 0.5f);
    }




    
}


public class PiggyAttack : IAttackBehavior
{
    
    public void ExecuteAttack(EnemyStateManager enemy)
    {
        
        
        //if(!canAttack)
        //    yield break;
        Debug.Log("Attack attempt");
        enemy.lookDir = enemy.GetVectorToTarget();

        enemy.canAttack = false;
        enemy.animator.Play("Attack", 0, 0f);

        var events = enemy.animEvents.GetEvents("Attack");

        events.Clear();

        events.Add(0.25f, () =>
        {
            enemy.lookDir = enemy.GetVectorToTarget();
            if (enemy.GetVectorToTarget().sqrMagnitude > enemy.enemyConfig.endAttackRange * enemy.enemyConfig.endAttackRange)
            {
                enemy.SwitchState(enemy.ChaseState);
                return;
            }
            Hit(enemy);
        });

        

        // Second projectile at 60% from bulletStart[1]
        events.Add(0.6f, () =>
        {
            enemy.lookDir = enemy.GetVectorToTarget();
            if (enemy.GetVectorToTarget().sqrMagnitude > enemy.enemyConfig.endAttackRange * enemy.enemyConfig.endAttackRange)
            {
                enemy.SwitchState(enemy.ChaseState);
                return;
            }
            Hit(enemy);
        });

        enemy.lookDir = enemy.GetVectorToTarget();

        events.Add(0.90f, () =>
        {
            enemy.lookDir = enemy.GetVectorToTarget();
            if (enemy.GetVectorToTarget().sqrMagnitude > enemy.enemyConfig.endAttackRange * enemy.enemyConfig.endAttackRange)
            {
                enemy.SwitchState(enemy.ChaseState);
                return;
            }
            Hit(enemy);
        });

        // Reset when animation ends
        events.Add(1f, () => 
        {
            enemy.StartCoroutine(enemy.ResetAttack());
            enemy.animator.Play("Attack");
        });

        
    }

    private void Hit(EnemyStateManager enemy)
    {
        if(Physics.Raycast(enemy.transform.position, enemy.transform.forward, out RaycastHit hit, enemy.enemyConfig.attackRange, 1 << 3, QueryTriggerInteraction.Ignore))
        {
            hit.transform.GetComponent<PlayerHitResponder>().TakeDamage(enemy.transform, enemy.enemyConfig.damage, hit.point);
        }
    }



    public bool IsInAttackRange(EnemyStateManager enemy)
    {
        return Vector3.Distance(enemy.transform.position, enemy.targetTransform.position) <= enemy.enemyConfig.attackRange;
    }

    public bool CanAttack(EnemyStateManager enemy)
    {
        //return (Time.time - lastAttackTime) >= enemy.enemyConfig.attackCooldown;
        return enemy.canAttack;
    }

    
}


////////////////////////////////////////////////////////////////////
// TRAVERSAL BEHAVIOR
////////////////////////////////////////////////////////////////////


public interface ITraversalBehavior
{
    public void Chase(EnemyStateManager enemy);
}

public class MeleeTraversal : ITraversalBehavior
{
    public void Chase(EnemyStateManager enemy)
    {
        if(enemy.GetVectorToTarget().sqrMagnitude > enemy.enemyConfig.startAttackRange * enemy.enemyConfig.startAttackRange || !enemy.IsTargetInSight())
        {
            
            enemy.GoToTarget(enemy.enemyConfig.chaseSpeed);
            
        }
        else
        {
            enemy.SwitchState(enemy.AttackState);
        }
        
        //enemy.RotateInDirection(Vector3.ProjectOnPlane(enemy.GetVectorToPlayerPosition(), enemy.transform.up).normalized);
    }
}

public class RangedTraversal : ITraversalBehavior
{
    private float chaseDuration = Random.Range(0.6f, 1f);
    private float chaseStartTime = float.NaN;
    
    private bool chaseTimerStarted = false;
    public void Chase(EnemyStateManager enemy)
    {
        if(!chaseTimerStarted)
        {
            chaseStartTime = Time.time;
            chaseTimerStarted = true;
        }

        if(enemy.GetVectorToTarget().sqrMagnitude > enemy.enemyConfig.startAttackRange * enemy.enemyConfig.startAttackRange 
        || !enemy.IsTargetInSight() 
        || !Physics.BoxCast(enemy.bulletStart[0].position, new Vector3(0.5f, 0.5f, 0.5f), enemy.targetTransform.position - enemy.bulletStart[0].position, enemy.transform.rotation, enemy.enemyConfig.attackRange, enemy.playerLayer) 
        /*|| (Time.time - chaseStartTime) < chaseDuration*/)
        {
            enemy.GoToTarget(enemy.enemyConfig.chaseSpeed);
        }
        else
        {
            chaseTimerStarted = false;
            enemy.SwitchState(enemy.AttackState);
            return;
        }
        
    }

}
