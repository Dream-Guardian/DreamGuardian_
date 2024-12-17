using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    public AudioMixer masterMixer;
    public Slider audioSlider;
    public Toggle muteToggle;  // 음소거 토글 UI 요소
    private string mixType;
    private float lastVolume;  // 마지막으로 설정된 볼륨 값

    void Start()
    {
        mixType = audioSlider.name;
        lastVolume = audioSlider.value;

        // 토글 이벤트 리스너 추가
        muteToggle.onValueChanged.AddListener(ToggleAudioVolume);
    }

    public void AudioControl()
    {
        float sound = audioSlider.value;
        if (sound == -40f) masterMixer.SetFloat(mixType, -80);
        else masterMixer.SetFloat(mixType, sound);

        lastVolume = sound;

        // 볼륨이 변경되면 음소거 해제
        if (muteToggle.isOn)
        {
            muteToggle.isOn = true;
        }
    }

    public void ToggleAudioVolume(bool isMuted)
    {
        if (!isMuted)
        {
            // 음소거
            masterMixer.SetFloat(mixType, -80);
            audioSlider.interactable = false;
        }
        else
        {
            // 음소거 해제
            masterMixer.SetFloat(mixType, lastVolume);
            audioSlider.interactable = true;
        }
    }
}