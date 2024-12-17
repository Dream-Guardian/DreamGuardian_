using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int colliderPhotonViewID = other.GetComponent<PhotonView>().ViewID;
            GetComponent<PhotonView>().RPC("elevatorTrigger", RpcTarget.All, colliderPhotonViewID, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int colliderPhotonViewID = other.GetComponent<PhotonView>().ViewID;
            GetComponent<PhotonView>().RPC("elevatorTrigger", RpcTarget.All, colliderPhotonViewID, false);
        }
    }

    [PunRPC]
    public void elevatorTrigger(int colliderPhotonViewID, bool value)
    {
        PhotonView collidedObjectView = PhotonView.Find(colliderPhotonViewID);
        Collider other = collidedObjectView.GetComponent<Collider>();

        if (value) other.transform.SetParent(transform);
        else other.transform.SetParent(null);
    }
}