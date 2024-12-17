using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstBoss : OriginalBoss
{
    public GameObject currentPlayerTarget;
    public static FirstBoss FirstBossinstance;
    protected override void Awake()
    {
        base.Awake();
        FirstBossinstance = this;
    }

    protected override void Start()
    {
        base.Start();
        UpdateCurrentPlayerTarget();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isFrozen)
        {
            UpdateCurrentPlayerTarget();
        }
    }

    public override void ShootBullet()
    {
        if (isFrozen) return;
        if (bulletPool.Count > 0 && currentPlayerTarget != null)
        {
            GameObject bullet = bulletPool.Dequeue();

            Vector3 shootTargetPosition = currentPlayerTarget.transform.position;
            shootTargetPosition.y = 2f;
            if (Vector3.Distance(mouthTransform.position, shootTargetPosition) > 7)
            {
                shootTargetPosition = targetPosition;
            }

            Vector3 direction = (shootTargetPosition - mouthTransform.position).normalized;
            BossBullet bossBullet = bullet.GetComponent<BossBullet>();
            if (bossBullet != null)
            {
                bossBullet.Initialize(direction, bulletSpeed, mouthTransform.position, shootTargetPosition);
            }
        }
    }

    void UpdateCurrentPlayerTarget()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestPlayer = null;

        foreach (GameObject player in playerTargets)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            Character character = player.GetComponent<Character>();
            if (distance < closestDistance && character.statement != "DEAD")
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        currentPlayerTarget = closestPlayer;
    }
}