using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class NetworkHandler
{
    public static string FileServerIp = "http://piliqstg.zealotdigital.com.tw/";
    public static string PatchServerIp = "";
    public static string APKPatchServerIp = "";
    public static string BillingServerIp = "";
    public static string DebugServerIp = "";
    public static string AnnouncementServerURL = "";

    public delegate void NetworkRequestDelegate(object response);
    public delegate void GetFileDelegate(Texture2D image);
    public delegate void AnnouncementDelegate(bool error, string text);

    private static JObject failedresult;

    static NetworkHandler()
    {
        failedresult = new JObject();
        failedresult.Add("result", 0);
    }

    public static IEnumerator SendWebRequestWithoutCookie(string ip, string router, Dictionary<string, string> parameters, NetworkRequestDelegate callback)
    {
        string url = string.Format("{0}/{1}", ip, router);
        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, string> kvp in parameters)
        {
            form.AddField(kvp.Key, kvp.Value);
        }

        WWW w = new WWW(url, form.data);
        yield return w;
        if (!string.IsNullOrEmpty(w.error))
        {
            Debug.LogFormat("Network Error: router = {0}, error = {1}", router, w.error);
            if (callback != null)
                callback(failedresult);
        }
        else
        {
            JObject jobject = JObject.Parse(w.text);
            if (callback != null)
                callback(jobject);
        }
    }

    public static IEnumerator GetImage(string filePath, GetFileDelegate callback)
    {
        string url = string.Format("{0}/{1}", FileServerIp, filePath);
        WWW w = new WWW(url);
        yield return w;
        if (!string.IsNullOrEmpty(w.error))
            Debug.LogFormat("Network Error: filePath = {0}, error = {1}", filePath, w.error);
        else
        {
            Texture2D image = new Texture2D(w.texture.width, w.texture.height);
            //image.wrapMode = TextureWrapMode.Clamp;
            //image.Compress(false);
            w.LoadImageIntoTexture(image);
            Object.DestroyImmediate(w.texture); //prevent memory leak from www
            callback(image);
        }
    }

    public static IEnumerator GetServerAnnouncement(AnnouncementDelegate callback)
    {
        string url = AnnouncementServerURL;
        WWW w = new WWW(url);
        yield return w;
        if (!string.IsNullOrEmpty(w.error))
        {
            Debug.LogFormat("Network Error: notification = {0}", w.error);
            callback(true, "");
        }
        else
            callback(false, w.text);
    }
}