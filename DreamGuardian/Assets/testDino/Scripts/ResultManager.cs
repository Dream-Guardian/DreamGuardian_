using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviourPunCallbacks
{
    public Text Title;
    public Text KillCountText;
    public Text GameTimeText;
    public Text BaseHealthText;
    public Button RestartBtn;
    public GameObject Stage01Back;
    public GameObject Stage02Back;
    public GameObject Stage03Back;
    private string level;
    public GameObject StageFinalBack;
    private string isCleared;
    private string killCnt;
    private string gameTime;
    private string baseHealth;
    private string nowStage;
    private PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();

        Stage01Back.SetActive(false);
        Stage02Back.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            isCleared = PlayerPrefs.GetString("isCleared");
            killCnt = PlayerPrefs.GetString("killCnt");
            gameTime = PlayerPrefs.GetString("gameTime");
            baseHealth = PlayerPrefs.GetString("baseHealth");
            nowStage = PlayerPrefs.GetString("checkedStage");
            level = PlayerPrefs.GetString("checkedLevel");

            pv.RPC("RPCSetVariables", RpcTarget.All, isCleared, killCnt, gameTime, baseHealth, nowStage, DateTime.Now.ToString(("yyyy-MM-dd tt HH:mm:ss")), level);

            RestartBtn.gameObject.SetActive(true);
        }
    }
    [PunRPC]
    public void RPCSetVariables(string cleared, string kills, string time, string health, string stage, string timeAt, string level)
    {
        isCleared = cleared;
        if (isCleared.Contains("!"))
        {
            SaveManager.SaveData(1, kills, time, health, stage, timeAt, level);
        }
        killCnt = kills;
        gameTime = time;
        baseHealth = health;
        nowStage = stage;



        if (nowStage.Contains("Final"))
        {
            Stage01Back.SetActive(false);
            Stage02Back.SetActive(false);
            Stage03Back.SetActive(false);
            StageFinalBack.SetActive(true);
        }
        else if (nowStage.Contains("Stage 1"))
        {
            Stage01Back.SetActive(true);
            Stage02Back.SetActive(false);
            Stage03Back.SetActive(false);
            StageFinalBack.SetActive(false);
        } else if (nowStage.Contains("Stage 2"))
        {
            Stage01Back.SetActive(false);
            Stage02Back.SetActive(true);
            Stage03Back.SetActive(false);
            StageFinalBack.SetActive(false);
        } else if (nowStage.Contains("Stage 3"))
        {
            Stage01Back.SetActive(false);
            Stage02Back.SetActive(false);
            Stage03Back.SetActive(true);
            StageFinalBack.SetActive(false);
        }

        UpdateUI();
    }
    private void UpdateUI()
    {
        Title.text = isCleared;
        KillCountText.text = killCnt;
        GameTimeText.text = gameTime;
        BaseHealthText.text = baseHealth;
    }
    public void restartScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
            PhotonNetwork.LoadLevel(PlayerPrefs.GetString("checkedStage"));
        }
    }
    public void ToLobbyScene()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void ToRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
            PhotonNetwork.LoadLevel("Lobby2");
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("Lobby2");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                RestartBtn.gameObject.SetActive(true);
            }
            else
            {
                RestartBtn.gameObject.SetActive(false);
            }
        }
    }
}
