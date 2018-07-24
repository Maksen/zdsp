using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

/// <summary>
/// This class is used for PlayerLabelsMarker + DamageLabelsMarker in CombatHiearchy
/// </summary>
class HUD_LabelController : MonoBehaviour
{
    short playerIndex = 0;
    short lastBossIndex = 0;
    short lastPartyMemIndex = 0;
    short lastNPCIndex = 0;
    short lastEnemyIndex = 0; //Represents enemy in pvp
    short lastMonsterIndex = 0;
    short lastFriendlyIndex = 0;
    short lastNeutralPlayerIndex = 0;

    short numPartyMember = 0;
    short numOtherPlayer = 0;
    short numMonster = 0;
    short numEnemy = 0; //Represents enemy in pvp

    public short PartyMemberCount
    {
        get { return numPartyMember; }
    }
    public short OtherPlayerCount
    {
        get { return numOtherPlayer; }
    }
    public short MonsterCount
    {
        get { return numMonster; }
    }
    public short EnemyCount
    {
        get { return numEnemy; }
    }
    public short GetMaxMonsterCount()
    {
        return (short)GameConstantRepo.GetConstantInt("MaxMonsterHPBarCount", 10);
    }
    public bool isMonsterCountExceeded()
    {
        return numMonster >= GetMaxMonsterCount();
    }

    public void Start()
    {
        ResetLabelOrder();
    }

    public void AssignPlayerLabel(GameObject obj, Vector3 localpos)
    {
        HUD_PlayerLabel2 pl = obj.GetComponent<HUD_PlayerLabel2>();
        if (pl == null)
            return;

        //Update new local position
        obj.transform.localPosition = localpos;

        //Identify the type of the label and insert sibling index accordingly
        switch (pl.LabelType)
        {
            case LabelTypeEnum.Player:
            case LabelTypeEnum.Battleground_Player:
                obj.transform.SetSiblingIndex(playerIndex);
                break;
            case LabelTypeEnum.BossMonster:
                obj.transform.SetSiblingIndex(lastBossIndex);
                break;
            case LabelTypeEnum.PartyMember:
                obj.transform.SetSiblingIndex(lastPartyMemIndex);
                break;
            case LabelTypeEnum.NPC:
            case LabelTypeEnum.HurtNPC:
                obj.transform.SetSiblingIndex(lastNPCIndex);
                break;
            case LabelTypeEnum.Battleground_Enemy:
                obj.transform.SetSiblingIndex(lastEnemyIndex);
                break;
            case LabelTypeEnum.Monster:
            case LabelTypeEnum.HurtMonster:
                obj.transform.SetSiblingIndex(lastMonsterIndex);
                break;
            case LabelTypeEnum.Battleground_Ally:
                obj.transform.SetSiblingIndex(lastFriendlyIndex);
                break;
            case LabelTypeEnum.OtherPlayer:
            case LabelTypeEnum.SelectedOtherPlayer:
                obj.transform.SetSiblingIndex(lastNeutralPlayerIndex);
                break;
            default:
                Debug.LogError("HUD_LabelController: unknown label type!");
                break;
        }

        UpdateLabelOrder(pl.LabelType);
    }

    public void ReassignPlayerLabel(LabelTypeEnum oldtype, GameObject obj, Vector3 localpos)
    {
        //Update label
        DecrementLabelOrder(oldtype);

        //now assign the player label
        AssignPlayerLabel(obj, localpos);
    }

    public void ResetLabelOrder()
    {
        playerIndex = 0;
        lastBossIndex = 0;
        lastPartyMemIndex = 0;
        lastNPCIndex = 0;
        lastEnemyIndex = 0;
        lastMonsterIndex = 0;
        lastFriendlyIndex = 0;
        lastNeutralPlayerIndex = 0;

        numPartyMember = 0;
        numOtherPlayer = 0;
        numMonster = 0;
        numEnemy = 0;
    }

    public void DecrementLabelOrder(LabelTypeEnum t)
    {
        switch (t)
        {
            case LabelTypeEnum.Player:
            case LabelTypeEnum.Battleground_Player:
                playerIndex--;
                break;
            case LabelTypeEnum.BossMonster:
                playerIndex--;
                lastBossIndex--;
                break;
            case LabelTypeEnum.PartyMember:
                playerIndex--;
                lastBossIndex--;
                lastPartyMemIndex--;

                numPartyMember--;
                break;
            case LabelTypeEnum.NPC:
            case LabelTypeEnum.HurtNPC:
                playerIndex--;
                lastBossIndex--;
                lastPartyMemIndex--;
                lastNPCIndex--;
                break;
            case LabelTypeEnum.Battleground_Enemy:
                playerIndex--;
                lastBossIndex--;
                lastPartyMemIndex--;
                lastNPCIndex--;
                lastEnemyIndex--;

                numEnemy--;
                break;
            case LabelTypeEnum.Monster:
            case LabelTypeEnum.HurtMonster:
                playerIndex--;
                lastBossIndex--;
                lastPartyMemIndex--;
                lastNPCIndex--;
                lastEnemyIndex--;
                lastMonsterIndex--;

                numMonster--;
                break;
            case LabelTypeEnum.Battleground_Ally:
                playerIndex--;
                lastBossIndex--;
                lastPartyMemIndex--;
                lastNPCIndex--;
                lastEnemyIndex--;
                lastMonsterIndex--;
                lastFriendlyIndex--;
                break;
            case LabelTypeEnum.OtherPlayer:
            case LabelTypeEnum.SelectedOtherPlayer:
                playerIndex--;
                lastBossIndex--;
                lastPartyMemIndex--;
                lastNPCIndex--;
                lastEnemyIndex--;
                lastMonsterIndex--;
                lastFriendlyIndex--;
                lastNeutralPlayerIndex--;

                numOtherPlayer--;
                break;
        }
    }

    private void UpdateLabelOrder(LabelTypeEnum e)
    {
        switch (e)
        {
            case LabelTypeEnum.Player:
            case LabelTypeEnum.Battleground_Player:
                playerIndex++;
                break;
            case LabelTypeEnum.BossMonster:
                playerIndex++;
                lastBossIndex++;
                break;
            case LabelTypeEnum.PartyMember:
                playerIndex++;
                lastBossIndex++;
                lastPartyMemIndex++;

                numPartyMember++;
                break;
            case LabelTypeEnum.NPC:
            case LabelTypeEnum.HurtNPC:
                playerIndex++;
                lastBossIndex++;
                lastPartyMemIndex++;
                lastNPCIndex++;
                break;
            case LabelTypeEnum.Battleground_Enemy:
                playerIndex++;
                lastBossIndex++;
                lastPartyMemIndex++;
                lastNPCIndex++;
                lastEnemyIndex++;

                numEnemy++;
                break;
            case LabelTypeEnum.Monster:
            case LabelTypeEnum.HurtMonster:
                playerIndex++;
                lastBossIndex++;
                lastPartyMemIndex++;
                lastNPCIndex++;
                lastEnemyIndex++;
                lastMonsterIndex++;

                numMonster++;
                break;
            case LabelTypeEnum.Battleground_Ally:
                playerIndex++;
                lastBossIndex++;
                lastPartyMemIndex++;
                lastNPCIndex++;
                lastEnemyIndex++;
                lastMonsterIndex++;
                lastFriendlyIndex++;
                break;
            case LabelTypeEnum.OtherPlayer:
            case LabelTypeEnum.SelectedOtherPlayer:
                playerIndex++;
                lastBossIndex++;
                lastPartyMemIndex++;
                lastNPCIndex++;
                lastEnemyIndex++;
                lastMonsterIndex++;
                lastFriendlyIndex++;
                lastNeutralPlayerIndex++;

                numOtherPlayer++;
                break;
        }

        /*
        Debug.Log("PlayerIndex: " + playerIndex + "\n" +
                  "lastBossIndex: " + lastBossIndex + "\n" +
                  "lastPartyMemberIndex: " + lastPartyMemIndex + "\n" +
                  "lastNPCIndex: " + lastNPCIndex + "\n" +
                  "lastEnemyIndex: " + lastEnemyIndex + "\n" +
                  "lastMonsterIndex: " + lastMonsterIndex + "\n" +
                  "lastFriendlyIndex: " + lastFriendlyIndex + "\n" +
                  "lastNeutralPlayerIndex: " + lastNeutralPlayerIndex + "\n");
        */
    }
}

