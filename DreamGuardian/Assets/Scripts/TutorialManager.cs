using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI speechBubbleText;
    public GameObject speechBubbleCanvas;
    public GameObject dreamGuardianImage;
    public Sprite jaden;
    private int dialogueIndex = 0;
    public float typingSpeed = 0.03f;

    // 코루틴 실행 여부를 확인하는 플래그
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    // 게임 오브젝트 컨트롤
    public GameObject workTable;
    public GameObject craftTable;
    public GameObject woodBox;
    public GameObject sandBox;
    public GameObject mixer;
    public GameObject targetWall;
    public GameObject capsuleBox;
    public GameObject furnace;
    public GameObject mindStoneMine;
    public GameObject blueprintBoxItem;
    public GameObject blueprintBoxTurret;
    public GameObject trashCan;
    public GameObject mob1;
    public GameObject mob2;
    private GameObject Character;

    // 중복 실행을 방지하기 위한 플래그
    private bool isCoroutineRunning = false;
    private bool isSceneLoading = false;

    private String[] dialogues = {
        "안녕 나는 꿈지기 로라야!\n<sprite name=\"spacebar\"> 키를 눌러 나와 함께 대화를 진행해봐.",
        "우리는 오늘 하루 고생한 주인님을 위해\n부정적인 감정들로부터 주인님의 꿈을 지켜야해",
        "그럼 주인님의 상쾌한 아침을 위해 일을 시작해보자!",
        "방향키를 통해 움직이고,\n<sprite name=\"spacebar\"> 를 통해 물건을 집거나 내려놓을 수 있어",
        "먼저 나무상자에서 나무를 꺼내서 제작대에 올려봐",
        "이제 <sprite name=\"ctrl\"> 을 눌러서 가공해봐", // index 5
        "잘했어! 이번엔 모래를 믹서에서 가공해 점토로 만들어봐",
        "이제 만든 재료를 제작대에 모두 올려서\n터렛 하부를 만들어보자",
        "꽤 오래걸리지? 다른 꿈지기들과 함께 작업하면\n더 빨리 만들 수 있어",
        "이제 만든 터렛하부는 옆에 벽에 올려놓도록 해",
        "물건을 잘못 집었을 때 이렇게 벽에 내려놓아도 돼!", // 10
        "터렛본체는 가공된 나무 3개, 점토 1개로 조합할 수 있어\n얼른 본체도 만들어봐",
        "마지막으로 터렛 포신은 가공된 나무2개, 모래 1개로\n조합할 수 있어. 이번엔 점토가 아니라 모래니까 유의해!",
        "이제 만든 터렛을 이 벽에 순서대로 조립해 볼까?",
        "조합법이 기억나지 않을 땐 여기있는 도면 박스를 확인해봐",
        "조합법이 켜진상태에선 다시 한번 누르면 끌 수 있어!", // 15
        "바쁠때는 방항키+<sprite name=\"alt\"> 로 더 빠르게 이동해봐",
        "<sprite name=\"z\"> 를 눌러 감정 몬스터들이 오는 방향을 확인할 수 있어",
        "캡슐 충전은 제이든이 알려줄거야!",
        "신입이구나, 안녕 난 꿈지기 제이든이야\n이제부터 캡슐과 감정석에 대해 알려줄게",
        "우리는 주인님의 무의식에서 나온 감정을 발사체로 사용해\n부정적인 감정들을 물리치고있어", // 20
        "먼저 감정석을 녹여서 감정 정수를 만들어 보자",
        "감정석이 다 녹으면 캡슐을 꺼내서 채워봐!",
        "너무 늦으면 타버리니까 조심해!\n 감정석이 타면 쓰레기통에 버려야해",
        "꽉찬 캡슐을 완성된 터렛에 넣으면 발사준비 완료!",
        "감정 정수가 다 소진되면 다시 채워줘야해\n캡슐에 담아서 옮길 수도 있으니 참고해", // 25
        "마지막으로 혼자 플레이할때 영혼을 바꾸고 싶으면\nshift 키를 누르면 돼!",
        "벌써 감정 괴물들이 몰려오고있어!\n빨리 가서 주인님의 꿈을 지켜줘!"
    };

    // Start is called before the first frame update
    void Start()
    {
        mindStoneMine.SetActive(false);
        capsuleBox.SetActive(false);
        furnace.SetActive(false);
        trashCan.SetActive(false);
        mob1.SetActive(false);
        mob2.SetActive(false);

        // 코루틴을 통해 딜레이 후에 Character를 찾음
        StartCoroutine(FindCharacterWithDelay(0.5f));
        speechBubbleCanvas.SetActive(true);
        UpdateDialogue();
    }

    IEnumerator FindCharacterWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Character = GameObject.Find("Character(Clone)");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            // 타이핑 중이라면 즉시 텍스트를 완성시키고, 아니면 다음 대화로 넘어감
            if (isTyping)
            {
                CompleteTyping();
            }
            else
            {
                CheckAndProceedDialogue();
            }
        }
    }

    // 말풍선 텍스트를 타이핑 효과로 업데이트하는 함수
    void UpdateDialogue()
    {
        if (dialogueIndex >= dialogues.Length)
        {
            // 대화 인덱스가 배열의 길이를 넘으면 종료
            return;
        }
        // 대화 내용에 따른 화살표 처리
        ActivateHintForCurrentDialogue();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(dialogues[dialogueIndex]));
    }

    // 타이핑 효과 코루틴
    IEnumerator TypeText(string dialogue)
    {
        isTyping = true;
        speechBubbleText.text = "";
        int i = 0;

        while (i < dialogue.Length)
        {
            // 태그 처리
            if (dialogue[i] == '<')
            {
                int closingTagIndex = dialogue.IndexOf('>', i);
                if (closingTagIndex != -1)
                {
                    speechBubbleText.text += dialogue.Substring(i, closingTagIndex - i + 1);
                    i = closingTagIndex + 1;
                }
                else
                {
                    speechBubbleText.text += dialogue[i];
                    i++;
                }
            }
            else
            {
                // 일반 문자는 한 글자씩 추가
                speechBubbleText.text += dialogue[i];
                i++;
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;  // 타이핑 종료
    }

    // 타이핑을 즉시 완료하는 함수
    void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 현재 대화 텍스트를 한 번에 출력
        speechBubbleText.text = dialogues[dialogueIndex];
        isTyping = false;
    }

    // 현재 대화에 맞는 화살표 활성화
    void ActivateHintForCurrentDialogue()
    {
        switch (dialogueIndex)
        {
            case 4: // "먼저 나무상자에서 나무를 꺼내서 제작대에 올려봐"
                woodBox.transform.Find("Arrow").gameObject.SetActive(true);
                ActivateHighlightCubes(woodBox);
                break;

            case 6: // "잘했어! 이번엔 모래를 믹서에서 가공해 점토로 만들어봐",
                sandBox.transform.Find("Arrow").gameObject.SetActive(true);
                ActivateHighlightCubes(sandBox);
                break;

            case 7: // "이제 만든 재료를 제작대에 모두 올려서\n터렛 하부를 만들어보자"
                craftTable.transform.Find("Arrow").gameObject.SetActive(true);
                craftTable.transform.Find("BPCanvas/pedestal1").gameObject.SetActive(true);
                ActivateHighlightCubes(craftTable);
                break;

            case 9: // "이제 만든 터렛하부는 옆에 벽에 올려놓도록 해"
                targetWall.transform.Find("Arrow").gameObject.SetActive(true);
                ActivateHighlightCubes(targetWall);
                break;

            case 11:
                craftTable.transform.Find("BPCanvas/body1").gameObject.SetActive(true);
                break;

            case 12:
                craftTable.transform.Find("BPCanvas/gun1").gameObject.SetActive(true);
                break;
            
            case 13:
                targetWall.transform.Find("Arrow").gameObject.SetActive(true);
                break;
            
            case 14: // "조합법이 기억나지 않을 땐 여기있는 도면 박스를 확인해봐"
                targetWall.transform.Find("Arrow").gameObject.SetActive(false);
                blueprintBoxTurret.transform.Find("Arrow").gameObject.SetActive(true);
                break;
            
            case 16:
                blueprintBoxTurret.transform.Find("Arrow").gameObject.SetActive(false);
                break;

            case 19: // "신입이구나, 안녕 난 꿈지기 제이든이야\n이제부터 캡슐과 감정석에 대해 알려줄게"
                dreamGuardianImage.GetComponent<UnityEngine.UI.Image>().sprite = jaden;

                // Y축 스케일을 1.1로 변경 (X와 Z는 기존 값 유지)
                RectTransform rectTransform = dreamGuardianImage.GetComponent<RectTransform>();
                rectTransform.localScale = new Vector3(rectTransform.localScale.x, 1.1f, rectTransform.localScale.z);
                
                workTable.SetActive(false);
                craftTable.SetActive(false);
                woodBox.SetActive(false);
                sandBox.SetActive(false);
                mixer.SetActive(false);
                blueprintBoxTurret.SetActive(false);
                mindStoneMine.SetActive(true);
                capsuleBox.SetActive(true);
                furnace.SetActive(true);
                trashCan.SetActive(true);
                break;

            case 21:
                mindStoneMine.transform.Find("Arrow").gameObject.SetActive(true);
                ActivateHighlightCubes(mindStoneMine);
                break;

            case 22:
                capsuleBox.transform.Find("Arrow").gameObject.SetActive(true);
                ActivateHighlightCubes(capsuleBox);
                break;
            
            case 24:
                targetWall.transform.Find("Arrow").gameObject.SetActive(true);
                mob1.SetActive(true);
                mob2.SetActive(true);
                break;
            
            default:
                break;
        }
    }
    void ActivateHighlightCubes(GameObject woodBox)
{
    // woodBox 하위에 있는 모든 MeshRenderer 컴포넌트를 가져옵니다.
    MeshRenderer[] meshRenderers = woodBox.GetComponentsInChildren<MeshRenderer>();

    foreach (MeshRenderer meshRenderer in meshRenderers)
    {
        // 이름이 "HighlightCube"인 오브젝트만 활성화
        if (meshRenderer.gameObject.name == "HighlightCube")
        {
            meshRenderer.enabled = true;  // MeshRenderer 활성화
        }
    }
}

    // 오브젝트가 테이블에 추가되었는지 확인하는 코루틴
    IEnumerator CheckForAllItems(GameObject table, string[] itemNames, GameObject canvas = null)
    {
        // 코루틴이 이미 실행 중이라면 중복 실행을 막음
        if (isCoroutineRunning) yield break;

        isCoroutineRunning = true;  // 코루틴 실행 플래그 설정

        while (true)
        {
            bool allItemsPresent = true;
            // 모든 아이템이 있는지 확인
            foreach (string itemName in itemNames)
            {
                Debug.Log(itemName);
                if (table.transform.Find($"{itemName}(Clone)") == null)
                {
                    allItemsPresent = false;
                    break;
                }
            }

            if (allItemsPresent)
            {
                dialogueIndex++;
                UpdateDialogue();
                isCoroutineRunning = false;
                Transform arrow = table.transform.Find("Arrow");
                if (arrow != null && arrow.gameObject.activeSelf)
                {
                    arrow.gameObject.SetActive(false);
                }
                if (canvas)
                {
                    canvas.SetActive(false);
                }
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }

        isCoroutineRunning = false;  // 코루틴이 끝나면 플래그 해제
    }

    // 아이템을 들었는지 확인하는 코루틴
    IEnumerator CheckCharacterHoldingItem(string itemName, GameObject sourceBox, GameObject targetTable)
    {
        if (isCoroutineRunning) yield break;
        isCoroutineRunning = true;
        
        while (true)
        {
            // 캐릭터가 아이템을 들고 있는지 확인
            if (Character.transform.Find($"{itemName}(Clone)"))
            {
                // 화살표를 비활성화하고 다음 단계를 활성화
                sourceBox.transform.Find("Arrow").gameObject.SetActive(false);
                targetTable.transform.Find("Arrow").gameObject.SetActive(true);
                break;  // 확인이 완료되면 코루틴을 종료
            }
            yield return new WaitForSeconds(0.1f);  // 0.2초마다 확인
        }

        isCoroutineRunning = false;
    }

    IEnumerator CheckCapsuleFillAmount(string itemName, GameObject sourceBox)
    {
        if (isCoroutineRunning) yield break; // 중복 실행 방지
        isCoroutineRunning = true;

        bool isFillAmountZeroChecked = false; // fillAmount 0 체크 여부 플래그

        while (true)
        {
            // 캐릭터가 캡슐을 들고 있는지 확인
            Transform capsule = Character.transform.Find($"{itemName}(Clone)");

            if (capsule != null)
            {
                // 캡슐 내부의 정확한 경로로 ProgressBar의 Image 컴포넌트를 찾음
                Transform progressTransform = capsule.Find("Canvas/ProgressBar/Progress");

                if (progressTransform != null)
                {
                    Image progressBarImage = progressTransform.GetComponent<Image>();

                    if (progressBarImage != null)
                    {
                        // fillAmount가 0일 때 처리
                        if (progressBarImage.fillAmount == 0 && !isFillAmountZeroChecked)
                        {
                            sourceBox.transform.Find("Arrow").gameObject.SetActive(false);
                            furnace.transform.Find("Arrow").gameObject.SetActive(true);
                            ActivateHighlightCubes(furnace);
                            isFillAmountZeroChecked = true; // 한 번만 실행되도록 플래그 설정
                        }

                        // fillAmount가 1일 때 처리
                        if (progressBarImage.fillAmount == 1)
                        {
                            furnace.transform.Find("Arrow").gameObject.SetActive(false);
                            
                            dialogueIndex++;
                            UpdateDialogue();
                            isCoroutineRunning = false; // 코루틴 종료
                            yield break; // 더 이상 반복하지 않음
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.1f); // 0.1초마다 상태 확인
        }
    }

    // 블루프린트 박스의 Canvas가 특정 상태(isActive)에 있는지 확인하는 코루틴
    IEnumerator CheckBlueprintBoxCanvas(GameObject blueprintBox, bool isActive)
    {
        // 코루틴이 이미 실행 중이라면 중복 실행을 막음
        if (isCoroutineRunning) yield break;

        isCoroutineRunning = true;  // 코루틴 실행 플래그 설정

        while (true)
        {
            // "recipe" Canvas 상태를 확인
            Transform canvasTransform = blueprintBox.transform.Find("recipe");
            if (canvasTransform != null && canvasTransform.gameObject.activeSelf == isActive)
            {
                // 원하는 상태에 도달하면 다음 대화로 진행
                dialogueIndex++;
                UpdateDialogue();

                isCoroutineRunning = false;
                break;
            }

            // 0.1초마다 확인
            yield return new WaitForSeconds(0.1f);
        }

        isCoroutineRunning = false;
    }


    // 대화 진행을 제어하는 함수
    void CheckAndProceedDialogue()
    {
        if (dialogueIndex >= dialogues.Length - 1 && !isSceneLoading)
        {
            isSceneLoading = true;
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("Lobby2");
            return;
        }

        switch (dialogueIndex)
        {
            case 4: // 나무를 제작대에 놓는 확인 (workTable, "Wood")
                if (!isCoroutineRunning)
                {
                    StartCoroutine(CheckCharacterHoldingItem("Wood", woodBox, workTable));
                    string[] requiredItems = { "Wood" };
                    StartCoroutine(CheckForAllItems(workTable, requiredItems));
                }
                break;

            case 5: // 나무가 가공되었는지 확인 (workTable, "WoodenPlank")
                if (!isCoroutineRunning)
                {
                    string[] requiredItems = { "WoodenPlank" };
                    StartCoroutine(CheckForAllItems(workTable, requiredItems));
                }
                break;

            case 6: // 점토가 믹서에 만들어졌는지 확인 (mixer, "Clay")
                if (!isCoroutineRunning)
                {
                    StartCoroutine(CheckCharacterHoldingItem("Sand", sandBox, mixer));
                    string[] requiredItems = { "Clay" };
                    StartCoroutine(CheckForAllItems(mixer, requiredItems));
                }
                break;
            case 7: // 터렛의 받침대(Pedestal)가 제작대에 있는지 확인 (craftTable, "Pedestal")
                if (!isCoroutineRunning)
                {
                    string[] requiredItems = { "Pedestal_basic" };
                    StartCoroutine(CheckForAllItems(craftTable, requiredItems, craftTable.transform.Find("BPCanvas/pedestal1").gameObject));
                }
                break;
            case 9: // 터렛하부를 벽에 올렸는지 확인
                if (!isCoroutineRunning)
                {
                    string[] requiredItems = { "Pedestal_basic" };
                    StartCoroutine(CheckForAllItems(targetWall, requiredItems));
                }
                break;
            case 11: // 터렛 본체가 제작대에 있는지 확인
                if (!isCoroutineRunning)
                {
                    string[] requiredItems = { "Body_basic" };
                    StartCoroutine(CheckForAllItems(craftTable, requiredItems, craftTable.transform.Find("BPCanvas/body1").gameObject));
                }
                break;
            case 12: // 터렛 포신이 제작대에 있는지 확인
                if (!isCoroutineRunning)
                {
                    string[] requiredItems = { "Gun_basic" };
                    StartCoroutine(CheckForAllItems(craftTable, requiredItems, craftTable.transform.Find("BPCanvas/gun1").gameObject));
                }
                break;
            case 13: // 터렛을 모두 타겟 벽에 올렸는지 확인
                if (!isCoroutineRunning)
                {
                    string[] requiredItems = { "Pedestal_basic", "Gun_basic", "Body_basic" };
                    StartCoroutine(CheckForAllItems(targetWall, requiredItems));
                }
                break;
            
            case 14:
                if (!isCoroutineRunning)
                {
                    StartCoroutine(CheckBlueprintBoxCanvas(blueprintBoxTurret, true));
                }
                break;

            case 15:
                if (!isCoroutineRunning)
                    {
                        StartCoroutine(CheckBlueprintBoxCanvas(blueprintBoxTurret, false));
                    }
                break;
            
            case 21:
                if (!isCoroutineRunning)
                {
                    StartCoroutine(CheckCharacterHoldingItem("MindStone", mindStoneMine, furnace));
                    string[] requiredItems = { "Pot(Clone)/MeltedMindStone" };
                    StartCoroutine(CheckForAllItems(furnace, requiredItems));
                }
                break;
            
            case 22:
                if (!isCoroutineRunning)
                {
                    StartCoroutine(CheckCapsuleFillAmount("Capsule", capsuleBox));
                }
                break;
            
            case 24:
                if (!isCoroutineRunning)
                {
                    string[] requiredItems = { "Pedestal_basic", "Gun_basic", "Body_basic", "Capsule" };
                    StartCoroutine(CheckForAllItems(targetWall, requiredItems));
                }
                break;
                
            default:
                    dialogueIndex++;
                    UpdateDialogue();
                break;
        }
    }
}
