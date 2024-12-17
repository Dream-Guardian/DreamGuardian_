using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WallRise : MonoBehaviour
{
    public Animator animator;  // 애니메이터 컴포넌트
    public float delayTime = 2.0f;  // 애니메이션 시작 지연 시간
    public Camera mainCamera;  // 흔들릴 카메라
    public float shakeDuration = 1.0f;  // 카메라 흔들림 지속 시간
    public float shakeMagnitude = 0.1f;  // 카메라 흔들림 강도

    private Rigidbody[] rigidbodies;  // 모든 하위 오브젝트의 Rigidbody 배열
    private Vector3 originalCameraPosition;  // 카메라의 원래 위치

    void Start()
    {
        // 카메라의 원래 위치 저장
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
        }

        // 하위 오브젝트에 있는 모든 Rigidbody를 가져옴
        rigidbodies = GetComponentsInChildren<Rigidbody>();

        // 모든 Rigidbody의 중력을 비활성화
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }

        // 처음에는 Animator를 비활성화
        animator.enabled = false;

        // 딜레이 후 Animator 활성화 및 애니메이션 재생
        StartCoroutine(ActivateAnimatorWithDelay());
    }

    IEnumerator ActivateAnimatorWithDelay()
    {
        // 딜레이 시간 대기
        yield return new WaitForSeconds(delayTime);

        // 카메라 흔들림 시작
        if (mainCamera != null)
        {
            StartCoroutine(CameraShake());
        }

        // Animator 활성화
        animator.enabled = true;

        // 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        yield return new WaitForSeconds(2.0f);

        // 애니메이션이 끝난 후 모든 Rigidbody의 중력을 다시 활성화
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
    }

    IEnumerator CameraShake()
    {
        yield return new WaitForSeconds(0.5f);

        float elapsed = 0.0f;

        // 카메라 흔들리는 시간 동안 반복
        while (elapsed < shakeDuration)
        {
            // 무작위로 카메라 위치를 약간 변경
            Vector3 randomPoint = originalCameraPosition + Random.insideUnitSphere * shakeMagnitude;
            mainCamera.transform.localPosition = new Vector3(randomPoint.x, originalCameraPosition.y, randomPoint.z);

            elapsed += Time.deltaTime;

            // 프레임마다 다음 위치로 갱신
            yield return null;
        }

        // 카메라 위치를 원래대로 복구
        mainCamera.transform.localPosition = originalCameraPosition;
    }
}
