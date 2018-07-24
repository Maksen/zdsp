using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class AnnouncementItem
{
    public string title;
    public string date;
    public string content;
    public int priority;

    public AnnouncementItem(string title, string date, string content, int priority)
    {
        this.title = title;
        this.date = date;
        this.content = content;
        this.priority = priority;
    }
}

public class Announcement
{
    public string date;
    public List<AnnouncementItem> announcements;

    public Announcement()
    {
        announcements = new List<AnnouncementItem>();
    }
}

public static class AnnouncementUtils
{
    public static Announcement ParseAnnouncement(string text)
    {
        Announcement announcement = new Announcement();
        List<AnnouncementItem> announcementList = new List<AnnouncementItem>();
        JObject jobject = JObject.Parse(text);
        JToken token;
        if (jobject != null)
        {
            JToken date;
            if (jobject.TryGetValue("date", out date))
                announcement.date = date.ToString();
            if (jobject.TryGetValue("content", out token))
            {
                string content = token.ToString();
                string[] announces = content.Split(new string[] { "_Ann_Title:" }, StringSplitOptions.None);
                int announceTotal = announces.Length;
                if (announceTotal == 0)
                    return announcement;
                for (int index = 1; index < announceTotal; index++)
                {
                    string announceDetail = announces[index];
                    string[] announceDetailArray = announceDetail.Split(new string[] { "_Ann_Date:" }, StringSplitOptions.None);
                    if (announceDetailArray.Length == 2)
                    {
                        string title = announceDetailArray[0].Replace('^', '<').Replace('~', '>').TrimEnd('\n', '\r');
                        announceDetailArray = announceDetailArray[1].Split(new string[] { "_Ann_Priority:" }, StringSplitOptions.None);
                        if (announceDetailArray.Length == 2)
                        {
                            string _data = announceDetailArray[0];
                            announceDetailArray = announceDetailArray[1].Split(new string[] { "_Ann_Content:" }, StringSplitOptions.None);
                            if (announceDetailArray.Length == 2)
                            {
                                int priority = int.Parse(announceDetailArray[0]);
                                announcementList.Add(new AnnouncementItem(title, _data, announceDetailArray[1].Replace('^', '<').Replace('~', '>').TrimEnd('\n', '\r'), priority));
                            }
                        } 
                    }
                }
                announcementList = announcementList.OrderBy(x => x.priority).ToList();
                announcement.announcements = announcementList;
            }
        }
        return announcement;
    }
}