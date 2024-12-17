using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBullet : Bullet
{
    protected override void OnCollisionEnter(Collision collision)
    {
        DisableBullet();
        if (collision.gameObject.CompareTag("Mob"))
        {
            Mob mob = collision.gameObject.GetComponent<Mob>();
            if (mob != null)
            {
                mob.TakeDamage(damage, "frozen");
            }
        }
        else if (collision.gameObject.CompareTag("Boss"))
        {
            Boss boss = collision.gameObject.GetComponent<Boss>();
            if (boss != null)
            {
                //float chance = Random.value; // 0.0에서 1.0 사이의 무작위 값 생성

                //if (chance <= 0.07f) // 7% 확률
                //{
                //    boss.TakeDamage(damage, "frozen");
                //}
                //else // 나머지 확률
                //{
                //    boss.TakeDamage(damage);
                //}
                boss.TakeDamage(damage, "frozen");
            }
        }
        MusicManager.Enemy();
        return;
    }
}
