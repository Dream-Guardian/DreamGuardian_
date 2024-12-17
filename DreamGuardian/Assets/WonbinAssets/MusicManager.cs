using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using UnityEngine.UIElements;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class MusicLayer
    {
        public AudioSource audioSource;
        public float targetVolume = 0.4f;
    }

    [SerializeField] private AudioMixer audioMixer;

    public List<MusicLayer> musicLayers;
    private int currentLayer = 0;
    public float fadeTime = 2f;
    public float volumeThreshold = 0.01f;
    public static int isStarted = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        foreach (var layer in musicLayers)
        {
            if (layer.audioSource.loop && layer.audioSource.clip.name.Contains("stage"))
            {
                layer.audioSource.volume = 0;
                layer.audioSource.Play();
            }
        }
        StartCoroutine(FadeInLayer(0));
    }

    //private void FixedUpdate()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        mute();
    //        musicLayers[0].audioSource.Stop();
    //    }
    //    else if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        playM();
    //        musicLayers[0].audioSource.Play();
    //    }
    //}

    private void FixedUpdate()
    {
        if(PlayerPrefs.GetString("checkedStage") == "0")
        {
            if(isStarted == 1)
                Startsound();
        }
    }

    public void ActivateNextLayer()
    {
        int nextLayer = (currentLayer + 1) % (musicLayers.Count - 1);
        StartCoroutine(TransitionLayers(currentLayer, nextLayer));
        currentLayer = nextLayer;
    }

    private IEnumerator TransitionLayers(int fromLayer, int toLayer)
    {
        StartCoroutine(FadeOutLayer(fromLayer));
        yield return new WaitForSeconds(fadeTime / 2);
        StartCoroutine(FadeInLayer(toLayer));
    }

    private IEnumerator FadeInLayer(int layerIndex)
    {
        MusicLayer layer = musicLayers[layerIndex];
        while (layer.audioSource.volume < layer.targetVolume - volumeThreshold)
        {
            layer.audioSource.volume = Mathf.MoveTowards(layer.audioSource.volume, layer.targetVolume, Time.deltaTime / fadeTime);
            yield return null;
        }
        layer.audioSource.volume = layer.targetVolume;
    }

    private IEnumerator FadeOutLayer(int layerIndex)
    {
        MusicLayer layer = musicLayers[layerIndex];
        while (layer.audioSource.volume > volumeThreshold)
        {
            layer.audioSource.volume = Mathf.MoveTowards(layer.audioSource.volume, 0, Time.deltaTime / fadeTime);
            yield return null;
        }
        layer.audioSource.volume = 0;
    }

    public static void Firesound()
    {
        MusicLayer layer = Instance.musicLayers[2];
        layer.audioSource.Play();
    }

    public static void MakeTreesound()
    {
        MusicLayer layer = Instance.musicLayers[3];
        layer.audioSource.Play();
    }

    public static void Craftsound()
    {
        MusicLayer layer = Instance.musicLayers[4];
        layer.audioSource.Play();
    }

    public static void MIxsound()
    {
        MusicLayer layer = Instance.musicLayers[5];
        layer.audioSource.Play();
    }

    public static void Startsound()
    {
        foreach (var layer in Instance.musicLayers)
        {
            if (layer.audioSource.clip.name.Contains("For"))
            {
                if (!layer.audioSource.isPlaying)
                {
                    layer.audioSource.Play();
                    isStarted = 1;
                }
            }
        }
    }

    public static void TurretReady()
    {
        audioPlay("TurretReady");
    }

    public static void Liftup()
    {
        audioPlay("Liftup");
    }
    public static void Liftdown()
    {
       audioPlay("Liftdown");
    }

    public static void Steel()
    {
        audioPlay("Steel");
    }
    public static void Flame()
    {
        audioPlay("Flame");
    }
    public static void FlameStop()
    {
        audioPlay("Flame");
    }
    public static void FlameEnemy()
    {
        audioPlay("FlameEnemy");
    }
    public static void Enemy()
    {
        audioPlay("Enemy");
    }

    public static void Boil()
    {
        audioPlay("Boil");
    }

    public static void Warning()
    {
        audioPlay("Warning");
    }
    public static void Defense()
    {
        audioPlay("Defense");
    }

    public static void audioPlay(string name)
    {
        foreach(var layer in Instance.musicLayers)
        {
            if(layer.audioSource.clip.name == name)
            {
                layer.audioSource.Play();
            }
        }
    }

    public static void mute()
    {
        Instance.musicLayers[0].audioSource.mute = true;
    }

    public static void playM()
    {
        Instance.musicLayers[0].audioSource.mute = false;
    }
}