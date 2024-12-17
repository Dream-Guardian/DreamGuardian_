using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using Photon.Pun;
using DG.Tweening;

public class Weapon : MonoBehaviourPunCallbacks
{
    public static Weapon Instance;
    public GameObject bulletPrefab;
    public int bulletPoolSize;
    private Queue<GameObject> bulletPool;

    public ParticleSystem gunFlash;
    public Transform firePoint;
    public float fireRate;
    public float bulletSpeed;
    public float firingRange;
    public float detectionRange;
    private int damage;
    public float rotationSpeed;
    public PhotonView pv;
    public float arcHeight;
    public Coroutine firingCoroutine;
    protected Capsule capsule;

    protected bool isFiring = false;
    protected Transform currentTarget;
    protected Gun gunComponent;
    protected GameObject currentTargetEnemy;
    public float bulletRadius;
    private WaitForSeconds attackWait;
    private Coroutine findEnemyCoroutine;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        damage = 30;
        rotationSpeed = 0.1f;
        arcHeight = 3f;
        bulletRadius = 0.3f;
        attackWait = new WaitForSeconds(fireRate);
        firingRange = 8f;
        detectionRange = 8f;
        InitializePool();
    }

    override public void OnEnable()
    {
        gunComponent = GetComponent<Gun>();
        base.OnEnable();
        findEnemyCoroutine = StartCoroutine(FindNearestEnemyRoutine());
    }

    override public void OnDisable()
    {
        base.OnDisable();
        if (findEnemyCoroutine != null)
            StopCoroutine(findEnemyCoroutine);
    }

    void InitializePool()
    {
        bulletPool = new Queue<GameObject>();
        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetWeaponReference(this);
            }
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
        return null;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    protected virtual IEnumerator FindNearestEnemyRoutine()
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
            } // 캡슐이 다 닳았을 때 처리
            yield return wait;
        }
        StopFiring();
    }

    protected IEnumerator FireRoutine()
    {
        while (currentTarget != null && gunComponent.isTurretReady)
        {
            Fire();
            yield return attackWait;
        }
        firingCoroutine = null;
    }

    protected virtual GameObject FindNearestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Mob");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");

        GameObject nearest = null;
        float nearestDistance = detectionRange;

        Vector3 weaponPosition = transform.position;

        foreach (GameObject enemy in enemies)
        {
            Mob mobComponent = enemy.GetComponentInChildren<Mob>();
            if (mobComponent == null || mobComponent.currentHealth <= 0) continue;

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
            if (bossComponent == null || bossComponent.currentHealth <= 0) continue;

            float distance = Vector3.Distance(weaponPosition, boss.transform.position);

            if (distance < nearestDistance)
            {
                nearest = boss;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    protected virtual void UpdateTargetToEnemy(GameObject nearestEnemy)
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

    protected void StartFiring()
    {
        firingCoroutine = null;
        isFiring = true;
    }

    protected void StopFiring()
    {
        isFiring = false;
        if (firingCoroutine != null)
            StopCoroutine(firingCoroutine);
        firingCoroutine = null;
    }

    void Fire()
    {
        if (currentTargetEnemy == null) return;

        GameObject bullet = GetBullet();
        if (bullet != null)
        {

            bullet.transform.position = firePoint.position;

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                Vector3 targetPosition = currentTarget.position;
                Vector3 directionToTarget = (targetPosition - firePoint.position).normalized;
                float distanceToTarget = Vector3.Distance(firePoint.position, targetPosition);

                RaycastHit hit;
                bool isObstacleInPath = Physics.SphereCast(firePoint.position, bulletRadius, directionToTarget, out hit, distanceToTarget);

                gunFlash.Play();
                if (isObstacleInPath && hit.collider.gameObject != currentTargetEnemy)
                {
                    bulletScript.FireParabolic(targetPosition, damage, firingRange, arcHeight);
                }
                else
                {
                    bulletScript.Initialize(directionToTarget * bulletSpeed, damage, firingRange, firePoint.position);
                }

                if (PhotonNetwork.IsMasterClient)
                    pv.RPC("RPCPlayEffectETC", RpcTarget.AllViaServer);
            }

        }
        else
        {
            Debug.Log("bullet 풀이 모두 활성화되어 생성 불가");
        }

    }

    [PunRPC]
    protected virtual void RPCPlayEffectETC()
    {
        if (capsule != null)
            capsule.DecreaseHp(10f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, firingRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * firingRange);
    }
}