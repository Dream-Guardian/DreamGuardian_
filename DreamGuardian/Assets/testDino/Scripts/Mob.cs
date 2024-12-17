using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Mob : MonoBehaviour
{
    public float moveSpeed;
    public float rotationSpeed;
    public float maxHealth;
    public float currentHealth;
    public float attackDamage;
    public float monsterScale;

    public GameObject IceObject;
    public bool isFrozen;

    private Rigidbody rb;
    private Vector3 targetPosition;
    private Animator anim;
    private bool isCollidingWithWall;
    private Coroutine attackCoroutine;
    public float pushBackForce;
    public float stunDuration;
    private bool isStunned;
    private WaitForSeconds attackWait;
    private BarrierCol Barrier;

    private bool isDead;
    private Collider mobCollider;

    void Awake()
    {
        rotationSpeed = 1f;
        targetPosition = Vector3.zero;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        transform.localScale = new Vector3(monsterScale, monsterScale, monsterScale);
        anim.SetBool("IsWalk", true);
        anim.SetBool("IsAttack", false);
        anim.SetBool("GetStun", false);
        isCollidingWithWall = false;
        isStunned = false;
        attackWait = new WaitForSeconds(1f);
        isDead = false;
        mobCollider = GetComponent<Collider>();
        isFrozen = false;
    }

    public void OnEnable()
    {
        anim.SetBool("IsWalk", true);
        anim.SetBool("IsAttack", false);
        anim.SetBool("GetStun", false);
        isCollidingWithWall = false;
        isStunned = false;
        currentHealth = maxHealth;
        isDead = false;
        mobCollider.enabled = true;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        bool currentlyCollidingWithWall = IsCollidingWithWall();
        if (isFrozen)
        {
            anim.SetBool("IsStun", true);
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }
        else if (currentlyCollidingWithWall && !isCollidingWithWall)
        {
            isCollidingWithWall = true;
            anim.SetBool("IsAttack", true);
            if (attackCoroutine == null)
            {
                attackCoroutine = StartCoroutine(AttackCoroutine());
            }
        }
        else if (!currentlyCollidingWithWall && isCollidingWithWall)
        {
            isCollidingWithWall = false;
            anim.SetBool("IsWalk", true);
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
        }

        if (!isCollidingWithWall && !isStunned && !isFrozen)
        {
            MoveTowardsTarget();
        }
    }

    void MoveTowardsTarget()
    {
        if (!isDead)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            float angle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);
            float rotationAngle = Mathf.Clamp(angle, -rotationSpeed, rotationSpeed);
            transform.Rotate(Vector3.up, rotationAngle);

            Vector3 movement = transform.forward * moveSpeed * Time.fixedDeltaTime;
            movement += directionToTarget * moveSpeed * Time.fixedDeltaTime * 0.5f;

            rb.MovePosition(rb.position + movement);
        }
    }

    private bool IsCollidingWithWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 0.7f))
        {
            if (hit.collider.CompareTag("Barrier"))
            {
                Barrier = hit.collider.gameObject.GetComponentInChildren<BarrierCol>();
                return true;
            }
            else
            {
                Barrier = null;
            }
            return hit.collider.CompareTag("BottomWall") || hit.collider.CompareTag("Wall");
        }
        return false;
    }

    IEnumerator AttackCoroutine()
    {
        while (!isFrozen)
        {
            yield return attackWait;
            if (isFrozen)
            {
                anim.SetBool("IsStun", true);
                yield break;
            }
            if(Barrier == null)
                Base.instance.TakeDamage(attackDamage);
            else
            {
                MusicManager.Defense();
                Barrier.DecreaseHp();
            }
        }
    }

    public void TakeDamage(float damage, string effectType = "default")
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            switch (effectType)
            {
                case "stunAndPushBack":
                    StartCoroutine(StunAndPushBack());
                    break;
                case "stunAndPushBack2":
                    StartCoroutine(StunAndPushBack2());
                    break;
                case "frozen":
                    StartCoroutine(Frozen());
                    break;
                default:
                    StartCoroutine(StunAndPushBack());
                    break;
            }
        }
    }

    IEnumerator Frozen()
    {
        if (IceObject != null)
        {
            isFrozen = true;
            isStunned = true;
            IceObject.SetActive(true);
            anim.SetBool("IsAttack", false);
            anim.SetBool("isFrozen", true);
            yield return new WaitForSeconds(4);
            //anim.SetBool("GetStun", false);
            isFrozen = false;
            isStunned = false;
            anim.SetBool("isFrozen", false);
            IceObject.SetActive(false);

            if (isCollidingWithWall)
            {
                anim.SetBool("IsAttack", true);
            }
            else
            {
                anim.SetBool("IsWalk", true);
            }
        }
    }

    void Die()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.killCnt++;
        }

        StartCoroutine(AfterDieAction());
    }

    IEnumerator StunAndPushBack()
    {
        isStunned = true;
        anim.SetBool("IsStun", true);

        Vector3 pushDirection = -transform.forward;
        rb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;

        if (isCollidingWithWall)
        {
            anim.SetBool("IsAttack", true);
        }
        else
        {
            anim.SetBool("IsWalk", true);
        }
    }

    IEnumerator StunAndPushBack2()
    {
        isStunned = true;
        anim.SetBool("IsStun", true);

        Vector3 pushDirection = -transform.forward;
        rb.AddForce(pushDirection * pushBackForce * 0.2f, ForceMode.Impulse);

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;

        if (isCollidingWithWall)
        {
            anim.SetBool("IsAttack", true);
        }
        else
        {
            anim.SetBool("IsWalk", true);
        }
    }

    IEnumerator AfterDieAction()
    {
        anim.SetTrigger("Dead");

        // 충분한 대기 시간을 설정하여 죽는 애니메이션이 완료될 수 있도록 합니다.
        if (anim.GetCurrentAnimatorStateInfo(0).length > 1)
        {
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + 1);
        }
        else
        {
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        }



        // 애니메이션이 완료된 후 객체를 풀로 반환합니다.
        if (GameManager.instance.staticBossCount == 1)
        {
            SpawnManager.instance.ReturnToPool(this.gameObject);
        }
        else if(GameManager.instance.staticBossCount == 2)
        {
            SpawnManager2.instance.ReturnToPool(this.gameObject);
        }
        isDead = true;
    }
}
