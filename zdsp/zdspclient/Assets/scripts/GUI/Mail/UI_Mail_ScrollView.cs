using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_Mail_ScrollView : MonoBehaviour
{
    public ToggleGroup toggleGroup;

    public GameObject GameObject_MailData;

    private List<GameObject> lstGOMailData;

    void Awake()
    {
        lstGOMailData = new List<GameObject>();
    }

    void OnEnable()
    {
        List<MailData> lstMailData = MailInvController.GetMailDataList();

        PopulateScrollView(lstMailData);
    }

    void OnDestroy()
    {
        toggleGroup = null;

        GameObject_MailData = null;

        ClearGOMailDataList();

        lstGOMailData = null;
    }

    private void ClearGOMailDataList()
    {
        if (lstGOMailData == null)
        {
            return;
        }

        foreach (GameObject goMailData in lstGOMailData)
        {
            Destroy(goMailData);
            //DestroyImmediate(goMailData);
        }

        lstGOMailData.Clear();
    }

    private void PopulateScrollView(List<MailData> lstMailData)
    {
        ClearGOMailDataList();

        int mailIdx = 0;

        foreach (MailData mailData in lstMailData)
        {
            GameObject goMailDataClone = Instantiate(GameObject_MailData, gameObject.transform.position, Quaternion.identity) as GameObject;
            goMailDataClone.transform.SetParent(gameObject.transform, false);
            goMailDataClone.SetActive(true);

            UI_MailData uiMailData = goMailDataClone.GetComponent<UI_MailData>();
            uiMailData.SetMailData(mailIdx, mailData, toggleGroup);

            mailIdx++;

            lstGOMailData.Add(goMailDataClone);
        }

        //Open first mail
        if (lstGOMailData.Count > 0)
        {
            UI_MailData uiMailData = lstGOMailData[0].GetComponent<UI_MailData>();
            uiMailData.OnClickOpenMail();
        }
    }

    public void OnOpenMail(int mailIndex, bool isAttachmentTaken)
    {
        GameObject goMailData = lstGOMailData[mailIndex];
        UI_MailData uiMailData = goMailData.GetComponent<UI_MailData>();

        uiMailData.SetMailIcon(isAttachmentTaken, MailStatus.Read);
    }

    public void OnOpenAllMail()
    {
        foreach (GameObject goMailData in lstGOMailData)
        {
            UI_MailData uiMailData = goMailData.GetComponent<UI_MailData>();
            uiMailData.SetOpenMailIcon();
        }
    }

    public void OnSendMail(List<MailData> lstMailData)
    {
        PopulateScrollView(lstMailData);
    }

    public void OnDeleteMail(int mailIdx)
    {
        DestroyImmediate(lstGOMailData[mailIdx]);
        lstGOMailData.RemoveAt(mailIdx);

        int newMailIdx = 0;

        foreach (GameObject goMailData in lstGOMailData)
        {
            UI_MailData uiMailData = goMailData.GetComponent<UI_MailData>();

            uiMailData.SetMailIndex(newMailIdx);

            newMailIdx++;
        }
    }

    public void OnDeleteAllMail(List<MailData> lstMailData)
    {
        //Remove only mail whom are opened and without attachment
        for (int i = lstMailData.Count-1; i >= 0; --i)
        {
            if (lstMailData[i].isTaken)
            {
                DestroyImmediate(lstGOMailData[i]);
                lstGOMailData.RemoveAt(i);
            }
        }

        //Reset index on the mail
        for (int i = 0; i < lstGOMailData.Count; ++i)
        {
            var uiMailData = lstGOMailData[i].GetComponent<UI_MailData>();
            uiMailData.SetMailIndex(i);
        }
    }

    public void SetInteraction(bool flag)
    {
        foreach (GameObject goMailData in lstGOMailData)
        {
            UI_MailData uiMailData = goMailData.GetComponent<UI_MailData>();

            uiMailData.SetInteraction(flag);
        }
    }

    public int GetMailGOCount()
    {
        return lstGOMailData.Count;
    }
}