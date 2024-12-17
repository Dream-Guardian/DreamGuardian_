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
        // 대기
        yield return new WaitForSeconds(0.5f);

        // 서서히 나타나는 효과 (페이드 인)
        float duration = 1f;  // 페이드 인에 걸리는 시간 (1초)
        float currentTime = 0f;

        // 알파 값을 0에서 1로 서서히 증가시키며 페이드 인 효과를 줌
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, currentTime / duration);  // 알파값을 0에서 1로 증가
            yield return null;  // 한 프레임 대기
        }

        // 최종적으로 알파값을 1로 설정
        canvasGroup.alpha = 1f;

        // 또다시 대기
        yield return new WaitForSeconds(0.5f);

        // 다른 씬으로 전환 (Lobby2 씬으로 전환)
        SceneManager.LoadScene("Lobby2");
    }

}
