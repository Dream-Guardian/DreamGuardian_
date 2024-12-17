using UnityEngine;
public class BossBullet : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private OriginalBoss boss;

    private ParticleSystem hit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hit = GetComponentInChildren<ParticleSystem>();
    }

    public void SetBossReference(OriginalBoss bossReference)
    {
        boss = bossReference;
    }

    public void Initialize(Vector3 dir, float bulletSpeed, Vector3 startPosition, Vector3 target)
    {
        direction = dir;
        speed = bulletSpeed;
        transform.position = startPosition;
        targetPosition = target;

        rb.velocity = direction * speed;

        transform.rotation = Quaternion.LookRotation(direction);
        gameObject.SetActive(true);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Character character = collision.gameObject.GetComponent<Character>();
            if (character.PV.IsMine)
            {
                character.die();
            }
            ReturnToPool();
        }
        else
        {
            Base.instance.TakeDamage(30f);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (boss != null)
        {
            boss.ReturnBulletToPool(gameObject);
        }
    }
}