using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    public Character myCharacter;
    public TextMeshProUGUI speechBubbleText;
    public GameObject storyCanvas;
    public PhotonView StoroyPV;

    private string[] dialogues;
    private Coroutine displayDialogueCoroutine;
    private string sceneName;
    private bool skipClicked = false;

    // 각 Scene에 대한 대사 설정
    private Dictionary<string, string[]> sceneDialogues = new Dictionary<string, string[]>()
    {
        { "Stage 1-1", new string[] {
            "안녕 신입! 나는 꿈지기 로라야",
            "우리는 꿈지기. Dream Guardian이야",
            "조금 생소하지? 우리는 주인님이 부정적인 감정에 잠식되지 않게 막는 역할을 해",
            "앞으로 우리가 함께해야 할 일이지",
            "터렛 만드는 법은 튜토리얼에서 배웠겠지?",
            "혹시 까먹을 수 있으니까 간단하게 알려줄게. 이미 다 알고 있다면 ESC나 스킵 버튼을 눌러서 건너 뛰어도 괜찮아",
            "SpaceBar를 눌러서 물건을 집고 놓을 수 있어",
            "CTRL을 눌러서 나무를 자르거나, 모래를 점토로 만들 수 있고 터렛 부품들을 만들수도 있어",
            "조합 상자를 열어서 재료 손질법과 부품 제작법을 확인할 수 있으니까 참고하라구",
            "그럼 어서 터렛을 만들어서 감정괴물들에게서 주인님을 지켜줘!"
        }},
        { "Stage 1-2", new string[] {
            "안녕 신입! 조금 적응은 했어?",
            "처음 교육 시간 때 배웠겠지만 사용할 수 있는 좋은 기능들이 많아",
            "z 키를 눌러서 현재 시점을 변경할 수 있어",
            "ALT 키를 누르면 이동 방향으로 짧게 대쉬도 할 수 있지",
            "혼자서 게임을 진행한다면 SHIFT 키를 통해 캐릭터를 변경하면서 효육적으로 게임을 할 수 있지",
            "그럼 이만! 주인님을 지키러 가보자!",

        }},
        { "Stage 1-3", new string[] {
            "안녕 나는 꿈지기 로라야! 오늘 우리 주인님께서 회사에서 많이 혼나셨어.",
            "우리 주인님은 정말 열심히 일하시는데, 왜 상사들은 그걸 몰라줄까?",
            "잠시 후 분노 괴물들이 몰려올거야. 그들은 주인님의 화를 더욱 부추길 거야.",
            "터렛을 만들어서 주인님이 분노에 잠식되지 않게 도와줘!",
            "터렛은 분노 괴물들을 막아줄 거야. 주인님이 다시 웃을 수 있게 해줘!"
        }},
        { "Stage 2-1", new string[] {
            "안녕 신입! 2스테이지부터는 철을 사용할 수 있다는 거 알아?",
            "철을 사용법 알아? 철을 가마솥에 넣어서 녹이고 녹은 철을 작업대에 부어서 두드리면 단련 철이 돼",
            "단련 철을 이용해서 강화 받침대, 강화 몸통, 화염방사기, 얼음발사기를 만들 수 있다구!",
            "자세한 방법은 제작법을 확인해봐! 제작법 확인 정도는 이제 알아서 할 수 있겠지?",
            "오늘 밤도 주인님이 편안히 주무실 수 있게 힘내보자!"
        }},        
        { "Stage 2-2", new string[] {
            "안녕 신입! 2스테이지부터는 철을 사용할 수 있다는 거 알아?",
            "철을 사용법 알아? 철을 가마솥에 넣어서 녹이고 녹은 철을 작업대에 부어서 두드리면 단련 철이 돼",
            "단련 철을 이용해서 강화 받침대, 강화 몸통, 화염방사기, 얼음발사기를 만들 수 있다구!",
            "자세한 방법은 제작법을 확인해봐! 제작법 확인 정도는 이제 알아서 할 수 있겠지?",
            "오늘 밤도 주인님이 편안히 주무실 수 있게 힘내보자!"
        }},       
        { "Stage 2-3", new string[] {
            "안녕 신입! 2스테이지부터는 철을 사용할 수 있다는 거 알아?",
            "철을 사용법 알아? 철을 가마솥에 넣어서 녹이고 녹은 철을 작업대에 부어서 두드리면 단련 철이 돼",
            "단련 철을 이용해서 강화 받침대, 강화 몸통, 화염방사기, 얼음발사기를 만들 수 있다구!",
            "자세한 방법은 제작법을 확인해봐! 제작법 확인 정도는 이제 알아서 할 수 있겠지?",
            "오늘 밤도 주인님이 편안히 주무실 수 있게 힘내보자!"
        }},
        { "Stage 3-1", new string[] {
            "오늘은 주인님이 공포영화를 보셨어.",
            "주인님은 영화가 끝난 후에도 두려움에 떨고 있어...",
            "그래서 공포 괴물들이 들이닥칠거야. 그들은 주인님의 두려움을 키울 거야.",
            "주인님이 공포에 빠지지 않게 도와줘!",
            
        }},
        { "Stage 3-2", new string[] {
            "이번 스테이지는 혼자서 클리어하기 어려울 거야.",
            "아무리 너가 뛰어난 제작 능력이 있더라도 혼자서는 해내는 건 아주 어려울 거야.",
            "우리 같이 힘을 합쳐서 주인님이 감정을 잘 이겨낼 수 있도록 도와드리자.",
        }},
        { "Stage 3-3", new string[] {
            "오늘은 주인님이 공포영화를 보셨어.",
            "주인님은 영화가 끝난 후에도 두려움에 떨고 있어...",
            "그래서 공포 괴물들이 들이닥칠거야. 그들은 주인님의 두려움을 키울 거야.",
            "주인님이 공포에 빠지지 않게 도와줘!",
            "이번 스테이지에서는 새로운 무기와 방어 전략이 필요해.",
            "공포 괴물들은 더 빠르고 강력해. 신중하게 대응해야 해.",
            "터렛을 적절하게 배치하고, 주인님을 보호해줘.",
            "주인님이 공포를 이겨내고 다시 평온을 찾을 수 있게 도와줘."
        }},
        { "Final Stage 1", new string[] {
            "그거 알아? 이번 스테이지에는 랜덤박스가 있다구!",
            "랜덤 박스를 클릭하면 특정 확률로 터렛 부품들이 나오기도 하고 화염발사기도 나온다구!",
            "너무 많은 물건을 쌓아두면 정신 없으니까 필요없는 건 쓰레기통에 버리는 게 좋을거야",
            "이번 스테이지는 많이 어려울걸? 행운을 빌어",
        }},
        { "Final Stage 2", new string[] {
            "우리의 주인님은 어릴적부터 부모님의 사랑을 많이 받으시며 밝게 살아오셨던 분이야",
            "어느 날 주인님의 부모님께서 불의의 사고로 돌아가신 이후 부정적인 감정이 자꾸만 파고들고 있어",
            "주인님이 공포와 슬픔의 감정을 물리치고 평화를 찾으셨으면 좋겠어",
            "주인님이 트라우마를 이겨내고 부모님과의 행복한 기억을 잊지 않게 도와줘",
        }},
    };

    private void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;
        if (!sceneDialogues.ContainsKey(sceneName))
        {
            SkipStory();
            return;
        }
    }

    private void Update()
    {
        // esc로 스토리 스킵
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SkipStory();
        }
        else if (Input.GetKeyDown(KeyCode.Space)) // 스페이스로 한 줄 스킵
        {
            skipClicked = true;
        }
    }

    // 스토리 시작
    public void StoryStart()
    {
        SetDialogues();
        displayDialogueCoroutine = StartCoroutine(DisplayDialogue());
        storyCanvas.SetActive(true);
    }

    // 스테이지 별로 대사 설정
    void SetDialogues()
    {
        
        if (sceneDialogues.ContainsKey(sceneName))
        {
            dialogues = sceneDialogues[sceneName];
        }
        else
        {
            // 그냥 빈 배열로 초기화
            dialogues = new string[0];
            // 특정 스테이지 아니면 스킵
            SkipStory();
        }
    }

    // 대사 출력하기
    IEnumerator DisplayDialogue()
    {
        foreach (string dialogue in dialogues)
        {
            yield return StartCoroutine(TypeSentence(dialogue));
            yield return new WaitForSeconds(1f);
        }

        // 모든 대화가 끝난 후 storyCanvas 비활성화
        SkipStory();
    }

    // 단어를 0.05초에 하나씩 보여주기
    IEnumerator TypeSentence(string sentence)
    {
        speechBubbleText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            if (skipClicked)
            {
                skipClicked = false;
                break;
            }
            speechBubbleText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
        speechBubbleText.text = sentence;
    }

    // 스토리 스킵 메서드
    private void SkipStory()
    {
        // storyCanvas 비활성화
        storyCanvas.SetActive(false);
        if (myCharacter.statement == "STORY") myCharacter.statement = "IDLE";
        if (GameManager.instance.ChatPanel != null)
        {
            GameManager.instance.ChatPanel.SetActive(true);
        }
        GameManager.instance.isStoryEnd = true;
        this.enabled = false; // 스크립트 비활성화
    }
}
