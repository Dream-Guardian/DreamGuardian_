using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 100;
    public float speed = 1f;
    public float changeDirectionInterval = 2f;

    private Vector3 direction = Vector3.right;
    private float timeSinceLastDirectionChange = 0f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Dead();

            // 사망 시, 다음 음악 세션 재생
            FindObjectOfType<MusicManager>().ActivateNextLayer();
        }
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        Move();
        CheckDirectionChange();
    }

    private void Move()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void CheckDirectionChange()
    {
        timeSinceLastDirectionChange += Time.deltaTime;

        if (timeSinceLastDirectionChange >= changeDirectionInterval)
        {
            ChangeDirection();
            timeSinceLastDirectionChange = 0f;
        }
    }

    private void ChangeDirection()
    {
        if (direction == Vector3.right)
            direction = Vector3.forward;
        else if (direction == Vector3.forward)
            direction = Vector3.left;
        else if (direction == Vector3.left)
            direction = Vector3.back;
        else if (direction == Vector3.back)
            direction = Vector3.right;
    }
}