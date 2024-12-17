using UnityEngine;
using System.Collections;
using Photon.Pun;

public class IceWeapon : Weapon
{
    protected override IEnumerator FindNearestEnemyRoutine()
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();
        capsule = transform.parent.GetComponentInChildren<Capsule>();
        while (gunComponent.isTurretReady)
        {
            if (capsule != null && capsule.capsuleHP > 0)
            {
                GameObject nearestEnemy = FindNearestEnemyInRange();
                if (nearestEnemy != null)
                {
                    UpdateTargetToEnemy(nearestEnemy);
                    if (isFiring && firingCoroutine == null)
                    {
                        firingCoroutine = StartCoroutine(FireRoutine());
                    }
                }
                else if (nearestEnemy == null)
                {
                    StopFiring();
                }
            }
            else
            {
                StopFiring();
            }
            yield return wait;
        }
        StopFiring();
    }

    protected override GameObject FindNearestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Mob");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");

        GameObject nearest = null;
        float nearestDistance = detectionRange;

        Vector3 weaponPosition = transform.position;

        foreach (GameObject enemy in enemies)
        {
            Mob mobComponent = enemy.GetComponentInChildren<Mob>();
            if (mobComponent == null || mobComponent.currentHealth <= 0 || mobComponent.isFrozen) continue;

            float distance = Vector3.Distance(weaponPosition, enemy.transform.position);

            if (distance < nearestDistance)
            {
                nearest = enemy;
                nearestDistance = distance;
            }
        }

        foreach (GameObject boss in bosses)
        {
            Boss bossComponent = boss.GetComponentInChildren<Boss>();
            if (bossComponent == null || bossComponent.currentHealth <= 0 || bossComponent.isFrozen) continue;

            float distance = Vector3.Distance(weaponPosition, boss.transform.position);

            if (distance < nearestDistance)
            {
                nearest = boss;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    protected override void UpdateTargetToEnemy(GameObject nearestEnemy)
    {
        if (!gunComponent.isTurretReady) return;

        currentTargetEnemy = nearestEnemy;
        currentTarget = nearestEnemy.transform.Find("Mesh");

        Vector3 targetPosition = currentTarget.position;
        Vector3 directionToEnemy = targetPosition - transform.position;
        directionToEnemy.y = 0;
        float distanceToEnemy = directionToEnemy.magnitude;



        if (distanceToEnemy <= firingRange)
        {
            Vector3 targetDirection = directionToEnemy.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
            foreach (Transform child in transform.parent)
            {
                if (child.name.Contains("Body_enforce"))
                {
                    // Body_enforce의 회전도 업데이트
                    child.rotation = Quaternion.Slerp(child.rotation, targetRotation, rotationSpeed);
                }
            }
            if (!isFiring)
            {
                StartFiring();
            }
        }
        else
        {
            StopFiring();
        }
    }
}
