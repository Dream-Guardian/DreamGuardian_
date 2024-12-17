using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ClearData
{
    public string name;
    public int isCleared;
    public string killed;
    public string time_length;
    public string now_stage;
    public string time_at;
    public string health;
    public string level;
    public int playerNum;
}

[Serializable]
public class ClearDataList
{
    public List<ClearData> clearDataList = new List<ClearData>();
}

[System.Serializable]
public class IdContainer
{
    public List<string> Ids;
}


public static class SaveManager
{
    private const string FILE_NAME = "clearData.json";
    private const string ETC_NAME = "Etc";

    public static void SaveData(int isCleared, string kills, string time, string health, string stage, string timeAt, string level)
    {
        ClearData data = new ClearData
        {
            isCleared = isCleared,
            killed = kills,
            time_length = time,
            health = health,
            now_stage = stage,
            time_at = timeAt,
            level = level,
            name = PhotonNetwork.LocalPlayer.NickName,
            playerNum = PhotonNetwork.CurrentRoom.PlayerCount
        };

        string path = Path.Combine(Application.persistentDataPath, FILE_NAME);
        ClearDataList dataList = new ClearDataList();

        try
        {
            // ������ �����ϸ� ���� �����͸� �ε�
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                dataList = JsonUtility.FromJson<ClearDataList>(json);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            // �� ������ �߰�
            dataList.clearDataList.Add(data);

            // JSON���� ����ȭ
            string jsonToSave = JsonUtility.ToJson(dataList, true);

            // ���Ͽ� ������ ����
            File.WriteAllText(path, jsonToSave);
            Debug.Log($"Data saved to {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }


    public static ClearDataList LoadData()
    {
        string path = Path.Combine(Application.persistentDataPath, FILE_NAME);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<ClearDataList>(json);
        }
        else
        {
            Debug.LogWarning($"������ ã�� �� ���� {path}");
            return null;
        }
    }
    
    public static IdContainer LoadEtc()
    {
        TextAsset json = Resources.Load(ETC_NAME) as TextAsset;
        if (json != null)
        {
            return JsonUtility.FromJson<IdContainer>(json.ToString());
        }
        else
        {
            Debug.LogWarning($"������ ã�� �� ���� {json}");
            return null;
        }
    }
}