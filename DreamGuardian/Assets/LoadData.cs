using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadData : MonoBehaviour
{
    public GameObject Stage1_1, Stage1_2, Stage1_3, Stage2_1, Stage2_2, Stage2_3, Stage2_4, Stage3_1, Stage3_2, Stage3_3, StageHidden1, StageHidden2;
    private Dictionary<string, GameObject> stageObjects;
    private Dictionary<string, StageInfo> stageInfos;

    private class StageInfo
    {
        public String ShortestTime { get; set; }
        public String Health { get; set; }
        public int PlayerCount { get; set; }
    }

    void OnEnable()
    {
        ClearDataList allData = SaveManager.LoadData();
        if(allData != null)
        {
            InitializeStageObjects();
            ResetStageUI();
            ProcessClearData();
        }
    }

    void InitializeStageObjects()
    {
        stageObjects = new Dictionary<string, GameObject>
        {
            {"Stage 1-1", Stage1_1}, {"Stage 1-2", Stage1_2}, {"Stage 1-3", Stage1_3},
            {"Stage 2-1", Stage2_1}, {"Stage 2-2", Stage2_2}, {"Stage 2-3", Stage2_3}, {"Stage 2-4", Stage2_4},
            {"Stage 3-1", Stage3_1}, {"Stage 3-2", Stage3_2}, {"Stage 3-3", Stage3_3},
            {"Final Stage 1", StageHidden1}, {"Final Stage 2", StageHidden2}
        };
    }

    void ResetStageUI()
    {
        foreach (var stageObj in stageObjects.Values)
        {
            stageObj.transform.Find("IsCleared").gameObject.SetActive(false);
            stageObj.transform.Find("ClearInfo").gameObject.SetActive(false);
            stageObj.transform.Find("OneStar").gameObject.SetActive(false);
            stageObj.transform.Find("TwoStars").gameObject.SetActive(false);
            stageObj.transform.Find("ThreeStars").gameObject.SetActive(false);
        }
    }

    void ProcessClearData()
    {
        ClearDataList allData = SaveManager.LoadData();
        stageInfos = new Dictionary<string, StageInfo>();

        foreach (var data in allData.clearDataList)
        {
            if (data.isCleared == 1 && PlayerPrefs.GetString("checkedLevel") == data.level)
            {
                UpdateStageInfo(data);
            }
        }

        DisplayClearInfo();
    }

    void UpdateStageInfo(ClearData data)
    {
        if (stageObjects.TryGetValue(data.now_stage, out GameObject stageObj))
        {
            stageObj.transform.Find("IsCleared").gameObject.SetActive(true);

            if (!stageInfos.ContainsKey(data.now_stage))
            {
                stageInfos[data.now_stage] = new StageInfo();
            }

            float healthValue = ParseHealthPercentage(data.health);

            var stageInfo = stageInfos[data.now_stage];
            if (healthValue > ParseHealthPercentage(stageInfo.Health))
            {
                if(healthValue < 33f)
                {
                    stageObj.transform.Find("TwoStars").gameObject.SetActive(false);
                    stageObj.transform.Find("ThreeStars").gameObject.SetActive(false);
                    stageObj.transform.Find("OneStar").gameObject.SetActive(true);
                }
                else if(healthValue < 66f)
                {
                    stageObj.transform.Find("ThreeStars").gameObject.SetActive(false);
                    stageObj.transform.Find("OneStar").gameObject.SetActive(false);
                    stageObj.transform.Find("TwoStars").gameObject.SetActive(true);
                }
                else if(healthValue < 100f)
                {
                    stageObj.transform.Find("OneStar").gameObject.SetActive(false);
                    stageObj.transform.Find("TwoStars").gameObject.SetActive(false);
                    stageObj.transform.Find("ThreeStars").gameObject.SetActive(true);
                }
                stageInfo.Health = data.health;
                stageInfo.ShortestTime = data.time_length;
                stageInfo.PlayerCount = data.playerNum;
            }
        }
    }

    private float ParseHealthPercentage(string healthString)
    {
        if (string.IsNullOrEmpty(healthString))
            return 0f;

        // '%' 기호와 공백 제거
        healthString = healthString.Trim('%', ' ');

        if (float.TryParse(healthString, out float result))
        {
            return result;
        }

        return 0f; // 파싱 실패 시 0 반환
    }

    void DisplayClearInfo()
    {
        foreach (var kvp in stageInfos)
        {
            if (stageObjects.TryGetValue(kvp.Key, out GameObject stageObj))
            {
                Text clearInfoText = stageObj.transform.Find("ClearInfo").GetComponent<Text>();
                var info = kvp.Value;
                clearInfoText.text = $"베스트 체력: {info.Health}\n" +
                                     $"플레이 시간: {info.ShortestTime:hh\\:mm}\n" +
                                     $"플레이 인원: {info.PlayerCount}명";
                stageObj.transform.Find("ClearInfo").gameObject.SetActive(true);
            }
        }
    }
}