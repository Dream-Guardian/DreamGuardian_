
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class DropDownController : MonoBehaviourPunCallbacks
{
    TMP_Dropdown options;
    List<string> Etcs;
    string[] appIDs;
    void Awake()
    {
        Etcs = SaveManager.LoadEtc().Ids;
        appIDs = new string[] { Etcs[0], Etcs[1], Etcs[2], Etcs[3], Etcs[4], Etcs[5] };
        setDropDown(0); // 기본값은 서버 1
    }

    void Start()
    {
        options = this.GetComponent<TMP_Dropdown>();
        options.value = 0; // 기본값은 서버 1
        options.onValueChanged.AddListener(delegate { setDropDown(options.value); }); // 드롭다운 이벤트 리스너 추가
    }

    void setDropDown(int option)
    {
        PlayerPrefs.SetString("SelectedServer", appIDs[option]); // PlayerPrefs에 저장해서 네트워크매니저에서 참조 가능하게
    }
}