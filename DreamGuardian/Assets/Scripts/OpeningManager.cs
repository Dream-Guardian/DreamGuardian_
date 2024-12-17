using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningManager : MonoBehaviour
{
    public GameObject canvas;

    public CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShowCanvasAndChangeScene());
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
    }

    IEnumerator ShowCanvasAndChangeScene()
    {
        // ���
        yield return new WaitForSeconds(0.5f);

        // ������ ��Ÿ���� ȿ�� (���̵� ��)
        float duration = 1f;  // ���̵� �ο� �ɸ��� �ð� (1��)
        float currentTime = 0f;

        // ���� ���� 0���� 1�� ������ ������Ű�� ���̵� �� ȿ���� ��
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, currentTime / duration);  // ���İ��� 0���� 1�� ����
            yield return null;  // �� ������ ���
        }

        // ���������� ���İ��� 1�� ����
        canvasGroup.alpha = 1f;

        // �Ǵٽ� ���
        yield return new WaitForSeconds(0.5f);

        // �ٸ� ������ ��ȯ (Lobby2 ������ ��ȯ)
        SceneManager.LoadScene("Lobby2");
    }

}
