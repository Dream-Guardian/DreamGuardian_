using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Base : MonoBehaviour
{
    public static Base instance;
    public float maxHealth;
    public float currentHealth;
    private bool isFinished;
    private bool isTurnon;

    public Image baseHp;

    void Awake()
    {
        instance = this;
        isFinished = false;
        isTurnon = false;
        if (PlayerPrefs.GetString("checkedStage").Contains("Final")) maxHealth = 50000f;
        else if(PlayerPrefs.GetString("checkedStage").Contains("Stage 3")) maxHealth = 40000f;
        else maxHealth = 30000f;
        currentHealth = maxHealth;
        baseHp = GameObject.Find("BaseRestHp").GetComponent<Image>();
    }


    public void TakeDamage(float damage)
    {
        if(PhotonNetwork.IsMasterClient) GetComponent<PhotonView>().RPC("TakeDamagePT", RpcTarget.AllViaServer, damage);
    }

    [PunRPC]
    public void TakeDamagePT(float damage)
    {
        currentHealth -= damage;

        if (!isTurnon && currentHealth <= maxHealth * 0.5)
        {
            isTurnon = true;
            //Debug.Log("bgm 크로스 페이드 on");
            GameObject musicManager = GameObject.Find("MusicManager");
            MusicManager musicManagercs = musicManager.GetComponentInChildren<MusicManager>();
            musicManagercs.ActivateNextLayer();
        }
        else if (currentHealth <= 0 && !isFinished)
        {
            isFinished = true;
            GameOver();
        }

        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (baseHp != null)
        {
            baseHp.fillAmount = currentHealth / maxHealth;
        }
    }

    void GameOver()
    {
        StopAllCoroutines();
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.gameOver();
        }
    }
}