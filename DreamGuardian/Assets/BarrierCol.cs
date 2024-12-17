using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class BarrierCol : MonoBehaviourPunCallbacks
{
    private Capsule capsule;
    private void Awake()
    {
        capsule = transform.parent.GetComponentInChildren<Capsule>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        MusicManager.Defense();
        DecreaseHp();
    }

    public void DecreaseHp()
    {
        PhotonView pv = transform.parent.GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.IsMasterClient)
        {
            pv.RPC("RPCPlayEffectETC2", RpcTarget.AllViaServer);
        }
    }
}
