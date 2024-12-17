using System.Collections;
using UnityEngine;

public class ShootingCamera : MonoBehaviour
{
    public Transform[] checkpoints; // 체크포인트 Transform 배열
    public int startingCheckpointIndex = 0; // 시작 체크포인트 인덱스
    public float transitionSpeed = 2.0f; // 카메라 이동 속도
    public float waitTimeAtCheckpoint = 0.0f; // 체크포인트 대기 시간 (0초로 설정 가능)

    private int currentCheckpointIndex; // 현재 체크포인트 인덱스

    void Start()
    {
        // 체크포인트 배열이 비어있지 않은지 확인
        if (checkpoints.Length == 0)
        {
            Debug.LogError("최소 1개의 체크포인트가 필요합니다.");
            return;
        }

        // 시작 체크포인트 인덱스 유효성 확인
        if (startingCheckpointIndex < 0 || startingCheckpointIndex >= checkpoints.Length)
        {
            Debug.LogError("시작 체크포인트 인덱스가 유효하지 않습니다.");
            return;
        }

        // 첫 번째 체크포인트로 카메라 위치 및 회전 설정
        currentCheckpointIndex = startingCheckpointIndex;
        transform.position = checkpoints[currentCheckpointIndex].position;
        transform.rotation = checkpoints[currentCheckpointIndex].rotation;

        // 전환 시작
        StartCoroutine(TransitionToNextCheckpoint());
    }

    IEnumerator TransitionToNextCheckpoint()
    {
        // 무한 루프를 통해 체크포인트 사이를 계속 순환
        while (true)
        {
            // 다음 체크포인트로 전환
            int previousCheckpointIndex = currentCheckpointIndex;
            currentCheckpointIndex = (currentCheckpointIndex + 1) % checkpoints.Length;

            // 현재 위치에서 다음 체크포인트까지 부드럽게 이동
            Transform nextCheckpoint = checkpoints[currentCheckpointIndex];
            yield return StartCoroutine(SmoothTransition(nextCheckpoint));

            // 체크포인트에서 대기 시간 적용
            if (waitTimeAtCheckpoint > 0.0f)
            {
                yield return new WaitForSeconds(waitTimeAtCheckpoint);
            }

            // 2번 인덱스에서 3번 인덱스로 이동할 때 1초 추가 대기
            if (previousCheckpointIndex == 2)
            {
                yield return new WaitForSeconds(3.0f); // 1초 추가 대기
            }
        }
    }

    IEnumerator SmoothTransition(Transform targetCheckpoint)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 targetPosition = targetCheckpoint.position;
        Quaternion targetRotation = targetCheckpoint.rotation;

        float elapsedTime = 0f;
        float distanceToTarget = Vector3.Distance(startPosition, targetPosition);
        float transitionDuration = distanceToTarget / transitionSpeed;

        while (elapsedTime < transitionDuration)
        {
            // 진행률을 계산 (0에서 1까지)
            float t = elapsedTime / transitionDuration;

            // Ease-in, Ease-out 처리 (처음과 끝에서 느리고 중간에서 빠름)
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            // 위치를 부드럽게 전환
            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothStep);

            // 회전도 부드럽게 전환 (Slerp 사용)
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothStep);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 목표 위치와 회전에 정확히 맞추기
        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }
}
