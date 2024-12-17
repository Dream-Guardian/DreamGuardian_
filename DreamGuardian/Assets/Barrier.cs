using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Barrier : MonoBehaviourPunCallbacks
{
    public GameObject barrier;
    public Gun gun;
    private PhotonView pv;
    private Capsule capsule;

    override public void OnEnable()
    {
        base.OnEnable();
        if (gun.isTurretReady)
        {
            capsule = transform.parent.GetComponentInChildren<Capsule>();
            if(capsule != null && capsule.capsuleHP > 0)
                barrier.SetActive(true);
        }
        else if(!gun.isTurretReady)
            barrier.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (capsule != null && capsule.capsuleHP <= 0)
            barrier.SetActive(false);
        else if (capsule != null)
            barrier.SetActive(true);
    }
    override public void OnDisable()
    {
        base.OnDisable();
        barrier.SetActive(false);
    }

    [PunRPC]
    private void RPCPlayEffectETC2()
    {
        if (capsule != null)
            capsule.DecreaseHp(10f);
    }
}
