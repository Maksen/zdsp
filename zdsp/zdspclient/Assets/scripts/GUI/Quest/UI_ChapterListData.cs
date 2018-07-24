using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_ChapterListData : MonoBehaviour
{
    [SerializeField]
    Image Background;

    [SerializeField]
    Text Name;

    [SerializeField]
    Text Progress;
   
    public void Init(string background, string name, string progress)
    {
        ClientUtils.LoadIconAsync(background, UpdateBackground);
        Name.text = name;
        Progress.text = progress;
    }

    private void UpdateBackground(Sprite sprite)
    {
        Background.sprite = sprite;
    }
}
