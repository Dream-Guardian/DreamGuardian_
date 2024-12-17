using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage;  // 진행도를 나타낼 UI 이미지
    public GameObject progressBarObject;  // 프로그래스바가 포함된 GameObject (비활성화/활성화 제어)
    public float maxValue = 100f;  // 최대 값
    public float currentValue;    // 현재 값
    private Transform parentObject;
    public float downOffSet = 0f;

    void Start()
    {
        // 게임 시작 시 프로그래스바 비활성화
        parentObject = transform.parent;
        progressBarObject.SetActive(false);
        UpdateProgressBar();
    }

    void Update()
    {
        // currentValue가 0이나 maxValue일 때만 비활성화
        //if (currentValue == 0f || currentValue >= maxValue)
        //{
        //    progressBarObject.SetActive(false);
        //}
        //else
        //{
        //    progressBarObject.SetActive(true);
        //}
        
    }

    public void plus(float value)
    {
        progressBarObject.SetActive(true);
        currentValue += value;
    }

    private void LateUpdate()
    {
        SetProgress(currentValue);
        transform.position = parentObject.position + new Vector3(0f, 0.6f - downOffSet, 0.6f - downOffSet);
        transform.forward = Camera.main.transform.forward;
    }

    // 외부에서 진행도를 설정하는 함수
    public void SetProgress(float progress)
    {
        currentValue = Mathf.Clamp(progress, 0, maxValue); // 진행도를 0과 maxValue 사이로 제한
        UpdateProgressBar();

        // 진행도가 0 또는 maxValue에 도달하면 프로그래스바를 비활성화
        //if (currentValue == 0f || currentValue >= maxValue)
        //{
        //    progressBarObject.SetActive(false);
        //}
        //else
        //{
        //    progressBarObject.SetActive(true); // 진행 중일 때는 프로그래스바를 활성화
        //}
    }

    void UpdateProgressBar()
    {
        // 프로그래스바의 fillAmount를 진행도에 맞춰 갱신
        if (fillImage != null)
        {
            fillImage.fillAmount = currentValue / maxValue;
        }
    }

    public void ResetProgress()
    {
        currentValue = 0f;
        SetProgress(currentValue);
        progressBarObject.SetActive(false);
    }

    public void toggleImages(bool value)
    {
        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image img in images) img.enabled = value;
    }
}
