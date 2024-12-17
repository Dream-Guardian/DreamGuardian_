using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarnBlink : MonoBehaviour
{
    public Image warnImage; // 깜빡일 이미지 (경고 이미지)
    public Image warnBackground;
    public float blinkSpeed; // 초기 깜빡임 속도 (시간 간격)
    public ProgressBar progressBar;

    public bool isBlinking = true;

    void OnEnable()
    {
        blinkSpeed = (progressBar.maxValue / 1.3f - progressBar.currentValue) / 100f + 0.1f;
        StartCoroutine(BlinkWarnImage());
    }

    IEnumerator BlinkWarnImage()
    {
        while (isBlinking)
        {
            // 이미지 보이게 (Alpha 1)
            SetImageAlpha(1f);
            yield return new WaitForSeconds(blinkSpeed);

            // 이미지 투명하게 (Alpha 0)
            SetImageAlpha(0f);
            yield return new WaitForSeconds(blinkSpeed);

            // 깜빡임 속도 증가 (속도를 빠르게)
            blinkSpeed = Mathf.Max((progressBar.maxValue / 1.3f - progressBar.currentValue) / 100f + 0.1f, 0.1f);
        }
    }

    // 이미지의 알파(투명도) 값을 설정하는 함수
    void SetImageAlpha(float alpha)
    {
        Color color1 = warnBackground.color;
        color1.a = alpha;
        warnBackground.color = color1;

        Color color2 = warnImage.color;
        color2.a = alpha;
        warnImage.color = color2;
    }
}