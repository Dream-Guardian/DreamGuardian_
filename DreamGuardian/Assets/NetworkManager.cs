using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    [Header("DisconnectPanel")]
    public GameObject DisconnectPanel;
    public InputField NickNameInput;
    [Header("LobbyPanel")]
    public GameObject LobbyPanel;
    public InputField RoomInput;
    // public InputField PasswordInput;
    public Text WelcomeText;
    public Text LobbyInfoText;
    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;

    [Header("RoomPanel")]
    public GameObject RoomPanel;
    public Text ListText;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;
    public GameObject SelectLevel;

    [Header("SelectStagePanel")]
    public GameObject SelectStagePanel;
    public Button Stage1;
    public Button Stage2;
    public Button Stage3;
    public Button StageFinal;
    public GameObject Stage1Check;
    public GameObject Stage2Check;
    public GameObject Stage3Check;
    public GameObject StageFinalCheck;
    public Text nowLevel;
    public Text nowLevelShadow;

    [Header("SelectStageDetailPanel")]
    public GameObject DefaultDetail;
    public GameObject Stage1Detail;
    public GameObject Stage2Detail;
    public GameObject Stage3Detail;
    public GameObject StageFinalDetail;
    public GameObject Stage1_1;
    public GameObject Stage1_2;
    public GameObject Stage1_3;
    public GameObject Stage2_1;
    public GameObject Stage2_2;
    public GameObject Stage2_3;
    public GameObject Stage2_4;
    public GameObject Stage3_1;
    public GameObject Stage3_2;
    public GameObject Stage3_3;
    public GameObject StageHidden;
    public GameObject StageHidden2;

    [Header("StageBackground")]
    //public GameObject Stage1Background;
    //public GameObject Stage2Background;
    //public GameObject Stage3Background;

    [Header("ETC")]
    public GameObject EscPanel;
    public GameObject KeySetting;
    public Text StatusText;
    public Button StartBtn;
    public Button StageSelectBtn;
    public PhotonView PV;
    private bool isClicked; // 광클 금지를 위한 bool
    // public string nowCheckedStage;
    //private bool isTutorial;
    public static List<string> Etcs;
    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;
    Dictionary<string, string> serverMap;


    void Awake()
    {
        Etcs = SaveManager.LoadEtc().Ids;
        serverMap = new Dictionary<string, string>
    {
        { Etcs[0], "1" },
        { Etcs[1], "2" },
        { Etcs[2], "3" },
        { Etcs[3], "4" },
        { Etcs[4], "5" },
        { Etcs[5], "6"}
    };
        StatusText.text = "";
        isClicked = false;
        Application.runInBackground = true;
        PlayerPrefs.SetString("checkedStage", "0");
        PlayerPrefs.SetString("isTutorial", "0");
    }
    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = true;
                PhotonNetwork.CurrentRoom.IsVisible = true;
                StartBtn.gameObject.SetActive(true);
                StageSelectBtn.gameObject.SetActive(true);
            }
            else
            {
                StartBtn.gameObject.SetActive(false);
                StageSelectBtn.gameObject.SetActive(false);
            }
            isClicked = false;
            DisconnectPanel.SetActive(false);
            LobbyPanel.SetActive(false);
            RoomPanel.SetActive(true);
            SelectStagePanel.SetActive(false);
            SelectLevel.SetActive(false);

            Stage1Check.SetActive(false);
            Stage2Check.SetActive(false);
            Stage3Check.SetActive(false);
            StageFinalCheck.SetActive(false);

            DefaultDetail.SetActive(true);
            Stage1Detail.SetActive(false);
            Stage2Detail.SetActive(false);
            Stage3Detail.SetActive(false);
            StageFinalDetail.SetActive(false);

            Stage1_1.SetActive(false);
            Stage1_2.SetActive(false);
            Stage1_3.SetActive(false);
            Stage2_1.SetActive(false);
            Stage2_2.SetActive(false);
            Stage2_3.SetActive(false);
            Stage2_4.SetActive(false);
            Stage3_1.SetActive(false);
            Stage3_2.SetActive(false);
            Stage3_3.SetActive(false);
            StageHidden.SetActive(false);
            StageHidden2.SetActive(false);



            RoomRenewal();
            ChatInput.text = "";
            for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
        }
        else if (PhotonNetwork.IsConnected)
        {
            DisconnectPanel.SetActive(false);
            LobbyPanel.SetActive(true);
            RoomPanel.SetActive(false);
            SelectStagePanel.SetActive(false);
            SelectLevel.SetActive(false);

            Stage1Check.SetActive(false);
            Stage2Check.SetActive(false);
            Stage3Check.SetActive(false);
            StageFinalCheck.SetActive(false);

            DefaultDetail.SetActive(true);
            Stage1Detail.SetActive(false);
            Stage2Detail.SetActive(false);
            Stage3Detail.SetActive(false);
            StageFinalDetail.SetActive(false);

            Stage1_1.SetActive(false);
            Stage1_2.SetActive(false);
            Stage1_3.SetActive(false);
            Stage2_1.SetActive(false);
            Stage2_2.SetActive(false);
            Stage2_3.SetActive(false);
            Stage2_4.SetActive(false);
            Stage3_1.SetActive(false);
            Stage3_2.SetActive(false);
            Stage3_3.SetActive(false);
            StageHidden.SetActive(false);
            StageHidden2.SetActive(false);
        }
    }

    public void quitGame()
    {
        PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
        Application.Quit();
    }

    #region 방리스트 갱신
    // ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else
        {
            PhotonNetwork.JoinRoom(myList[multiple + num].Name);
            StatusText.text = "방에 들어가는 중..";
        }
        MyListRenewal();
    }

    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % CellBtn.Length == 0) ? myList.Count / CellBtn.Length : myList.Count / CellBtn.Length + 1;

        // 이전, 다음버튼
        PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        NextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellBtn[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }
    #endregion


    #region 서버연결

    void Update()
    {
        // StatusText.text = PhotonNetwork.NetworkClientState.ToString();
        if (serverMap != null)
            LobbyInfoText.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + "로비 / " + PhotonNetwork.CountOfPlayers + "접속 (" + serverMap[PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime] + "서버)";

        if (RoomPanel.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Send();
        }

        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (EscPanel.activeSelf)
            {
                if (KeySetting.activeSelf)
                {
                    KeySetting.SetActive(false);
                }
                else
                {
                    EscPanel.SetActive(false);
                }
            }
            else
            {
                EscPanel.SetActive(true);
            }
        }
    }

    // ESC 버튼 내부에 KeySetting 창 열기/닫기 버튼
    public void OpenKeySetting()
    {
        KeySetting.SetActive(true);
    }
    public void CloseKeySetting()
    {
        KeySetting.SetActive(false);
    }

    public void Connect()
    {
        if (string.IsNullOrEmpty(NickNameInput.text))
        {
            // 닉네임이 입력되지 않았을 경우 경고 메시지 표시
            StatusText.text = "닉네임을 입력해주세요!";
        }
        else
        {
            if (isClicked) return;
            isClicked = true;
            // 닉네임이 입력되었을 경우 연결 진행
            StatusText.text = "접속중..";
            PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = PlayerPrefs.GetString("SelectedServer");
            // 서버 지역을 Asia로 설정 (베트남과 한국 모두 같은 서버로 접속)
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (PhotonNetwork.CountOfPlayers >= 19)
        {
            Disconnect();
            StatusText.text = "다른 서버를 이용해 주세요!";
        }
        else
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        isClicked = false;
        StatusText.text = "";
        DisconnectPanel.SetActive(false);
        LobbyPanel.SetActive(true);
        RoomPanel.SetActive(false);

        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다";
        myList.Clear();
        MyListRenewal();
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isClicked = false;
        StatusText.text = "";
        DisconnectPanel.SetActive(true);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);
    }
    #endregion


    #region 방
    public void CreateRoom()
    {
        if (isClicked) return;
        isClicked = true;
        StatusText.text = "방 생성 중..";
        PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });
    }

    public void GoToTutorialRoom()
    {
        if (isClicked) return;
        isClicked = true;
        PlayerPrefs.SetString("isTutorial", "1");

        PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Room" + Random.Range(0, 100) : RoomInput.text, new RoomOptions { MaxPlayers = 4 });
    }

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnJoinedRoom()
    {
        RoomInput.text = "";
        SelectLevel.SetActive(false);
        StatusText.text = "";
        if (PlayerPrefs.GetString("isTutorial") == "1")
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel("Tutorial");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartBtn.gameObject.SetActive(true);
            StageSelectBtn.gameObject.SetActive(true);
        }
        else
        {
            StartBtn.gameObject.SetActive(false);
            StageSelectBtn.gameObject.SetActive(false);
        }
        isClicked = false;
        DisconnectPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(true);
        RoomRenewal();
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnJoinRandomFailed(short returnCode, string message) { RoomInput.text = ""; CreateRoom(); }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        isClicked = false;
        PhotonNetwork.JoinLobby();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    void RoomRenewal()
    {
        ListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            ListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name;
    }
    #endregion


    #region 채팅
    public void Send()
    {
        if (string.IsNullOrWhiteSpace(ChatInput.text))
        {
            return; // 빈 메시지이거나 공백만 있는 경우 전송하지 않음
        }
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";

        ChatInput.ActivateInputField();
        ChatInput.Select();
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
    #endregion



    #region 스테이지 선택

    // 스테이지 선택 로직
    public void SelectStage1()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage1", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage1()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 1");
        Stage1Check.SetActive(true);
        Stage2Check.SetActive(false);
        Stage3Check.SetActive(false);
        StageFinalCheck.SetActive(false);

        DefaultDetail.SetActive(false);
        Stage1Detail.SetActive(true);
        Stage2Detail.SetActive(false);
        Stage3Detail.SetActive(false);
        StageFinalDetail.SetActive(false);

        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage2()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage2", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage2()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 2");
        Stage1Check.SetActive(false);
        Stage2Check.SetActive(true);
        Stage3Check.SetActive(false);
        StageFinalCheck.SetActive(false);

        DefaultDetail.SetActive(false);
        Stage1Detail.SetActive(false);
        Stage2Detail.SetActive(true);
        Stage3Detail.SetActive(false);
        StageFinalDetail.SetActive(false);

        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage3()
    {
        //return;
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage3", RpcTarget.AllViaServer);
        }
    }

    [PunRPC]
    public void RPCSelectStage3()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 3");
        Stage1Check.SetActive(false);
        Stage2Check.SetActive(false);
        Stage3Check.SetActive(true);
        StageFinalCheck.SetActive(false);

        DefaultDetail.SetActive(false);
        Stage1Detail.SetActive(false);
        Stage2Detail.SetActive(false);
        Stage3Detail.SetActive(true);
        StageFinalDetail.SetActive(false);

        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStageHidden()
    {
        //return;
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStageHidden", RpcTarget.AllViaServer);
        }
    }

    [PunRPC]
    public void RPCSelectStageHidden()
    {
        PlayerPrefs.SetString("checkedStage", "Stage H");
        Stage1Check.SetActive(false);
        Stage2Check.SetActive(false);
        Stage3Check.SetActive(false);
        StageFinalCheck.SetActive(true);

        DefaultDetail.SetActive(false);
        Stage1Detail.SetActive(false);
        Stage2Detail.SetActive(false);
        Stage3Detail.SetActive(false);
        StageFinalDetail.SetActive(true);

        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void BackToRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCBackToRoom", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCBackToRoom()
    {
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.CurrentRoom.IsVisible = true;
        PlayerPrefs.SetString("checkedStage", "2");
        DisconnectPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(true);
        SelectStagePanel.SetActive(false);

        Stage1Check.SetActive(false);
        Stage2Check.SetActive(false);
        Stage3Check.SetActive(false);
        StageFinalCheck.SetActive(false);

        DefaultDetail.SetActive(true);
        Stage1Detail.SetActive(false);
        Stage2Detail.SetActive(false);
        Stage3Detail.SetActive(false);
        StageFinalDetail.SetActive(false);

        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

        //Stage1Background.SetActive(true);
    }

    #endregion

    #region 스테이지 세부 선택

    public void SelectStage1_1()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage1_1", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage1_1()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 1-1");
        Stage1_1.SetActive(true);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage1_2()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage1_2", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage1_2()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 1-2");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(true);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage1_3()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage1_3", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage1_3()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 1-3");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(true);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage2_1()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage2_1", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage2_1()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 2-1");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(true);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage2_2()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage2_2", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage2_2()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 2-2");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(true);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage2_3()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage2_3", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage2_3()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 2-3");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(true);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage2_4()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage2_4", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage2_4()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 2-4");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(true);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }


    public void SelectStage3_1()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage3_1", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage3_1()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 3-1");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(true);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage3_2()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage3_2", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage3_2()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 3-2");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(true);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStage3_3()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStage3_3", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStage3_3()
    {
        PlayerPrefs.SetString("checkedStage", "Stage 3-3");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(true);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(false);

    }

    public void SelectStageHidden1()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStageHidden1", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStageHidden1()
    {
        PlayerPrefs.SetString("checkedStage", "Final Stage 1");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(true);
        StageHidden2.SetActive(false);

    }

    public void SelectStageHidden2()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCSelectStageHidden2", RpcTarget.AllViaServer);
        }
    }
    [PunRPC]
    public void RPCSelectStageHidden2()
    {
        PlayerPrefs.SetString("checkedStage", "Final Stage 2");
        Stage1_1.SetActive(false);
        Stage1_2.SetActive(false);
        Stage1_3.SetActive(false);
        Stage2_1.SetActive(false);
        Stage2_2.SetActive(false);
        Stage2_3.SetActive(false);
        Stage2_4.SetActive(false);
        Stage3_1.SetActive(false);
        Stage3_2.SetActive(false);
        Stage3_3.SetActive(false);
        StageHidden.SetActive(false);
        StageHidden2.SetActive(true);

    }

    #endregion


    #region 스테이지 난이도 선택

    public void GoToStageSelectNormal()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCGoToStageSelectNormal", RpcTarget.AllViaServer);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }
    [PunRPC]
    public void RPCGoToStageSelectNormal()
    {
        PlayerPrefs.SetString("checkedLevel", "normal");
        DisconnectPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);
        SelectStagePanel.SetActive(true);
        SelectLevel.SetActive(false);
        nowLevel.text = "Mode : Normal";
        nowLevelShadow.text = "Mode : Normal";
    }

    public void GoToStageSelectHard()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCGoToStageSelectHard", RpcTarget.AllViaServer);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }
    [PunRPC]
    public void RPCGoToStageSelectHard()
    {
        PlayerPrefs.SetString("checkedLevel", "hard");
        DisconnectPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);
        SelectStagePanel.SetActive(true);
        SelectLevel.SetActive(false);
        nowLevel.text = "Mode : Hard";
        nowLevelShadow.text = "Mode : Hard";
    }

    public void GoToStageSelectCrazy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PV.RPC("RPCGoToStageSelectCrazy", RpcTarget.AllViaServer);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }
    [PunRPC]
    public void RPCGoToStageSelectCrazy()
    {
        PlayerPrefs.SetString("checkedLevel", "crazy");
        DisconnectPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        RoomPanel.SetActive(false);
        SelectStagePanel.SetActive(true);
        SelectLevel.SetActive(false);
        nowLevel.text = "Mode : Crazy";
        nowLevelShadow.text = "Mode : Crazy";
    }

    #endregion


    public void GoToLevelSelect()
    {
        PV.RPC("RPCGoToLevelSelect", RpcTarget.AllViaServer);
    }
    [PunRPC]
    public void RPCGoToLevelSelect()
    {

        SelectLevel.SetActive(true);

    }

    public void OnClickStartButton()
    {
        if (PlayerPrefs.GetString("checkedStage") != null)
        {
            if (isClicked || PlayerPrefs.GetString("checkedStage") == "0"
                || PlayerPrefs.GetString("checkedStage") == "2"
                || PlayerPrefs.GetString("checkedStage") == "Stage 2"
                || PlayerPrefs.GetString("checkedStage") == "Stage 1"
                || PlayerPrefs.GetString("checkedStage") == "Stage 3"
                || PlayerPrefs.GetString("checkedStage") == "Stage H"
                ) return;
            isClicked = true;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(PlayerPrefs.GetString("checkedStage"));
        }
        else
        {
            return;
        }

    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StartBtn.gameObject.SetActive(true);
                StageSelectBtn.gameObject.SetActive(true);
            }
            else
            {
                StartBtn.gameObject.SetActive(false);
                StageSelectBtn.gameObject.SetActive(false);
            }
        }
    }
}