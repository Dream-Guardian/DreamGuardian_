using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureScreenshot : MonoBehaviour
{
    public Camera captureCamera;
    public string screenshotFileName = "3DModelScreenshot.png";

    void Update()
    {
        // 'P' 키를 누르면 스크린샷 저장
        if (Input.GetKeyDown(KeyCode.P))
        {
            Capture();
        }
    }

    void Capture()
    {
        // 스크린샷을 저장할 파일 경로
        string filePath = Application.dataPath + "/" + screenshotFileName;

        // 카메라의 렌더 텍스처 설정
        RenderTexture rt = new RenderTexture(1024, 1024, 24);
        captureCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(1024, 1024, TextureFormat.RGB24, false);

        // 카메라로부터 렌더링된 이미지 가져오기
        captureCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0);
        screenShot.Apply();

        // 렌더 텍스처 초기화
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // PNG 파일로 저장
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Screenshot saved to: {filePath}");
    }
}
