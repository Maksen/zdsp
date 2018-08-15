using UnityEngine;
using UnityEngine.UI;

public class UI_PhotoClueMessage : MonoBehaviour
{
    [SerializeField]
    Image Photo;

    public void Init(string path)
    {
        ClientUtils.LoadIconAsync(path, OnImageLoaded);
    }

    public void OnImageLoaded(Sprite sprite)
    {
        Photo.sprite = sprite;
    }

    public void Clean()
    {
        Photo.sprite = null;
    }
}
