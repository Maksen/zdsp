using UnityEngine;
using System.Collections;
using Zealot.Client.Entities;

/// <summary>
/// can be remove
/// </summary>
public class PlayerGhostTrigger : MonoBehaviour {


    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("loot"))
        {
           // GameInfo.gCombat.CheckLootStatus(other);
        }

    }
}
