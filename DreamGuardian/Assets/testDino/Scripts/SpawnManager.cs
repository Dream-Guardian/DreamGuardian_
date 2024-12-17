using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.Rendering;
using TMPro;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public static SpawnManager instance;
    public GameObject mobPrefab;
    public GameObject tankerPrefab;
    public GameObject dealerPrefab;
    public GameObject bossPrefab;
    public GameObject bossHp;
    public GameObject warningUI;
    public int poolSize;
    public float spawnRadius;
    public Image warningImage;
    private Queue<GameObject> mobPool;
    private Coroutine mobSpawnCoroutine;
    private Coroutine bossSpawnCoroutine;
    public PhotonView pv;
    private float mobSpawnStart;
    private float mobSpawnInterval;
    private float bossSpawnStart;
    private WaitForSeconds mobSpawnWait;
    private WaitForSeconds bossSpawnWait;
    private Image bossHpFillImage;
    private TextMeshProUGUI bossHpText;
    private string checkedStage;
    private string checkedLevel;
    private string playerCount;
    public Canvas targetCanvas;
    private GameObject flashPanel;
    private Image flashImage;
    private bool hasTeleported;
    public float flashDuration;
    public int flashCount;
    public float maxDarkness;
    private bool isDoubled;

    void Awake()
    {
        instance = this;
        checkedStage = PlayerPrefs.GetString("checkedStage");
        checkedLevel = PlayerPrefs.GetString("checkedLevel");
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        poolSize = 30;
        spawnRadius = 20f;
        pv = GetComponent<PhotonView>();
        bossHpFillImage = bossHp.transform.Find("FillArea/Fill").GetComponent<Image>();
        bossHpText = bossHp.transform.Find("FillArea/Text_Value").GetComponent<TextMeshProUGUI>();
        flashCount = 3;
        flashDuration = 3f;
        maxDarkness = 1f;
        hasTeleported = false;
        isDoubled = false;
    }

    void Start()
    {
        mobSpawnStart = GameSettingsManager.instance.GetSettingValue(checkedStage, playerCount, checkedLevel, "mobSpawnStart");
        mobSpawnInterval = GameSettingsManager.instance.GetSettingValue(checkedStage, playerCount, checkedLevel, "mobSpawnIntervalBeforeBoss");
        bossSpawnStart = GameSettingsManager.instance.GetSettingValue(checkedStage, playerCount, checkedLevel, "bossSpawnStart");
        mobSpawnWait = new WaitForSeconds(mobSpawnInterval);
        bossSpawnWait = new WaitForSeconds(bossSpawnStart);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("BossBullet"), LayerMask.NameToLayer("Mob"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("BossBullet"), LayerMask.NameToLayer("Boss"), true);

        InitializePool();

        if (PhotonNetwork.IsMasterClient)
        {
            StartSpawning();
        }
        CreateFlashPanel();
    }

    void InitializePool()
    {
        mobPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject mob = Instantiate(mobPrefab, new Vector3(100f, -100f, 100f), Quaternion.identity);
            mob.SetActive(false);
            mobPool.Enqueue(mob);
        }
    }

    void StartSpawning()
    {
        mobSpawnCoroutine = StartCoroutine(MobSpawnRoutine());
        bossSpawnCoroutine = StartCoroutine(BossSpawnRoutine());
    }

    IEnumerator MobSpawnRoutine()
    {
        yield return new WaitForSeconds(mobSpawnStart);
        while (true)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            pv.RPC("RPC_SpawnMob", RpcTarget.AllViaServer, spawnPosition);
            yield return mobSpawnWait;
        }
    }

    IEnumerator BossSpawnRoutine()
    {
        yield return bossSpawnWait;
        Vector3 spawnPosition = GetSpawnPosition();
        pv.RPC("RPC_SpawnBoss", RpcTarget.AllViaServer, spawnPosition);
    }

    IEnumerator RestartMobSpawnRoutine()
    {
        yield return new WaitForSeconds(mobSpawnStart - GameManager.instance.gameTime < 0 ? 0 : mobSpawnStart - GameManager.instance.gameTime);
        while (true)
        {
            yield return mobSpawnWait;
            Vector3 spawnPosition = GetSpawnPosition();
            pv.RPC("RPC_SpawnMob", RpcTarget.AllViaServer, spawnPosition);
        }
    }

    IEnumerator RestartBossSpawnRoutine()
    {
        if (bossSpawnStart - GameManager.instance.gameTime < 0)
        {
            yield return new WaitForSeconds(0f);
        }
        else
        {
            yield return new WaitForSeconds(bossSpawnStart - GameManager.instance.gameTime);
            Vector3 spawnPosition = GetSpawnPosition();
            pv.RPC("RPC_SpawnBoss", RpcTarget.AllViaServer, spawnPosition);
        }
    }

    [PunRPC]
    void RPC_SpawnMob(Vector3 position)
    {
        if (mobPool.Count > 0)
        {
            GameObject mob = mobPool.Dequeue();
            mob.transform.position = position;
            mob.SetActive(true);
        }
    }

    public void ReturnToPool(GameObject mob)
    {
        mob.SetActive(false);
        mobPool.Enqueue(mob);
    }

    [PunRPC]
    void RPC_SpawnBoss(Vector3 position)
    {
        mobSpawnWait = new WaitForSeconds(GameSettingsManager.instance.GetSettingValue(checkedStage, playerCount, checkedLevel, "mobSpawnIntervalAfterBoss"));
        for (int i = 0; i < 20; i++)
        {
            GameObject mob = Instantiate(dealerPrefab, new Vector3(100f, -100f, 100f), Quaternion.identity);
            mob.SetActive(false);
            mobPool.Enqueue(mob);
            mob = Instantiate(tankerPrefab, new Vector3(100f, -100f, 100f), Quaternion.identity);
            mob.SetActive(false);
            mobPool.Enqueue(mob);
            mob = Instantiate(mobPrefab, new Vector3(100f, -100f, 100f), Quaternion.identity);
            mob.SetActive(false);
            mobPool.Enqueue(mob);
            mobPool.Enqueue(mobPool.Dequeue());
        }
        bossHp.SetActive(true);
        GameObject bossObject = Instantiate(bossPrefab, position, Quaternion.identity);
        bossObject.transform.LookAt(Vector3.zero);
        Boss bossScript = bossObject.GetComponent<Boss>();

        if (bossScript != null)
        {
            bossHpFillImage.fillAmount = 0.153f;
            bossHpText.text = $"{bossScript.currentHealth} / {bossScript.maxHealth}";
        }
        StartCoroutine(BossWarningRoutine());
        MusicManager.Warning();
        StartCoroutine(BossWarningRoutine2(bossObject));
    }

    Vector3 GetSpawnPosition()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = spawnRadius * Mathf.Cos(randomAngle);
        float z = spawnRadius * Mathf.Sin(randomAngle);
        return new Vector3(x, 1f, z);
    }

    IEnumerator BossWarningRoutine()
    {
        int blinkCount = 3;
        float blinkDuration = 0.5f;
        Color originalColor = warningImage.color;
        Color color = originalColor;

        for (int i = 0; i < blinkCount; i++)
        {
            float time = 0f;
            warningImage.enabled = true;
            while (time < blinkDuration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 0.3f, time / blinkDuration);
                color.a = alpha;
                warningImage.color = color;
                yield return null;
            }

            time = 0f;

            while (time < blinkDuration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(0.3f, 0f, time / blinkDuration);
                color.a = alpha;
                warningImage.color = color;
                yield return null;
            }
        }

        warningImage.color = originalColor;
        warningImage.enabled = false;

        warningUI.SetActive(false);
    }

    IEnumerator BossWarningRoutine2(GameObject bossObject)
    {
        Vector2 startPos = new Vector2(-2400f, warningUI.GetComponent<RectTransform>().anchoredPosition.y);
        Vector2 middlePos = new Vector2(0, warningUI.GetComponent<RectTransform>().anchoredPosition.y);
        Vector2 endPos = new Vector2(2400f, warningUI.GetComponent<RectTransform>().anchoredPosition.y);

        warningUI.SetActive(true);
        warningUI.GetComponent<RectTransform>().anchoredPosition = startPos;

        float moveDuration = 0.7f;
        float moveTime = 0f;

        while (moveTime < moveDuration)
        {
            moveTime += Time.deltaTime;
            float t = moveTime / moveDuration;
            warningUI.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPos, middlePos, t);
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        moveTime = 0f;
        while (moveTime < moveDuration)
        {
            moveTime += Time.deltaTime;
            float t = moveTime / moveDuration;
            warningUI.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(middlePos, endPos, t);
            yield return null;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            mobSpawnCoroutine = StartCoroutine(RestartMobSpawnRoutine());
            bossSpawnCoroutine = StartCoroutine(RestartBossSpawnRoutine());
        }
    }

    [PunRPC]
    void RPCUpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (!isDoubled && currentHealth < maxHealth / 2)
        {
            isDoubled = true;
            mobSpawnWait = new WaitForSeconds(GameSettingsManager.instance.GetSettingValue(checkedStage, playerCount, checkedLevel, "mobSpawnIntervalAfterBoss") / 2);
        }
        Boss.instance.currentHealth = currentHealth;
        bossHpFillImage.fillAmount = currentHealth / maxHealth * 0.153f;
        bossHpText.text = $"{(int)currentHealth} / {(int)maxHealth}";
    }


    private void CreateFlashPanel()
    {
        Transform existingFlashPanel = targetCanvas.transform.Find("FlashPanel");
        if (existingFlashPanel != null)
        {
            Destroy(existingFlashPanel.gameObject);
        }

        flashPanel = new GameObject("FlashPanel");
        flashPanel.transform.SetParent(targetCanvas.transform, false);

        RectTransform rectTransform = flashPanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        flashImage = flashPanel.AddComponent<Image>();
        flashImage.color = Color.clear;
        flashPanel.transform.SetAsFirstSibling();
    }
    public void FlashScreen(bool isFrozen)
    {
        if (isFrozen)
        {
            StopCoroutine(FlashEffect());
            return;
        }
        if (flashPanel != null)
        {
            hasTeleported = false;  // 플래그 초기화
            StartCoroutine(FlashEffect());
        }
        else
        {
            Debug.LogWarning("FlashPanel not found. Creating one now.");
            CreateFlashPanel();
            FlashScreen(isFrozen);
        }
    }

    private IEnumerator FlashEffect()
    {
        for (int i = 0; i < flashCount; i++)
        {
            float startTime = Time.time;
            float endTime = startTime + flashDuration / flashCount;

            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / (flashDuration / flashCount);
                float sinWave = Mathf.Sin(t * Mathf.PI);
                float alpha = Mathf.Min(((sinWave + 1f) / 2f) * 1.02f, 1.0f);
                flashImage.color = new Color(0, 0, 0, alpha * maxDarkness);

                // 마지막 깜박임의 중간 지점(가장 어두운 순간)에서 순간이동
                if (PhotonNetwork.IsMasterClient && !hasTeleported && i == flashCount - 1 && t >= 0.4f)
                {
                    int randomIndex = Random.Range(0, SecondBoss.SecondBossinstance.possibleTeleportPositions.Count);
                    pv.RPC("RPC_TeleportBoss", RpcTarget.AllViaServer, randomIndex);
                    hasTeleported = true;
                }

                yield return null;
            }
        }

        // 마지막 깜박임이 끝난 후 화면을 다시 밝게
        flashImage.color = Color.clear;
    }
    [PunRPC]
    void RPC_TeleportBoss(int randomIndex)
    {
        if (SecondBoss.instance != null)
        {
            SecondBoss.SecondBossinstance.Teleport(randomIndex);
            //SecondBoss.instance.bossEffect.Play();
        }
    }
}