using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintButtonController : MonoBehaviour
{
    public GameObject hintPanel;
    public Button hintButton;
    public Button closeButton;

    void Start()
    {
        // 처음에는 힌트 버튼이 보이고, 패널은 숨겨진 상태
        hintPanel.SetActive(false);
        hintButton.gameObject.SetActive(true);

        // 버튼 클릭 이벤트 설정
        hintButton.onClick.AddListener(ShowHintPanel);
        closeButton.onClick.AddListener(CloseHintPanel);
    }

    void ShowHintPanel()
    {
        hintPanel.SetActive(true);
        hintButton.gameObject.SetActive(false);
    }

    void CloseHintPanel()
    {
        hintPanel.SetActive(false);
        hintButton.gameObject.SetActive(true);
    }
}
