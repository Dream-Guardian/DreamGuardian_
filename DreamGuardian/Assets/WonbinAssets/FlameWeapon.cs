using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;  // Concat ����ϱ� ���� �ʿ�

public class FlameWeapon : MonoBehaviourPunCallbacks
{
    private Gun gunComponent;
    private GameObject currentTargetEnemy;  // ���� Ÿ��
    public float fireDuration = 3f;          // �߻� �ð�
    public float detectionRange;              // ���� ����
    public Transform firePoint;               // �߻� ��ġ
    public PhotonView pv;                     // ���� ��
    public float rotationSpeed;               // ��ž ȸ�� �ӵ�
    public GameObject Flame;                  // �Ѿ�(Flame)
    private ParticleSystem flameParticleSystem;
    private bool isFiring = false;
    private Capsule capsule;
    private Coroutine FindEnemy;
    private float damage = 3f;

    // Start is called before the first frame update
    void Awake()
    {
        flameParticleSystem = Flame.GetComponent<ParticleSystem>();
        flameParticleSystem.Stop();
        pv = GetComponent<PhotonView>(); // ���� �� ������Ʈ ��������
        rotationSpeed = 3f;
    }

    override public void OnEnable()
    {
        gunComponent = GetComponent<Gun>();
        if (gunComponent == null)
        {
            Debug.LogError("Gun component not found.");
        }
        base.OnEnable();
        if (gunComponent.isTurretReady)
        {
            FindEnemy = StartCoroutine(FindNearestEnemyRoutine());
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (FindEnemy != null)
            StopCoroutine(FindEnemy);
    }

    void FixedUpdate()
    {
        if (currentTargetEnemy != null)
        {
            // currentTargetEnemy�� ���� ȸ��
            RotateTowardsEnemy();
        }
    }

    IEnumerator FindNearestEnemyRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        capsule = transform.parent.GetComponentInChildren<Capsule>();
        while (true)
        {
            if (gunComponent != null && gunComponent.isTurretReady && capsule != null)
            {
                GameObject nearestEnemy = FindNearestEnemyInRange();
                UpdateTargetToEnemy(nearestEnemy);
                if (nearestEnemy != null && !isFiring && capsule.capsuleHP > 0)
                {
                    StartCoroutine(FireRoutine());
                }
            }
            else
            {
                StopFiring();
            }
            yield return wait;
        }
    }

    // ���� �� ����� ���� ã�� �޼���
    GameObject FindNearestEnemyInRange()
    {
        GameObject[] enemiesAndBosses = GameObject.FindGameObjectsWithTag("Mob")
            .Concat(GameObject.FindGameObjectsWithTag("Boss"))
            .ToArray();

        GameObject nearest = null;
        float nearestDistance = detectionRange;
        Vector3 weaponPosition = transform.position;

        foreach (GameObject enemy in enemiesAndBosses)
        {
            // Mob �Ǵ� Boss ������Ʈ ��������
            Mob mobComponent = enemy.GetComponent<Mob>();
            Boss bossComponent = enemy.GetComponent<Boss>();

            // ������Ʈ�� ���ų� ü���� 0 ������ ��� ����
            if ((mobComponent == null || mobComponent.currentHealth <= 0) &&
                (bossComponent == null || bossComponent.currentHealth <= 0)) continue;

            float distance = Vector3.Distance(weaponPosition, enemy.transform.position);

            if (distance < nearestDistance)
            {
                nearest = enemy;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    IEnumerator FireRoutine()
    {
        isFiring = true;
        float elapsedTime = 0f;

        MusicManager.Flame();
        while (elapsedTime < fireDuration)
        {
            Fire();
            elapsedTime += Time.deltaTime;
            yield return null; // ���� �����ӱ��� ���
        }

        StopFiring();
        yield return new WaitForSeconds(1f); // 1�� ���
        isFiring = false; // �ٽ� ���� ���� ���·� ����
    }

    void Fire()
    {
        if (currentTargetEnemy == null) return;

        // ��ġ ������Ʈ
        flameParticleSystem.transform.position = firePoint.position;

        // Y�� ���� ������Ʈ
        Vector3 firePointEulerAngles = firePoint.eulerAngles;
        flameParticleSystem.transform.eulerAngles = new Vector3(firePointEulerAngles.x, firePointEulerAngles.y, firePointEulerAngles.z);

        // currentTargetEnemy.position �� firePoint.position �Ÿ��� 6f �̸��� ���
        Vector3 dir = (firePoint.position - currentTargetEnemy.transform.position);

        if (dir.magnitude < 6f)
        {
            flameParticleSystem.Play();
            if (PhotonNetwork.IsMasterClient)
                pv.RPC("RPCPlayEffectETC", RpcTarget.AllViaServer);
            // ���� �� ���鿡�� ������ �ֱ�
            ApplyDamageToEnemiesInRange();
        } else
        {
            MusicManager.FlameStop();
        }
    }

    void StopFiring()
    {
        if (isFiring)
        {
            flameParticleSystem.Stop();
        }
    }

    // �� �������� Y�ุ ȸ���ϱ�
    void RotateTowardsEnemy()
    {
        // �ͷ� �غ�ȵǸ� return
        if (!gunComponent.isTurretReady) return;
        Vector3 direction = (currentTargetEnemy.transform.position - transform.position).normalized;
        direction.y = 0; // Y�� ������ 0���� �����Ͽ� ���� ���⸸ ���

        if (direction != Vector3.zero) // ������ 0�� �ƴ� ���� ȸ��
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion currentRotation = transform.rotation;

            // ���� ȸ���� Y�� ���� ������Ʈ
            transform.rotation = Quaternion.Slerp(currentRotation, Quaternion.Euler(0, lookRotation.eulerAngles.y, 0), rotationSpeed * Time.deltaTime);

            // �θ��� �ڽ� �� �̸��� "Body_enforce"�� ���Ե� GameObject ã��
            foreach (Transform child in transform.parent)
            {
                if (child.name.Contains("Body_enforce"))
                {
                    // Enforce_body�� ȸ���� ������Ʈ
                    child.rotation = Quaternion.Slerp(child.rotation, Quaternion.Euler(0, lookRotation.eulerAngles.y, 0), rotationSpeed * Time.deltaTime);
                }
            }
        }
    }


    // Ÿ�� �� ������Ʈ
    void UpdateTargetToEnemy(GameObject nearEnemy)
    {
        currentTargetEnemy = nearEnemy;
    }

    // ĸ�� ���
    [PunRPC]
    void RPCPlayEffectETC()
    {
        if (capsule != null)
            capsule.DecreaseHp(0.3f);
    }

    // ��ٸ��� 4���� ã��
    private Vector3[] GetTrapezoidVertices()
    {
        Transform position1Transform = transform.Find("Point1");
        Transform position2Transform = transform.Find("Point2");
        Transform position3Transform = transform.Find("Point3");
        Transform position4Transform = transform.Find("Point4");
        //Debug.Log("position1Transform:", position1Transform);
        //Debug.Log("position2Transform:", position2Transform);
        //Debug.Log("position3Transform:", position3Transform);
        //Debug.Log("position4Transform:", position4Transform);
        return new Vector3[]
        {
            position1Transform.position,
            position2Transform.position,
            position3Transform.position,
            position4Transform.position,
        };
    }

    // ���� �� ���鿡�� �������� �ִ� �޼���
    private void ApplyDamageToEnemiesInRange()
    {
        // ���� �� ������ ã�� ���� ��ٸ����� ������ ���մϴ�.
        Vector3[] trapezoid = GetTrapezoidVertices();
        List<GameObject> enemiesInRange = IsEnemyInTrapezoid(trapezoid);

        // ���� �� ���鿡�� ������ ����
        ApplyDamageToEnemies(enemiesInRange, damage * 0.2f); // 0.2�ʿ� �ѹ��� ������
    }


    // ���鿡�� ������ �ִ� �޼���
    private void ApplyDamageToEnemies(List<GameObject> enemies, float damage)
    {
        if (enemies.Count == 0) return;

        foreach (GameObject enemy in enemies)
        {
            if (enemy.CompareTag("Mob"))
            {
                Mob mob = enemy.GetComponent<Mob>();
                if (mob != null && mob.currentHealth > 0)
                {
                    mob.TakeDamage(damage, "stunAndPushBack2");
                }
            }
            else if (enemy.CompareTag("Boss"))
            {
                Boss boss = enemy.GetComponent<Boss>();
                if (boss != null && boss.currentHealth > 0)
                {
                    boss.TakeDamage(damage);
                }
            }
        }
        //MusicManager.FlameEnemy();
    }

    // ��ٸ��� ������ ������ ��ȯ�ϴ� �޼���
    private List<GameObject> IsEnemyInTrapezoid(Vector3[] trapezoid)
    {
        GameObject[] enemiesAndBosses = GameObject.FindGameObjectsWithTag("Mob")
            .Concat(GameObject.FindGameObjectsWithTag("Boss"))
            .ToArray();

        List<GameObject> enemiesInTrapezoid = new List<GameObject>();

        foreach (GameObject enemy in enemiesAndBosses)
        {
            if (IsPointInTrapezoid(enemy.transform.position, trapezoid))
            {
                enemiesInTrapezoid.Add(enemy);
            }
        }

        return enemiesInTrapezoid;
    }

    // �ﰢ�� ���ο� ���� �ִ��� Ȯ���ϴ� �޼���
    private bool IsPointInTriangle(Vector2 p, Vector2[] tri)
    {
        Vector2 v0 = tri[1] - tri[0];
        Vector2 v1 = tri[2] - tri[0];
        Vector2 v2 = p - tri[0];

        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    // �簢�� ���ο� ���� �ִ��� Ȯ���ϴ� �޼���
    private bool IsPointInTrapezoid(Vector3 point, Vector3[] trapezoid)
    {
        Vector2 p = new Vector2(point.x, point.z);
        Vector2[] tri1 = {
            new Vector2(trapezoid[0].x, trapezoid[0].z),
            new Vector2(trapezoid[1].x, trapezoid[1].z),
            new Vector2(trapezoid[2].x, trapezoid[2].z)
        };
        Vector2[] tri2 = {
            new Vector2(trapezoid[0].x, trapezoid[0].z),
            new Vector2(trapezoid[2].x, trapezoid[2].z),
            new Vector2(trapezoid[3].x, trapezoid[3].z)
        };

        return IsPointInTriangle(p, tri1) || IsPointInTriangle(p, tri2);
    }
}
