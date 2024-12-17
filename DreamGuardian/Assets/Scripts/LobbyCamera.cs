using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LobbyCamera : MonoBehaviour
{
    public Camera mainCamera;  // Main Camera를 연결할 변수
    public CanvasGroup uiCanvasGroup;  // UI의 투명도를 조절할 CanvasGroup
    public float transitionSpeed = 0.5f;  // 카메라 전환 속도
    public float fadeSpeed = 0.5f;  // UI가 나타나는 속도
    private Vector3 targetPosition;       // 목표 위치
    private Quaternion targetRotation;    // 목표 회전
    private bool isLoading = false;

    private Vector3 stagePosition;        // 스테이지 화면 이동

    // 첫 번째와 두 번째 카메라 위치 및 회전값을 Inspector에서 관리할 수 있게 public으로 설정
    public Vector3 position1 = new Vector3(0, 20, -23);
    public Quaternion rotation1 = Quaternion.Euler(80, 0, 0);
    public Vector3 position2 = new Vector3(0, 5, -3);
    public Quaternion rotation2 = Quaternion.Euler(38, 0, 0);

    // 스테이지2, 스테이지3, final스테이지 카메라 위치(스테이지1 = position2)
    public Vector3 position4 = new Vector3(53, 5, -3);
    public Vector3 position5 = new Vector3(102, 5, -3);
    public Vector3 position6 = new Vector3(153, 5, -3);

    // NetworkManager 에서 nowCheckedStage 가져와봅시다
    private NetworkManager networkManager;

    private bool isFirstTime = true;


    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();

        if (PhotonNetwork.IsConnected)
        {
            // 메인 카메라를 두 번째 위치로 설정
            mainCamera.transform.position = position2;
            mainCamera.transform.rotation = rotation2;

            isFirstTime = false;

        }
        else
        {
            // 메인 카메라를 첫 번째 위치로 설정
            mainCamera.transform.position = position1;
            mainCamera.transform.rotation = rotation1;

            uiCanvasGroup.alpha = 0f;  // UI 숨김
            // 카메라 전환을 바로 시작
            StartSwitching();

        }


    }

    void Update()
    {
        if (isFirstTime)
        {
            if (PlayerPrefs.GetString("checkedStage") == null)
            {

                mainCamera.transform.position = position2;
                //stagePosition = position2;
                //StageTransition();
            }
            else if (PlayerPrefs.GetString("checkedStage") == "2")
            {
                mainCamera.transform.position = position2;
            }
            else if (PlayerPrefs.GetString("checkedStage") == "Stage H")
            {
                mainCamera.transform.position = position6;
            }
            else if (PlayerPrefs.GetString("checkedStage").Contains("Final"))
            {
                mainCamera.transform.position = position6;
            }
            else if (PlayerPrefs.GetString("checkedStage").Contains("Stage 1"))
            {
                mainCamera.transform.position = position2;
                //stagePosition = position2;
                //StageTransition();
            }
            else if (PlayerPrefs.GetString("checkedStage").Contains("Stage 2"))
            {
                mainCamera.transform.position = position4;
                //StageTransition();
            }
            else if (PlayerPrefs.GetString("checkedStage").Contains("Stage 3"))
            {
                mainCamera.transform.position = position5;
            }
            else
            {
                SmoothTransition();
            }
        }
        else
        {
            if (PlayerPrefs.GetString("checkedStage") == null)
            {

                mainCamera.transform.position = position2;
                //stagePosition = position2;
                //StageTransition();
            }
            else if (PlayerPrefs.GetString("checkedStage") == "2")
            {
                mainCamera.transform.position = position2;
            }
            else if (PlayerPrefs.GetString("checkedStage")== "Stage H")
            {
                mainCamera.transform.position = position6;
            }
            else if (PlayerPrefs.GetString("checkedStage").Contains("Final"))
            {
                mainCamera.transform.position = position6;
            }
            else if (PlayerPrefs.GetString("checkedStage").Contains("Stage 1"))
            {
                mainCamera.transform.position = position2;
                //stagePosition = position2;
                //StageTransition();
            }
            else if (PlayerPrefs.GetString("checkedStage").Contains("Stage 2"))
            {
                mainCamera.transform.position = position4;
                //StageTransition();
            }
            else if (PlayerPrefs.GetString("checkedStage").Contains("Stage 3"))
            {
                mainCamera.transform.position = position5;
            }
            

        }

    }

    // 카메라 전환 시작
    void StartSwitching()
    {
        // 목표 위치와 회전 설정 (첫 위치에서 두 번째 위치로만 이동)
        targetPosition = position2;
        targetRotation = rotation2;
    }

    // 부드러운 카메라 전환 처리
    void SmoothTransition()
    {
        // 카메라의 위치와 회전을 목표 위치로 부드럽게 이동
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * transitionSpeed);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);

        // 목표 위치에 거의 도달하면 캔버스 로딩
        if (Vector3.Distance(mainCamera.transform.position, targetPosition) < 3.0f)
        {
            if (!isLoading)
            {
                MusicManager.Startsound();
                isLoading = true;
            }
            uiCanvasGroup.alpha = Mathf.Lerp(uiCanvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
        }
    }

    // 스테이지 이동 시 움직이는 카메라 로직
    void StageTransition()
    {

        // 목표 위치와의 거리가 0.1 이하로 작아지면, 목표 위치에 강제로 고정
        if (Vector3.Distance(mainCamera.transform.position, stagePosition) < 0.1f)
        {
            mainCamera.transform.position = stagePosition;
        }
        else
        {
            // 목표 위치로 부드럽게 이동
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, stagePosition, Time.deltaTime * 1f);
        }

    }
}
