using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera mainCamera;  // Main Camera를 연결할 변수

    public float transitionSpeed = 10.0f;  // 카메라 전환 속도
    private Vector3 targetPosition;       // 목표 위치
    private Quaternion targetRotation;    // 목표 회전

    // 첫 번째와 두 번째 카메라 위치 및 회전값을 Inspector에서 관리할 수 있게 public으로 설정
    public Vector3 position1 = new Vector3(0, 12, -4);
    public Quaternion rotation1 = Quaternion.Euler(70, 0, 0);
    public Vector3 position2 = new Vector3(0, 22, -2);
    public Quaternion rotation2 = Quaternion.Euler(85, 0, 0);

    private bool isAtPosition1 = true;    // 현재 위치가 position1인지 여부
    GameManager gameManager;

    public KeyCode ZoomKey;

    void Start()
    {
        // 메인 카메라를 첫 번째 위치로 설정
        mainCamera.transform.position = position1;
        mainCamera.transform.rotation = rotation1;

        // 첫 번째 위치를 목표로 설정
        targetPosition = position1;
        targetRotation = rotation1;
        gameManager = GameManager.instance;
        ZoomKey = (KeyCode)PlayerPrefs.GetInt("ZoomKey", (int)KeyCode.Z);
    }

    void Update()
    {
        // Z 키를 누르면 카메라 전환 시작
        if (!(gameManager.isChatFocused) && Input.GetKeyDown(ZoomKey))
        {
            StartSwitching();
        }

        // 카메라 전환 중일 때 부드럽게 이동
        SmoothTransition();
    }

    // 카메라 전환 시작
    void StartSwitching()
    {
        // 목표 위치와 회전 설정
        if (isAtPosition1)
        {
            targetPosition = position2;
            targetRotation = rotation2;
        }
        else
        {
            targetPosition = position1;
            targetRotation = rotation1;
        }

        // 현재 위치 상태를 반전
        isAtPosition1 = !isAtPosition1;
    }

    // 부드러운 카메라 전환 처리
    void SmoothTransition()
    {
        // 카메라의 위치와 회전을 목표 위치로 부드럽게 이동
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * transitionSpeed);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);
    }
}
