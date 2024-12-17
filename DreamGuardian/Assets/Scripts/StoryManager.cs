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

    // �� Scene�� ���� ��� ����
    private Dictionary<string, string[]> sceneDialogues = new Dictionary<string, string[]>()
    {
        { "Stage 1-1", new string[] {
            "�ȳ� ����! ���� ������ �ζ��",
            "�츮�� ������. Dream Guardian�̾�",
            "���� ��������? �츮�� ���δ��� �������� ������ ��ĵ��� �ʰ� ���� ������ ��",
            "������ �츮�� �Բ��ؾ� �� ������",
            "�ͷ� ����� ���� Ʃ�丮�󿡼� �������?",
            "Ȥ�� ����� �� �����ϱ� �����ϰ� �˷��ٰ�. �̹� �� �˰� �ִٸ� ESC�� ��ŵ ��ư�� ������ �ǳ� �پ ������",
            "SpaceBar�� ������ ������ ���� ���� �� �־�",
            "CTRL�� ������ ������ �ڸ��ų�, �𷡸� ����� ���� �� �ְ� �ͷ� ��ǰ���� ������� �־�",
            "���� ���ڸ� ��� ��� �������� ��ǰ ���۹��� Ȯ���� �� �����ϱ� �����϶�",
            "�׷� � �ͷ��� ���� ���������鿡�Լ� ���δ��� ������!"
        }},
        { "Stage 1-2", new string[] {
            "�ȳ� ����! ���� ������ �߾�?",
            "ó�� ���� �ð� �� ��������� ����� �� �ִ� ���� ��ɵ��� ����",
            "z Ű�� ������ ���� ������ ������ �� �־�",
            "ALT Ű�� ������ �̵� �������� ª�� �뽬�� �� �� ����",
            "ȥ�ڼ� ������ �����Ѵٸ� SHIFT Ű�� ���� ĳ���͸� �����ϸ鼭 ȿ�������� ������ �� �� ����",
            "�׷� �̸�! ���δ��� ��Ű�� ������!",

        }},
        { "Stage 1-3", new string[] {
            "�ȳ� ���� ������ �ζ��! ���� �츮 ���δԲ��� ȸ�翡�� ���� ȥ���̾�.",
            "�츮 ���δ��� ���� ������ ���Ͻôµ�, �� ������ �װ� �����ٱ�?",
            "��� �� �г� �������� �����ðž�. �׵��� ���δ��� ȭ�� ���� ���߱� �ž�.",
            "�ͷ��� ���� ���δ��� �г뿡 ��ĵ��� �ʰ� ������!",
            "�ͷ��� �г� �������� ������ �ž�. ���δ��� �ٽ� ���� �� �ְ� ����!"
        }},
        { "Stage 2-1", new string[] {
            "�ȳ� ����! 2�����������ʹ� ö�� ����� �� �ִٴ� �� �˾�?",
            "ö�� ���� �˾�? ö�� �����ܿ� �־ ���̰� ���� ö�� �۾��뿡 �ξ �ε帮�� �ܷ� ö�� ��",
            "�ܷ� ö�� �̿��ؼ� ��ȭ ��ħ��, ��ȭ ����, ȭ������, �����߻�⸦ ���� �� �ִٱ�!",
            "�ڼ��� ����� ���۹��� Ȯ���غ�! ���۹� Ȯ�� ������ ���� �˾Ƽ� �� �� �ְ���?",
            "���� �㵵 ���δ��� ����� �ֹ��� �� �ְ� ��������!"
        }},        
        { "Stage 2-2", new string[] {
            "�ȳ� ����! 2�����������ʹ� ö�� ����� �� �ִٴ� �� �˾�?",
            "ö�� ���� �˾�? ö�� �����ܿ� �־ ���̰� ���� ö�� �۾��뿡 �ξ �ε帮�� �ܷ� ö�� ��",
            "�ܷ� ö�� �̿��ؼ� ��ȭ ��ħ��, ��ȭ ����, ȭ������, �����߻�⸦ ���� �� �ִٱ�!",
            "�ڼ��� ����� ���۹��� Ȯ���غ�! ���۹� Ȯ�� ������ ���� �˾Ƽ� �� �� �ְ���?",
            "���� �㵵 ���δ��� ����� �ֹ��� �� �ְ� ��������!"
        }},       
        { "Stage 2-3", new string[] {
            "�ȳ� ����! 2�����������ʹ� ö�� ����� �� �ִٴ� �� �˾�?",
            "ö�� ���� �˾�? ö�� �����ܿ� �־ ���̰� ���� ö�� �۾��뿡 �ξ �ε帮�� �ܷ� ö�� ��",
            "�ܷ� ö�� �̿��ؼ� ��ȭ ��ħ��, ��ȭ ����, ȭ������, �����߻�⸦ ���� �� �ִٱ�!",
            "�ڼ��� ����� ���۹��� Ȯ���غ�! ���۹� Ȯ�� ������ ���� �˾Ƽ� �� �� �ְ���?",
            "���� �㵵 ���δ��� ����� �ֹ��� �� �ְ� ��������!"
        }},
        { "Stage 3-1", new string[] {
            "������ ���δ��� ������ȭ�� ���̾�.",
            "���δ��� ��ȭ�� ���� �Ŀ��� �η��� ���� �־�...",
            "�׷��� ���� �������� ���̴�ĥ�ž�. �׵��� ���δ��� �η����� Ű�� �ž�.",
            "���δ��� ������ ������ �ʰ� ������!",
            
        }},
        { "Stage 3-2", new string[] {
            "�̹� ���������� ȥ�ڼ� Ŭ�����ϱ� ����� �ž�.",
            "�ƹ��� �ʰ� �پ ���� �ɷ��� �ִ��� ȥ�ڼ��� �س��� �� ���� ����� �ž�.",
            "�츮 ���� ���� ���ļ� ���δ��� ������ �� �̰ܳ� �� �ֵ��� ���͵帮��.",
        }},
        { "Stage 3-3", new string[] {
            "������ ���δ��� ������ȭ�� ���̾�.",
            "���δ��� ��ȭ�� ���� �Ŀ��� �η��� ���� �־�...",
            "�׷��� ���� �������� ���̴�ĥ�ž�. �׵��� ���δ��� �η����� Ű�� �ž�.",
            "���δ��� ������ ������ �ʰ� ������!",
            "�̹� �������������� ���ο� ����� ��� ������ �ʿ���.",
            "���� �������� �� ������ ������. �����ϰ� �����ؾ� ��.",
            "�ͷ��� �����ϰ� ��ġ�ϰ�, ���δ��� ��ȣ����.",
            "���δ��� ������ �̰ܳ��� �ٽ� ����� ã�� �� �ְ� ������."
        }},
        { "Final Stage 1", new string[] {
            "�װ� �˾�? �̹� ������������ �����ڽ��� �ִٱ�!",
            "���� �ڽ��� Ŭ���ϸ� Ư�� Ȯ���� �ͷ� ��ǰ���� �����⵵ �ϰ� ȭ���߻�⵵ ���´ٱ�!",
            "�ʹ� ���� ������ �׾Ƶθ� ���� �����ϱ� �ʿ���� �� �������뿡 ������ �� �����ž�",
            "�̹� ���������� ���� ������? ����� ����",
        }},
        { "Final Stage 2", new string[] {
            "�츮�� ���δ��� ������� �θ���� ����� ���� �����ø� ��� ��ƿ��̴� ���̾�",
            "��� �� ���δ��� �θ�Բ��� ������ ���� ���ư��� ���� �������� ������ �ڲٸ� �İ��� �־�",
            "���δ��� ������ ������ ������ ����ġ�� ��ȭ�� ã�������� ���ھ�",
            "���δ��� Ʈ��츶�� �̰ܳ��� �θ�԰��� �ູ�� ����� ���� �ʰ� ������",
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
        // esc�� ���丮 ��ŵ
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SkipStory();
        }
        else if (Input.GetKeyDown(KeyCode.Space)) // �����̽��� �� �� ��ŵ
        {
            skipClicked = true;
        }
    }

    // ���丮 ����
    public void StoryStart()
    {
        SetDialogues();
        displayDialogueCoroutine = StartCoroutine(DisplayDialogue());
        storyCanvas.SetActive(true);
    }

    // �������� ���� ��� ����
    void SetDialogues()
    {
        
        if (sceneDialogues.ContainsKey(sceneName))
        {
            dialogues = sceneDialogues[sceneName];
        }
        else
        {
            // �׳� �� �迭�� �ʱ�ȭ
            dialogues = new string[0];
            // Ư�� �������� �ƴϸ� ��ŵ
            SkipStory();
        }
    }

    // ��� ����ϱ�
    IEnumerator DisplayDialogue()
    {
        foreach (string dialogue in dialogues)
        {
            yield return StartCoroutine(TypeSentence(dialogue));
            yield return new WaitForSeconds(1f);
        }

        // ��� ��ȭ�� ���� �� storyCanvas ��Ȱ��ȭ
        SkipStory();
    }

    // �ܾ 0.05�ʿ� �ϳ��� �����ֱ�
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

    // ���丮 ��ŵ �޼���
    private void SkipStory()
    {
        // storyCanvas ��Ȱ��ȭ
        storyCanvas.SetActive(false);
        if (myCharacter.statement == "STORY") myCharacter.statement = "IDLE";
        if (GameManager.instance.ChatPanel != null)
        {
            GameManager.instance.ChatPanel.SetActive(true);
        }
        GameManager.instance.isStoryEnd = true;
        this.enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
    }
}
