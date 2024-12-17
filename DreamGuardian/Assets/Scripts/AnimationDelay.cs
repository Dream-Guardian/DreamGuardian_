using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDelay : MonoBehaviour
{
    public Animator animator;  // 애니메이터 컴포넌트
    public float delayTime = 2.0f;  // 애니메이션 시작 지연 시간

    void Start()
    {
        // 처음에는 Animator를 비활성화
        animator.enabled = false;

        // 딜레이 후 Animator 활성화
        StartCoroutine(ActivateAnimatorWithDelay());
    }

    IEnumerator ActivateAnimatorWithDelay()
    {
        // 딜레이 시간 대기
        yield return new WaitForSeconds(delayTime);

        // Animator 활성화
        animator.enabled = true;
    }
}
