using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoTarget : MonoBehaviour
{
    public float speed = 1f; // 캐릭터의 이동 속도
    public float stopDistance = 1f; // 멈추는 거리 (더 짧게 설정 가능)

    public float pushBackForce = 3f;    // 충돌 시 뒤로 밀리는 힘
    public float stunDuration = 4f;     // 스턴 지속 시간

    private Transform target; // 가장 가까운 target 오브젝트의 Transform
    private Animator anim;  // 애니메이터 컴포넌트
    private Rigidbody rb;   // Rigidbody 컴포넌트

    private bool isCollided = false;    // 충돌 여부 확인
    private bool isStunned = false;     // 스턴 여부 확인

    void Start()
    {
        // Animator 컴포넌트를 가져옵니다.
        anim = GetComponent<Animator>();

        // Rigidbody 컴포넌트를 가져옵니다.
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!isCollided && !isStunned)
        {
            FindClosestTarget(); // 가까운 target 찾기

            if (target != null)
            {
                //RotateTowardsTarget();
                //MoveTowardsTarget(); // target 방향으로 이동
                MoveAndRotateTowardsTarget();
            }

        }

    }

    // 가장 가까운 Wall 태그를 가진 오브젝트 찾기
    void FindClosestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Wall");
        float closestDistance = Mathf.Infinity;
        GameObject closestTarget = null;

        foreach (GameObject potentialTarget in targets)
        {
            float distance = Vector3.Distance(transform.position, potentialTarget.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = potentialTarget;
            }
        }

        if (closestTarget != null)
        {
            target = closestTarget.transform; // 타겟을 찾으면 설정
        }
        else
        {
            target = null; // 타겟이 없으면 null로 설정
        }
    }

    // target 방향으로 이동하는 함수
    void MoveTowardsTarget()
    {
        // 현재 타겟과의 거리 계산
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 타겟까지의 거리가 stopDistance보다 크면 이동
        if (distanceToTarget > stopDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized; // 목표 방향
            direction.y = 0;    // y축 고정

            transform.position += direction * speed * Time.deltaTime; // 이동

            // Walk 애니메이션 설정 (계속 Walk 상태 유지)
            anim.SetBool("IsWalk", true);
            anim.SetBool("IsAttack", false);
        }

        /*
        else
        {
            // 거리가 stopDistance 이하면 이동을 멈춤 (속도를 0으로)
            transform.position = transform.position; // 멈추기 위한 코드 (속도 0)

            // Attack 상태로 유지
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsAttack", true);
        }
        */
    }

    // 타겟 방향으로 Y축 회전하는 함수
    void RotateTowardsTarget()
    {
        Vector3 targetDirection = target.position - transform.position; // 타겟까지의 방향
        targetDirection.y = 0; // Y축 회전을 고정

        if (targetDirection != Vector3.zero) // 방향이 0이 아닌 경우에만 회전
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection); // 타겟 방향으로의 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // 부드럽게 회전
        }
    }

    // 타겟을 향해 이동과 회전을 함께 하는 함수
    void MoveAndRotateTowardsTarget()
    {
        Vector3 targetDirection = (target.position - transform.position).normalized;    // 타겟 방향
        targetDirection.y = 0;  // y축 고정

        // 1. 회전 처리 (부드럽게 회전)
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);   // 타겟 방향으로의 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 1f);

        // 2. 이동 처리
        transform.position += transform.forward * speed * Time.deltaTime;   // 전방 방향으로 이동

        // Walk 애니메이션 설정
        anim.SetBool("IsWalk", true);
        anim.SetBool("IsAttack", false);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Wall 태그를 가진 오브젝트와 충돌하면 애니메이션 변경
        if (collision.gameObject.CompareTag("Wall"))
        {
            isCollided = true;

            Debug.Log("부딪혔다!!!");

            // 이동을 멈추고 Attack 애니메이션 실행
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsAttack", true);
        }

        // 몬스터가 공격을 받으면(Bullet 태그를 가진 오브젝트와 충돌하면
        // 스턴 및 밀려남 처리
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("공격받았다!");

            // 스턴 처리
            StartCoroutine(StunAndPushBack(collision));
        }

    }

    IEnumerator StunAndPushBack(Collision collision)
    {
        isStunned = true;

        // 스턴 애니메이션 재생
        anim.SetBool("GetStun", true);
        // anim.SetTrigger("IsStun");

        // 충돌 반대 방향으로 밀려나는 처리
        Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
        rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);

        // 스턴 지속 시간 동안 대기
        yield return new WaitForSeconds(stunDuration);

        // 스턴 종료 후 원래 애니메이션으로 복귀
        isStunned = false;

        // 트리거 애니메이션 끄기
        anim.SetBool("GetStun", false);
        // anim.ResetTrigger("IsStun");

        // Walk 애니메이션 재개
        anim.SetBool("IsWalk", true);
        anim.SetBool("IsAttack", false);

        // 만약 Attack 상태였다면, 다시 Wall을 향해 이동하도록 처리
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            isCollided = false;
        }


    }
}