using Photon.Pun;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;
using UnityEngine.Audio;

public class Bullet : MonoBehaviourPunCallbacks
{
    public IObjectPool<GameObject> Pool { get; set; }
    public float damage;
    private Weapon weapon;
    private float range;
    private Vector3 startPosition;
    private Rigidbody rb;

    private ParticleSystem gunFire;
    private TrailRenderer trail;
    private bool isDisabled;
    private Tween currentTween;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        isDisabled = false;
    }

    public void SetWeaponReference(Weapon weaponReference)
    {
        weapon = weaponReference;
    }

    public void Initialize(Vector3 direction, float damage, float range, Vector3 startPosition)
    {
        this.damage = damage;
        this.range = range;
        this.startPosition = startPosition;
        transform.position = startPosition;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = direction;
        }
        isDisabled = false;
        gameObject.SetActive(true);
        MusicManager.Firesound();
    }

    public void FireParabolic(Vector3 targetPosition, float damage, float range, float arcHeight)
    {
        this.damage = damage;
        this.range = range;
        this.startPosition = transform.position;
        isDisabled = false;
        gameObject.SetActive(true);

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        float duration = Vector3.Distance(transform.position, targetPosition) / 10f;
        currentTween = transform.DOJump(targetPosition, arcHeight, 1, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                DisableBullet();
            });
        MusicManager.Firesound();
    }

    void FixedUpdate()
    {
        if (rb != null && !rb.isKinematic)
        {
            if (Vector3.Distance(startPosition, transform.position) > range)
            {
                DisableBullet();
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        DisableBullet();
        if (collision.gameObject.CompareTag("Mob"))
        {
            Mob mob = collision.gameObject.GetComponent<Mob>();
            if (mob != null)
            {
                mob.TakeDamage(damage);
            }
        }
        else if (collision.gameObject.CompareTag("Boss"))
        {
            Boss boss = collision.gameObject.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }
        }
        MusicManager.Enemy();
        return;
    }

    protected void DisableBullet()
    {
        if (!isDisabled)
        {
            isDisabled = true;
            if (currentTween != null)
            {
                currentTween.Kill();
            }
            gameObject.SetActive(false);
            weapon.ReturnBullet(gameObject);
        }
    }
}
