using System;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_MailData : MonoBehaviour
{
    public Sprite Sprite_MailOpen;
    public Sprite Sprite_MailClose;
    public Sprite Sprite_MailHasAttachment;

    public Text Text_MailName;
    public Text Text_MailDate;
    public Image Image_MailIcon;

    private int mailIndex;

    private Toggle togMailData;

    void Awake()
    {
        togMailData = gameObject.GetComponent<Toggle>();

        togMailData.onValueChanged.AddListener(value =>
        {
            if (value == true)
            {
                OnClickOpenMail();
            }
        });
    }

    void OnDestroy()
    {
        Sprite_MailOpen = null;
        Sprite_MailHasAttachment = null;

        Text_MailName = null;
        Text_MailDate = null;
        Image_MailIcon = null;

        togMailData = null;
    }

    public void OnClickOpenMail()
    {
        MailInvController.OpenMail(mailIndex);
    }

    public void SetMailData(int mailIndex, MailData mailData, ToggleGroup toggleGroup)
    {
        this.mailIndex = mailIndex;

        //GUILocalizationRepo guiRepo;
        TimeSpan ts = TimeSpan.FromTicks(mailData.expiryTicks - GameInfo.GetSynchronizedServerDT().Ticks);
        string sExpiry = GUILocalizationRepo.GetShortLocalizedTimeString(ts.TotalSeconds);
        //string sExpiry = "";
        //string sDay = "D";
        //string sHour = "H";
        //string sMin = "M";
        //System.TimeSpan dt = new System.TimeSpan(mailData.expiryTicks);
        //if (dt.TotalDays > 1.0)
        //    sExpiry = dt.Days.ToString() + sDay + dt.Hours.ToString() + sHour;
        //else
        //    sExpiry = dt.Hours.ToString() + sHour + dt.Minutes.ToString() + sMin;

        Text_MailName.text = ClientUtils.FormatString(MailRepo.GetInfoByName(mailData.mailName).subject, mailData.dicBodyParam);
        Text_MailDate.text = sExpiry.ToString();

        togMailData.group = toggleGroup;

        SetMailIcon(mailData.isTaken, mailData.mailStatus);
    }

    public void SetMailIcon(bool isAttachmentTaken, MailStatus ms)
    {
        if (!isAttachmentTaken)
        {
            Image_MailIcon.sprite = Sprite_MailHasAttachment;
        }
        else if (ms == MailStatus.Read)
        {
            Image_MailIcon.sprite = Sprite_MailOpen;
        }
        else
        {
            Image_MailIcon.sprite = Sprite_MailClose;
        }
    }

    public void SetOpenMailIcon()
    {
        Image_MailIcon.sprite = Sprite_MailOpen;
    }

    public void SetAttachmentMailIcon()
    {
        Image_MailIcon.sprite = Sprite_MailHasAttachment;
    }

    public void SetMailIndex(int idx)
    {
        mailIndex = idx;
    }

    public void SetInteraction(bool flag)
    {
        togMailData.interactable = flag;
    }
}
