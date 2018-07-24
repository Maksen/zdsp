using AssetBundles;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Net;

public enum PatchErrorCode
{
    None = 101,
    Unknown = 201,
    NotFound404 = 202,
    RecvFailure = 301,
    CouldNotResolveHost = 302,
    RETRResponse = 601,
}


public class PatchManager
{
    AssetBundleSizeInfo assetBundleSizeInfo = null;
    bool waitPatchDialog = false;

    long currentSize = 0;
    long downloadedSize = 0;
    long totalSize = 0;
    string strTotalSize = "";

    public IEnumerator StartPatch()
    {
#if USE_ASSETBUNDLE

        var downloadList = AssetBundleManager.GetDownloadList().ToArray();
        
        if (downloadList.Length > 0)
        {
            totalSize = 0;
            downloadedSize = 0;

            string progressText = "";

            yield return GetAssetBundleSizeInfo();
            if (assetBundleSizeInfo != null)
            {
                for (int i = 0; i < downloadList.Length; i++)
                {
                    string bundleName = downloadList[i];
                    long size = assetBundleSizeInfo.GetBundleSize(bundleName);
                    if (size > 0)
                    {
                        totalSize += size;
                    }
                    else
                        Debug.LogWarningFormat("[Patch Warning] {0} size is 0", bundleName);
                }
            }

            strTotalSize = ConvertBytesToMegabytes(totalSize).ToString("N2");

            ShowPatchWarning(strTotalSize);
            while (waitPatchDialog)
                yield return null;

            float count = 0;
            for (int i = 0; i < downloadList.Length; i++)
            {
                string bundleName = downloadList[i];

                currentSize = assetBundleSizeInfo.GetBundleSize(bundleName);

                yield return ProcessPatch(bundleName);

                AssetBundleManager.MarkAsUsed(bundleName);

                count++;
                float value = count / downloadList.Length;

                if (assetBundleSizeInfo != null)
                {
                    downloadedSize += currentSize;
                }

                if (assetBundleSizeInfo != null)
                    progressText = string.Format("{0} {1} MB / {2} MB", GameLoader.Instance.splashScreenSettings.loadingText, ConvertBytesToMegabytes(downloadedSize).ToString("N2"), strTotalSize);
                else
                    progressText = string.Format("{0} {1} %", GameLoader.Instance.splashScreenSettings.loadingText, value);

                GameLoader.Instance.splashScreen.SetDescriptionText(progressText);
                //GameLoader.Instance.splashScreen.SetDownloadProgress(value);
            }

        }
        else
        {
            GameLoader.Instance.splashScreen.SetDescriptionText("");
        }

        //AssetBundleManager.MarkAllBundleAsUsed();
#else
        GameLoader.Instance.splashScreen.SetDescriptionText("");
#endif
        yield return null;

    }

    IEnumerator ProcessPatch(string bundleName)
    {
        bool isDone = false;
        
        while (!isDone)
        {
            if (!waitPatchDialog)
            {
                var patchRequest = AssetBundleManager.PatchBundle(bundleName);
                if (patchRequest != null)
                {
                    patchRequest.SetProgress = DownloadedProgress;

                    yield return patchRequest;

                    if (patchRequest.HasDownloadError())
                    {
                        var errorCode = GetErrorCode(patchRequest.GetError());
                        ShowPatchError((int)errorCode);
                        isDone = true;
                    }
                    else
                    {
                        //unload patch download
                        AssetBundleManager.UnloadAssetBundle(bundleName);

                        isDone = true;
                    }
                }
            }
            yield return null;
        }
    }

    public void DownloadedProgress(float value)
    {
        if (value > 0f && value < 1f)
        {
            long currProgress = downloadedSize + (long)(currentSize * value);
            string progressText = string.Format("{0} {1} MB / {2} MB", GameLoader.Instance.splashScreenSettings.loadingText, ConvertBytesToMegabytes(currProgress).ToString("N2"), strTotalSize);
            GameLoader.Instance.splashScreen.SetDescriptionText(progressText);
            GameLoader.Instance.splashScreen.SetDownloadProgress((float)currProgress / (float)totalSize);
        }
    }

    void ShowPatchError(int errorcode)
    {
        waitPatchDialog = true;
        GameLoader.Instance.splashScreen.ShowResumePatchDialog(true, errorcode, OnPatchDialogCallback);
    }

    void ShowPatchWarning(string totalSize)
    {
#if !UNITY_EDITOR
        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            waitPatchDialog = true;
            GameLoader.Instance.splashScreen.ShowPatchWarningDialog(true, totalSize, OnPatchDialogCallback);
        }
#endif
    }

    /// <summary>
    /// callback from patch warning dialog ok button
    /// </summary>
    public void OnPatchDialogCallback()
    {
        waitPatchDialog = false;
    }

    IEnumerator GetAssetBundleSizeInfo()
    {
        string baseURL = AssetBundleManager.BaseDownloadingURL;
        string fileURL = System.IO.Path.Combine(baseURL, AssetBundleSizeInfo.FileName);

        WWW www = new WWW(fileURL);
        if (www != null)
        {
            yield return www;

            string json = www.text;
            if (!string.IsNullOrEmpty(json))
            {
                assetBundleSizeInfo = AssetBundleSizeInfo.Deserialize(json);
            }
        }
        else
            yield return null;
    }

    /*
    /// <summary>
    /// Get content size from HTTP header. Does not work with ftp
    /// </summary>
    long GetHTTPBundleContentSize(string bundleName)
    {
        string url = AssetBundleManager.BaseDownloadingURL + bundleName;
        WebRequest req = HttpWebRequest.Create(url);
        req.Method = "HEAD";
        long ContentLength;
        using (System.Net.WebResponse resp = req.GetResponse())
        {
            long.TryParse(resp.ContentLength.ToString(), out ContentLength);
        }
        return ContentLength;
    }
    */

    static double ConvertBytesToMegabytes(long bytes)
    {
        return (bytes / 1024f) / 1024f;
    }

    public static PatchErrorCode GetErrorCode(string error)
    {
        var errorString = error.ToLower();
        if (!string.IsNullOrEmpty(errorString))
        {
            if (errorString.Contains("404 not found"))
                return PatchErrorCode.NotFound404;
            else if (errorString.Contains("recv failure"))
                return PatchErrorCode.RecvFailure;
            else if (errorString.Contains("could not resolve host"))
                return PatchErrorCode.CouldNotResolveHost;
            else if (errorString.Contains("retr response"))
                return PatchErrorCode.RETRResponse;
            else
                return PatchErrorCode.Unknown;
        }
        else
            return PatchErrorCode.None;
    }
}