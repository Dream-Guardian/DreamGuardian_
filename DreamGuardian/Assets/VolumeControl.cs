using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    public AudioMixer masterMixer;
    public Slider audioSlider;
    public Toggle muteToggle;  // ���Ұ� ��� UI ���
    private string mixType;
    private float lastVolume;  // ���������� ������ ���� ��

    void Start()
    {
        mixType = audioSlider.name;
        lastVolume = audioSlider.value;

        // ��� �̺�Ʈ ������ �߰�
        muteToggle.onValueChanged.AddListener(ToggleAudioVolume);
    }

    public void AudioControl()
    {
        float sound = audioSlider.value;
        if (sound == -40f) masterMixer.SetFloat(mixType, -80);
        else masterMixer.SetFloat(mixType, sound);

        lastVolume = sound;

        // ������ ����Ǹ� ���Ұ� ����
        if (muteToggle.isOn)
        {
            muteToggle.isOn = true;
        }
    }

    public void ToggleAudioVolume(bool isMuted)
    {
        if (!isMuted)
        {
            // ���Ұ�
            masterMixer.SetFloat(mixType, -80);
            audioSlider.interactable = false;
        }
        else
        {
            // ���Ұ� ����
            masterMixer.SetFloat(mixType, lastVolume);
            audioSlider.interactable = true;
        }
    }
}