using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Zealot.Repository;

public enum ChartDirection
{
    Top = 0,
    Bottom = 1,
    Left = 2,
    Right = 3,
    None =4,
}

public class ChartInfo
{
    public int Column;
    public int Row;

    public ChartInfo(string[] xy)
    {
        Column = int.Parse(xy[0]);
        Row = int.Parse(xy[1]);
    }
}

public class UI_ChartData : MonoBehaviour
{
    [SerializeField]
    GameObject Toggle1;

    [SerializeField]
    GameObject Toggle2;

    [SerializeField]
    GameObject Toggle3;

    [SerializeField]
    GameObject Toggle4;

    [SerializeField]
    GameObject Toggle5;

    [SerializeField]
    GameObject LineObject;

    [SerializeField]
    Transform LineContent;

    private UI_DestinyQuest mParent;
    private List<QuestDestinyJson> mDestinyJsons;
    private List<GameObject> mLineObjects;
    private QuestClientController mQuestController;

    public void Init(List<QuestDestinyJson> destinies, ToggleGroup toggleGroup, UI_DestinyQuest parent, QuestClientController questcontroller)
    {
        mParent = parent;
        mDestinyJsons = destinies;
        mQuestController = questcontroller;
        mLineObjects = new List<GameObject>();
        for (int i = 1; i <= 5; i++)
        {
            QuestDestinyJson destinyJson = null;
            foreach (QuestDestinyJson destiny in destinies)
            {
                if (destiny.uirow == i)
                {
                    destinyJson = destiny;
                }
            }

            GameObject toggle = GetToggle(i);
            if (destinyJson != null)
            {
                toggle.SetActive(true);
                toggle.GetComponent<UI_ChartToggleData>().Init(destinyJson, parent);
                toggle.GetComponent<Toggle>().group = toggleGroup;
            }
            else
            {
                toggle.SetActive(false);
                toggle.GetComponent<Toggle>().group = null;
            }
        }
    }

    public void GenerateLine()
    {
        foreach (QuestDestinyJson destiny in mDestinyJsons)
        {
            List<ChartInfo> nextdatas = GetNextBlockData(destiny.next);
            foreach(ChartInfo nextdata in nextdatas)
            {
                ChartDirection direction;
                ChartDirection invertdirection;
                GetDirection(destiny.uicolumn, destiny.uirow, nextdata.Column, nextdata.Row, out direction, out invertdirection);

                bool specialline = false;
                QuestJson questJson = QuestRepo.GetNextBlockQuestJson(destiny.groupid, destiny.uirow, destiny.uicolumn);
                if (questJson != null)
                {
                    specialline = mQuestController.IsQuestInProgressOrUnlockOrCompleted(questJson);
                }

                Vector2 startpoint;
                Vector2 endpoint;
                mParent.GetPoint(destiny.uicolumn, destiny.uirow, direction, nextdata.Column, nextdata.Row, invertdirection, out startpoint, out endpoint);
                GameObject newline = Instantiate(LineObject);
                newline.transform.SetParent(LineContent, false);
                newline.GetComponent<RectTransform>().anchoredPosition = startpoint;
                newline.GetComponent<UILineRenderer>().Points[0].x = 0;
                newline.GetComponent<UILineRenderer>().Points[0].y = 0;
                newline.GetComponent<UILineRenderer>().Points[1].x = endpoint.x;
                newline.GetComponent<UILineRenderer>().Points[1].y = endpoint.y;
                if (specialline)
                {
                    newline.GetComponent<UILineRenderer>().LineThickness = 20;
                    newline.GetComponent<UILineRenderer>().color = Color.cyan;
                }
                else
                {
                    newline.GetComponent<UILineRenderer>().LineThickness = 5;
                    newline.GetComponent<UILineRenderer>().color = Color.white;
                }
                mLineObjects.Add(newline);
            }
        }
    }

    public void ClearLineObject()
    {
        foreach(GameObject obj in mLineObjects)
        {
            Destroy(obj);
        }
    }

    private List<ChartInfo> GetNextBlockData(string data)
    {
        List<ChartInfo> result = new List<ChartInfo>();
        string[] list = data.Split(',');
        foreach (string listdata in list)
        {
            if (!string.IsNullOrEmpty(listdata))
            {
                string[] xy = listdata.Split('|');
                if (xy.Length >= 2)
                {
                    ChartInfo info = new ChartInfo(xy);
                    result.Add(info);
                }
            }
        }
        return result;
    }

    private GameObject GetToggle(int row)
    {
        if (row == 1)
        {
            return Toggle1;
        }
        else if (row == 2)
        {
            return Toggle2;
        }
        else if (row == 3)
        {
            return Toggle3;
        }
        else if (row == 4)
        {
            return Toggle4;
        }
        else
        {
            return Toggle5;
        }
    }

    public Vector2 GetPoint(int row, ChartDirection direction)
    {
        GameObject toggle = GetToggle(row);
        return toggle.GetComponent<UI_ChartToggleData>().GetLinkPoint(direction, GetComponent<RectTransform>());
    }

    private void GetDirection(int startX, int startY, int endX, int endY, out ChartDirection direction, out ChartDirection invertdirection)
    {
        if (startX < endX)
        {
            direction = ChartDirection.Right;
            invertdirection = ChartDirection.Left;
        }
        else if (startX > endX)
        {
            direction = ChartDirection.Left;
            invertdirection = ChartDirection.Right;
        }
        else
        {
            if (startY < endY)
            {
                direction = ChartDirection.Bottom;
                invertdirection = ChartDirection.Top;
            }
            else if (startY > endY)
            {
                direction = ChartDirection.Top;
                invertdirection = ChartDirection.Bottom;
            }
            else
            {
                direction = ChartDirection.None;
                invertdirection = ChartDirection.None;
            }
        }
    }
}
