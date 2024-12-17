using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEditor.Rendering;

public abstract class Boss : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
    protected Animator anim;
    protected bool isFinished;
    public bool isFrozen;
    public GameObject IceObject;
    public static Boss instance;
    public int iceStack;

    protected virtual void Awake()
    {
        if(PlayerPrefs.GetString("checkedLevel") == "normal"){
            maxHealth = 8000f;
        }else if(PlayerPrefs.GetString("checkedLevel") == "hard"){
            maxHealth = 13000f;
        }else if(PlayerPrefs.GetString("checkedLevel") == "crazy"){
            maxHealth = 20000f;
        }
        instance = this;

        currentHealth = maxHealth;
        transform.localScale = new Vector3(5f, 7f, 5f);
        anim = GetComponent<Animator>();
        isFinished = false;
        isFrozen = false;
        iceStack = 0;
    }

    protected virtual void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (GameManager.instance.staticBossCount == 1)
        {
            SpawnManager.instance.pv.RPC("RPCUpdateHealthBar", RpcTarget.AllViaServer, currentHealth, maxHealth);
        }
        else if (GameManager.instance.staticBossCount == 2)
        {
            int bossNum = 2;
            if (this.gameObject.name == "NewBoss(Clone)") bossNum = 3;
            SpawnManager2.instance.pv.RPC("FRPCUpdateHealthBar2", RpcTarget.AllViaServer, currentHealth, maxHealth, bossNum);
        }
    }

    public virtual void TakeDamage(float damage, string effectType = "default")
    {
        if (isFinished) return;
        currentHealth -= damage;
        if (currentHealth <= 0 && !isFinished)
        {
            isFinished = true;
            Die();
        }
        if (effectType == "frozen")
        {
            iceStack++;
            if(iceStack >= 8)
            {
                iceStack = 0;
                Freeze();
            }
        }
        if (!PhotonNetwork.IsMasterClient) return;
        if (GameManager.instance.staticBossCount == 1)
        {
            SpawnManager.instance.pv.RPC("RPCUpdateHealthBar", RpcTarget.AllViaServer, currentHealth, maxHealth);
        }
        else if(GameManager.instance.staticBossCount == 2)
        {
            int bossNum = 2;
            if (this.gameObject.name == "NewBoss(Clone)") bossNum = 3;
            SpawnManager2.instance.pv.RPC("FRPCUpdateHealthBar2", RpcTarget.AllViaServer, currentHealth, maxHealth, bossNum);
        }
    }

    protected virtual void Die()
    {
        GameManager.instance.bossCount--;
        this.gameObject.SetActive(false);
        if (GameManager.instance.bossCount > 0) return;

        StopAllCoroutines();
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.gameClear();
        }
    }

    protected virtual void Freeze()
    {
        StartCoroutine(Frozen());
    }

    protected virtual IEnumerator Frozen()
    {
        if (IceObject != null)
        {
            isFrozen = true;
            anim.SetBool("isFrozen", true);
            IceObject.SetActive(true);
            yield return new WaitForSeconds(2);
            isFrozen = false;
            anim.SetBool("isFrozen", false);
            IceObject.SetActive(false);
            Melt();
        }
    }

    protected virtual void Melt()
    {
        //StartCoroutine(Frozen());
    }
}