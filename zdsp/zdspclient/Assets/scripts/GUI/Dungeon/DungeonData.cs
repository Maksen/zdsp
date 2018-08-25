using Kopio.JsonContracts;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public enum DungeonDataState : byte
{
    Open,
    Locked,
    NotOpen
} 

public class DungeonData : MonoBehaviour
{
    [SerializeField]
    Animator animator = null;

    [SerializeField]
    Image imgIcon = null;

    [SerializeField]
    Text txtDescription = null;

    public DungeonJson DungeonJson { get; private set; }

    DungeonDataState dungeonDataState = DungeonDataState.Open;

    void OnEnable()
    {
        SetDungeonState();
    }

    public void Init(DungeonJson dungeonJson, byte daysOpen, DungeonDataState state)
    {
        DungeonJson = dungeonJson;

        if (daysOpen != 0)
        {
            string localizedWeek = GUILocalizationRepo.localizedWeek;
            string openTime = dungeonJson.opentime, openHr = "", openMin = "";
            string closeTime = dungeonJson.closetime, closeHr = "", closeMin = "";
            bool timeAvail = false;
            if (!string.IsNullOrEmpty(openTime))
            {
                openHr = openTime.Substring(0, 2);
                openMin = openTime.Substring(2, 2);
                if (!string.IsNullOrEmpty(closeTime))
                {
                    closeHr = closeTime.Substring(0, 2);
                    closeMin = closeTime.Substring(2, 2);
                }
                timeAvail = true;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < 7; ++i)
            {
                if (GameUtils.IsBitSet(daysOpen, i))
                {
                    sb.AppendFormat("\n{0}{1}", localizedWeek, GUILocalizationRepo.GetLocalizedNumber(i));
                    if (timeAvail)
                        sb.AppendFormat(" {0}：{1} ~ {2}：{3}", openHr, openMin, closeHr, closeMin);
                }
            }
            if (GameUtils.IsBitSet(daysOpen, 0))
            {
                sb.AppendFormat("\n{0}{1}", localizedWeek, GUILocalizationRepo.localizedDay);
                if (timeAvail)
                    sb.AppendFormat(" {0}：{1} ~ {2}：{3}", openHr, openMin, closeHr, closeMin);
            }
            if (sb.Length > 0)
                sb.Remove(0, 1);

            txtDescription.text = sb.ToString();
        }

        dungeonDataState = state;
        SetDungeonState();
    }

    void SetDungeonState()
    {
        switch (dungeonDataState)
        {
            case DungeonDataState.Open: animator.Play("Dungeon_Enter"); break;
            case DungeonDataState.Locked: animator.Play("Dungeon_Locked"); break;
            case DungeonDataState.NotOpen: animator.Play("Dungeon_NotOpened"); break;
        }
    }

    public void OnClickOpenDungeonUI()
    {
        DungeonType dungeonType = DungeonJson.dungeontype;
        switch (dungeonType)
        {
            case DungeonType.Story:
                UIManager.OpenWindow(WindowType.DungeonStory);
                break;
        }
    }
}
