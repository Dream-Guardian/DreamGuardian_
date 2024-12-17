using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;

public class SecondBoss : Boss
{
    public static SecondBoss SecondBossinstance;
    public float skillCooldown;
    public List<Vector3> possibleTeleportPositions;
    public float teleportDistance;
    public ParticleSystem bossEffect;
    public bool skillStart;
    public Coroutine skillCoroutine;
    protected override void Awake()
    {
        base.Awake();
        if (PlayerPrefs.GetString("checkedLevel") == "normal") maxHealth = 5000f;
        else if (PlayerPrefs.GetString("checkedLevel") == "hard") maxHealth = 8000f;
        else if (PlayerPrefs.GetString("checkedLevel") == "crazy") maxHealth = 13000f;
        currentHealth = maxHealth;
        SecondBossinstance = this;
        skillCooldown = 15f;
        teleportDistance = 4f;
        anim.SetBool("IsIdle", true);
        skillStart = false;
        bossEffect.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        InitializeTeleportPositions();
    }

    //private void OnEnable()
    //{
    //    bossEffect = GetComponentInChildren<ParticleSystem>();
    //}

    protected override void Start()
    {
        base.Start();
        skillCoroutine =  StartCoroutine(SkillRoutine());
    }

    protected override void Freeze()
    {
        StopCoroutine(skillCoroutine);
        base.Freeze();
    }

    protected override void Melt()
    {
        base.Melt();
        skillCoroutine = StartCoroutine(SkillRoutine());
    }

    void InitializeTeleportPositions()
    {
        possibleTeleportPositions = new List<Vector3>();
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");

        foreach (GameObject wall in walls)
        {
            Wall target = wall.GetComponent<Wall>();
            if (target.rotation == -1) continue;
            Vector3 wallPosition = wall.transform.position;
            Vector3 directionFromCenter = (wallPosition - Vector3.zero).normalized;

            Vector3 teleportPosition = wallPosition + directionFromCenter * teleportDistance;
            teleportPosition.y = 0;

            possibleTeleportPositions.Add(teleportPosition);
        }

        if (possibleTeleportPositions.Count == 0)
        {
            Debug.LogWarning("No walls found for teleportation!");
        }
    }

    IEnumerator SkillRoutine()
    {
        while (!isFinished)
        {
            //bossEffect.gameObject.SetActive(false);
            
            while (skillCooldown > 0)
            {
                yield return new WaitForSeconds(0.5f);
                skillCooldown -= 0.5f;
            }
            if (!isFrozen)
            {
                bossEffect.Play();
                Base.instance.TakeDamage(400f);
            }
            anim.SetBool("IsIdle", false);
            anim.SetBool("IsUseSkill", true);

            SpawnManager.instance.FlashScreen(isFrozen);
            skillCooldown = 15f;
            anim.SetBool("IsIdle", true);
            anim.SetBool("IsUseSkill", false);
            //bossEffect.gameObject.SetActive(true);

            //bossEffect.Play();

            // 파티클 시스템의 위치를 보스의 현재 위치로 설정
            //if (bossEffect != null)
            //{
            //    bossEffect.gameObject.SetActive(true);
            //    Debug.LogWarning("잘 나온듯?");
            //}
            //else
            //{
            //    Debug.LogWarning("보스 스킬 안들어옴!");
            //}

        }
    }

    public void Teleport(int randomIndex)
    {
        if (randomIndex >= 0 && randomIndex < possibleTeleportPositions.Count)
        {
            transform.position = possibleTeleportPositions[randomIndex];

            Vector3 closestWallDirection = (Vector3.zero - transform.position).normalized;
            closestWallDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(closestWallDirection);
        }
        else
        {
            Debug.LogWarning("Invalid teleport index!");
        }
    }
}