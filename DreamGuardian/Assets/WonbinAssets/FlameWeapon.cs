using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;  // Concat 사용하기 위해 필요

public class FlameWeapon : MonoBehaviourPunCallbacks
{
    private Gun gunComponent;
    private GameObject currentTargetEnemy;  // 현재 타겟
    public float fireDuration = 3f;          // 발사 시간
    public float detectionRange;              // 감지 범위
    public Transform firePoint;               // 발사 위치
    public PhotonView pv;                     // 포톤 뷰
    public float rotationSpeed;               // 포탑 회전 속도
    public GameObject Flame;                  // 총알(Flame)
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
        pv = GetComponent<PhotonView>(); // 포톤 뷰 컴포넌트 가져오기
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
            // currentTargetEnemy를 향해 회전
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

    // 범위 내 가까운 적을 찾는 메서드
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
            // Mob 또는 Boss 컴포넌트 가져오기
            Mob mobComponent = enemy.GetComponent<Mob>();
            Boss bossComponent = enemy.GetComponent<Boss>();

            // 컴포넌트가 없거나 체력이 0 이하인 경우 무시
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
            yield return null; // 다음 프레임까지 대기
        }

        StopFiring();
        yield return new WaitForSeconds(1f); // 1초 대기
        isFiring = false; // 다시 공격 가능 상태로 설정
    }

    void Fire()
    {
        if (currentTargetEnemy == null) return;

        // 위치 업데이트
        flameParticleSystem.transform.position = firePoint.position;

        // Y축 각도 업데이트
        Vector3 firePointEulerAngles = firePoint.eulerAngles;
        flameParticleSystem.transform.eulerAngles = new Vector3(firePointEulerAngles.x, firePointEulerAngles.y, firePointEulerAngles.z);

        // currentTargetEnemy.position 과 firePoint.position 거리가 6f 미만인 경우
        Vector3 dir = (firePoint.position - currentTargetEnemy.transform.position);

        if (dir.magnitude < 6f)
        {
            flameParticleSystem.Play();
            if (PhotonNetwork.IsMasterClient)
                pv.RPC("RPCPlayEffectETC", RpcTarget.AllViaServer);
            // 범위 내 적들에게 데미지 주기
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

    // 적 방향으로 Y축만 회전하기
    void RotateTowardsEnemy()
    {
        // 터렛 준비안되면 return
        if (!gunComponent.isTurretReady) return;
        Vector3 direction = (currentTargetEnemy.transform.position - transform.position).normalized;
        direction.y = 0; // Y축 방향을 0으로 설정하여 수평 방향만 고려

        if (direction != Vector3.zero) // 방향이 0이 아닐 때만 회전
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Quaternion currentRotation = transform.rotation;

            // 현재 회전의 Y축 값만 업데이트
            transform.rotation = Quaternion.Slerp(currentRotation, Quaternion.Euler(0, lookRotation.eulerAngles.y, 0), rotationSpeed * Time.deltaTime);

            // 부모의 자식 중 이름에 "Body_enforce"가 포함된 GameObject 찾기
            foreach (Transform child in transform.parent)
            {
                if (child.name.Contains("Body_enforce"))
                {
                    // Enforce_body의 회전도 업데이트
                    child.rotation = Quaternion.Slerp(child.rotation, Quaternion.Euler(0, lookRotation.eulerAngles.y, 0), rotationSpeed * Time.deltaTime);
                }
            }
        }
    }


    // 타겟 적 업데이트
    void UpdateTargetToEnemy(GameObject nearEnemy)
    {
        currentTargetEnemy = nearEnemy;
    }

    // 캡슐 사용
    [PunRPC]
    void RPCPlayEffectETC()
    {
        if (capsule != null)
            capsule.DecreaseHp(0.3f);
    }

    // 사다리꼴 4지점 찾기
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

    // 범위 내 적들에게 데미지를 주는 메서드
    private void ApplyDamageToEnemiesInRange()
    {
        // 범위 내 적들을 찾기 위해 사다리꼴의 정점을 구합니다.
        Vector3[] trapezoid = GetTrapezoidVertices();
        List<GameObject> enemiesInRange = IsEnemyInTrapezoid(trapezoid);

        // 범위 내 적들에게 데미지 적용
        ApplyDamageToEnemies(enemiesInRange, damage * 0.2f); // 0.2초에 한번씩 데미지
    }


    // 적들에게 데미지 주는 메서드
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

    // 사다리꼴 내부의 적들을 반환하는 메서드
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

    // 삼각형 내부에 점이 있는지 확인하는 메서드
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

    // 사각형 내부에 점이 있는지 확인하는 메서드
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
