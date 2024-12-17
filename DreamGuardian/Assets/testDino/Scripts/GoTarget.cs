using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoTarget : MonoBehaviour
{
    public float speed = 1f; // ĳ������ �̵� �ӵ�
    public float stopDistance = 1f; // ���ߴ� �Ÿ� (�� ª�� ���� ����)

    public float pushBackForce = 3f;    // �浹 �� �ڷ� �и��� ��
    public float stunDuration = 4f;     // ���� ���� �ð�

    private Transform target; // ���� ����� target ������Ʈ�� Transform
    private Animator anim;  // �ִϸ����� ������Ʈ
    private Rigidbody rb;   // Rigidbody ������Ʈ

    private bool isCollided = false;    // �浹 ���� Ȯ��
    private bool isStunned = false;     // ���� ���� Ȯ��

    void Start()
    {
        // Animator ������Ʈ�� �����ɴϴ�.
        anim = GetComponent<Animator>();

        // Rigidbody ������Ʈ�� �����ɴϴ�.
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!isCollided && !isStunned)
        {
            FindClosestTarget(); // ����� target ã��

            if (target != null)
            {
                //RotateTowardsTarget();
                //MoveTowardsTarget(); // target �������� �̵�
                MoveAndRotateTowardsTarget();
            }

        }

    }

    // ���� ����� Wall �±׸� ���� ������Ʈ ã��
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
            target = closestTarget.transform; // Ÿ���� ã���� ����
        }
        else
        {
            target = null; // Ÿ���� ������ null�� ����
        }
    }

    // target �������� �̵��ϴ� �Լ�
    void MoveTowardsTarget()
    {
        // ���� Ÿ�ٰ��� �Ÿ� ���
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Ÿ�ٱ����� �Ÿ��� stopDistance���� ũ�� �̵�
        if (distanceToTarget > stopDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized; // ��ǥ ����
            direction.y = 0;    // y�� ����

            transform.position += direction * speed * Time.deltaTime; // �̵�

            // Walk �ִϸ��̼� ���� (��� Walk ���� ����)
            anim.SetBool("IsWalk", true);
            anim.SetBool("IsAttack", false);
        }

        /*
        else
        {
            // �Ÿ��� stopDistance ���ϸ� �̵��� ���� (�ӵ��� 0����)
            transform.position = transform.position; // ���߱� ���� �ڵ� (�ӵ� 0)

            // Attack ���·� ����
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsAttack", true);
        }
        */
    }

    // Ÿ�� �������� Y�� ȸ���ϴ� �Լ�
    void RotateTowardsTarget()
    {
        Vector3 targetDirection = target.position - transform.position; // Ÿ�ٱ����� ����
        targetDirection.y = 0; // Y�� ȸ���� ����

        if (targetDirection != Vector3.zero) // ������ 0�� �ƴ� ��쿡�� ȸ��
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection); // Ÿ�� ���������� ȸ��
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // �ε巴�� ȸ��
        }
    }

    // Ÿ���� ���� �̵��� ȸ���� �Բ� �ϴ� �Լ�
    void MoveAndRotateTowardsTarget()
    {
        Vector3 targetDirection = (target.position - transform.position).normalized;    // Ÿ�� ����
        targetDirection.y = 0;  // y�� ����

        // 1. ȸ�� ó�� (�ε巴�� ȸ��)
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);   // Ÿ�� ���������� ȸ��
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 1f);

        // 2. �̵� ó��
        transform.position += transform.forward * speed * Time.deltaTime;   // ���� �������� �̵�

        // Walk �ִϸ��̼� ����
        anim.SetBool("IsWalk", true);
        anim.SetBool("IsAttack", false);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Wall �±׸� ���� ������Ʈ�� �浹�ϸ� �ִϸ��̼� ����
        if (collision.gameObject.CompareTag("Wall"))
        {
            isCollided = true;

            Debug.Log("�ε�����!!!");

            // �̵��� ���߰� Attack �ִϸ��̼� ����
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsAttack", true);
        }

        // ���Ͱ� ������ ������(Bullet �±׸� ���� ������Ʈ�� �浹�ϸ�
        // ���� �� �з��� ó��
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("���ݹ޾Ҵ�!");

            // ���� ó��
            StartCoroutine(StunAndPushBack(collision));
        }

    }

    IEnumerator StunAndPushBack(Collision collision)
    {
        isStunned = true;

        // ���� �ִϸ��̼� ���
        anim.SetBool("GetStun", true);
        // anim.SetTrigger("IsStun");

        // �浹 �ݴ� �������� �з����� ó��
        Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
        rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);

        // ���� ���� �ð� ���� ���
        yield return new WaitForSeconds(stunDuration);

        // ���� ���� �� ���� �ִϸ��̼����� ����
        isStunned = false;

        // Ʈ���� �ִϸ��̼� ����
        anim.SetBool("GetStun", false);
        // anim.ResetTrigger("IsStun");

        // Walk �ִϸ��̼� �簳
        anim.SetBool("IsWalk", true);
        anim.SetBool("IsAttack", false);

        // ���� Attack ���¿��ٸ�, �ٽ� Wall�� ���� �̵��ϵ��� ó��
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            isCollided = false;
        }


    }
}