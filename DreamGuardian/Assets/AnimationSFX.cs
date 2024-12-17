using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimationSFX : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Character character;

    [SerializeField]
    private PhotonView pv;
    private string[] liftableTags = { "Pot", "Pedestal", "Body", "Gun", "Capsule", "Sand", "Clay", "Wood", "WoodenPlank", "MindStone", "BluePrint", "Steel", "MeltedSteel", "ForgedSteel" };
    private Block block;

    private void FixedUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    if (character.target == null) return;
        //    String target = character.target.tag;
        //    Block block = character.target.GetComponent<Block>();
        //    if (character.holdingObject == null && ((block != null &&
        //        block.liftedItems.Count > 0 && Array.Exists(liftableTags, x => x == block.liftedItems.Peek().tag) || 
        //        (target == "SandBox" || target == "WoodBox" || target == "MindStoneMine" || target == "SteelBox" || target == "CapsuleBox"))))
        //    {
        //        MusicManager.Liftup();
        //    }
        //    else if (character.holdingObject != null)
        //    {
        //        String name = character.holdingObject.tag;
        //        if (target == "Furnace" && (name == "Steel" || name == "Clay" || name == "MindStone"))
        //        {
        //            MusicManager.Boil();
        //        }
        //        else if (target == "Furnace" || target == "Wall" || target == "TrashCan" || target == "CraftTable" || target == "WorkTable" || target == "Mixer")
        //        {
        //            MusicManager.Liftdown();
        //        }
        //    }
        //}
    }

    void Craftsound()
    {
        if (character.target != null)
        {
            block = character.target.GetComponent<Block>();
            if (block != null && block.liftedItems.Count > 0 &&
                (block.liftedItems.Peek().tag == "MeltedSteel" ||
                block.liftedItems.Peek().tag == "ForgedSteel"))
            {
                MusicManager.Steel();
            }
            else
            {
                MusicManager.MakeTreesound();
            }

            if (character.target.tag == "CraftTable")
                pv.RPC("RPCCraftSound", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPCCraftSound()
    {
        MusicManager.Craftsound();
    }
    void MIxsound()
    {
        MusicManager.MIxsound();
    }
}
