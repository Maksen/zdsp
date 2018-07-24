using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class GameDownload : MonoSingleton<GameDownload>
{
    private string imageRootName = "Images/";

    public string ImageRootPath
    {
        get { return string.Format("{0}/{1}", Application.persistentDataPath, imageRootName); }
    }

    public string ImageFileServerUrl
    {
        get
        {
            if (NetworkHandler.FileServerIp != "")
                return NetworkHandler.FileServerIp + imageRootName;
            return "";
        }
    }

    private Dictionary<string, Sprite> imageContainer = new Dictionary<string, Sprite>();
    
    private class NotifyCallBack
    {
        public string path;
        public Action<Sprite> callback;
    }
    private List<NotifyCallBack> waitDownloads = new List<NotifyCallBack>();
    private bool isLoadIng = false;


    public void GetImage(string path, Action<Sprite> afterLoad_callback)
    {
        Sprite image;
        if (imageContainer.TryGetValue(path, out image) == false)
            StartDownloadImage(path, afterLoad_callback);
        else
            afterLoad_callback(image);
    }

    public void RemoveFile(string path)
    {
        if (imageContainer.ContainsKey(path))
            imageContainer.Remove(path);

        string imagePath = ImageRootPath + path;
        Debug.Log("GameDownload RemoveFile path:" + imagePath);
        if (File.Exists(imagePath))
        {
            File.Delete(imagePath);
            string dir = imagePath;
            while (true)
            {
                dir = Path.GetDirectoryName(dir);
                int count = Directory.GetFileSystemEntries(dir).Length;
                if (count > 0 || (dir + "/") == ImageRootPath)
                    break;
                else
                    Directory.Delete(dir);  //資料夾為空時刪除
            }
        }
    }

    public void Clear()
    {
        imageContainer.Clear();
    }

    private void StartDownloadImage(string path, Action<Sprite> afterLoad_callback)
    {
        if (isLoadIng == false)
        {
            isLoadIng = true;
            StartCoroutine(DownloadImage(path, afterLoad_callback));
        }
        else
        {
            var notify = new NotifyCallBack();
            notify.callback = afterLoad_callback;
            notify.path = path;
            waitDownloads.Add(notify);
        }
    }

    private void CheckNextDownload()
    {
        if (waitDownloads.Count == 0)
            isLoadIng = false;
        else
        {
            var notify = waitDownloads[0];
            waitDownloads.RemoveAt(0);
            if (imageContainer.ContainsKey(notify.path) == false)
                StartCoroutine(DownloadImage(notify.path, notify.callback));
            else
                CheckNextDownload();
        }
    }

    private IEnumerator DownloadImage(string path, Action<Sprite> afterLoad_callback)
    {
        if (ImageFileServerUrl == "")
        {
            Debug.Log("ImageFileServerUrl is null at GameDownload");
            yield break;
        }

        string imagePath = ImageRootPath + path;
        Sprite sprite = null;
        if (!File.Exists(imagePath))
        {
            string url = ImageFileServerUrl + path;
            Debug.Log(string.Format("URL: {0}", url));

            WWW www = new WWW(url);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
                Debug.LogErrorFormat("Downloaded image: {0}, error = {1}", path, www.error);
            else
            {
                string dir = Path.GetDirectoryName(imagePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(imagePath, www.bytes);

                Texture2D image = new Texture2D(www.texture.width, www.texture.height);
                //image.wrapMode = TextureWrapMode.Clamp;
                //image.Compress(false);
                www.LoadImageIntoTexture(image);
                UnityEngine.Object.DestroyImmediate(www.texture);
                Rect rect = new Rect(0, 0, image.width, image.height);
                sprite = Sprite.Create(image, rect, new Vector2(0, 0));
                Debug.Log(string.Format("Downloaded image: {0}, size={1} x {2}", path, image.width, image.height));
            }
        }
        else
        {
            sprite = LoadNewSprite(imagePath);
        }
        if (sprite != null)
            imageContainer.Add(path, sprite);
        afterLoad_callback(sprite);
        CheckNextDownload();
        yield return null;
    }


    private Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Texture2D SpriteTexture = LoadTexture(FilePath);
        //SpriteTexture.wrapMode = TextureWrapMode.Clamp;
        //SpriteTexture.Compress(false);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);
        Debug.Log(string.Format("Downloaded LoadNewSprite: {0}, size={1} x {2}", FilePath, SpriteTexture.width, SpriteTexture.height));
        return NewSprite;
    }

    private Texture2D LoadTexture(string FilePath)
    {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails
        Texture2D Tex2D;
        byte[] FileData;
        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);            // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))          // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                       // If data = readable -> return texture
        }
        return null;                                // Return null if load failed
    }

}

