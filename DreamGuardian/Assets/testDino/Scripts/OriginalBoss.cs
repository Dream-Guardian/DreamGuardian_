using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginalBoss : Boss
{
    public float moveSpeed;
    public float rotationSpeed;
    public GameObject bulletPrefab;
    public int bulletPoolSize;
    public float shootInterval;
    public float shootDistance;
    public float bulletSpeed;
    public GameObject mouth;
    public Transform mouthTransform;
    public GameObject[] wallTargets;
    public GameObject[] playerTargets;
    public Rigidbody rb;
    public Vector3 targetPosition;
    public Queue<GameObject> bulletPool;
    public bool isShooting;
    public WaitForSeconds attackWait;

    protected override void Awake()
    {
        base.Awake();
        moveSpeed = 0.3f;
        rotationSpeed = 0.5f;
        bulletPoolSize = 20;
        shootInterval = 2f;
        bulletSpeed = 5f;
        shootDistance = 4f;
        isShooting = false;
        wallTargets = GameObject.FindGameObjectsWithTag("Wall");
        playerTargets = GameObject.FindGameObjectsWithTag("Player");
        attackWait = new WaitForSeconds(shootInterval);

        rb = GetComponent<Rigidbody>();
        mouthTransform = mouth.transform;
        anim.SetBool("IsWalk", true);
        anim.SetBool("IsAttack", false);

        InitializeBulletPool();
    }

    protected override void Start()
    {
        base.Start();
        float closestDistance = Mathf.Infinity;
        GameObject closestTarget = null;

        foreach (GameObject potentialTarget in wallTargets)
        {
            Wall wall = potentialTarget.GetComponent<Wall>();
            if (wall.rotation == -1) continue;
            float distance = Vector3.Distance(transform.position, potentialTarget.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = potentialTarget;
            }
        }
        targetPosition = closestTarget.transform.position;
    }



    protected virtual void FixedUpdate()
    {
        if (isFrozen)
        {
            StopCoroutine(ShootRoutine());
            isShooting = false;
        }
        else
        {
            MoveTowardsTarget();
        }
    }

    void MoveTowardsTarget()
    {

        if (Vector3.Distance(transform.position, targetPosition) > shootDistance)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;

            float angle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
            float rotationAngle = Mathf.Clamp(angle, -rotationSpeed, rotationSpeed);
            transform.Rotate(Vector3.up, rotationAngle);
            Vector3 movement = transform.forward * moveSpeed * Time.fixedDeltaTime;
            movement += directionToTarget * moveSpeed * Time.fixedDeltaTime * 0.5f;
            rb.MovePosition(rb.position + movement);
        }
        else if (!isShooting)
        {
            StartCoroutine(ShootRoutine());
        }
    }

    void InitializeBulletPool()
    {
        bulletPool = new Queue<GameObject>();
        for (int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, new Vector3(100f, 100f, 100f), Quaternion.identity);
            BossBullet bossBullet = bullet.GetComponent<BossBullet>();
            if (bossBullet != null)
            {
                bossBullet.SetBossReference(this);
            }
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    IEnumerator ShootRoutine()
    {
        Debug.Log("routine start");
        isShooting = true;
        anim.SetBool("IsWalk", false);
        anim.SetBool("IsAttack", true);
        while (Vector3.Distance(transform.position, targetPosition) <= shootDistance)
        {
            ShootBullet();
            yield return attackWait;
        }
        isShooting = false;
        anim.SetBool("IsWalk", true);
        anim.SetBool("IsAttack", false);
    }

    public virtual void ShootBullet()
    {
        Debug.Log("SHOOT");
        if (isFrozen) return;
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();

            Vector3 shootTargetPosition = targetPosition;

            Vector3 direction = (shootTargetPosition - mouthTransform.position).normalized;
            BossBullet bossBullet = bullet.GetComponent<BossBullet>();
            if (bossBullet != null)
            {
                bossBullet.Initialize(direction, bulletSpeed, mouthTransform.position, shootTargetPosition);
            }
        }
    }

    public void ReturnBulletToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}