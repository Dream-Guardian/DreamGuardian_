using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOnObject : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // 플레이어가 움직이는 오브젝트에 올라갔을 때
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            // 부모를 움직이는 오브젝트로 설정
            transform.parent = collision.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // 플레이어가 움직이는 오브젝트에서 벗어났을 때
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            // 부모 관계 해제 (루트 오브젝트로 설정)
            transform.parent = null;
        }
    }
}
