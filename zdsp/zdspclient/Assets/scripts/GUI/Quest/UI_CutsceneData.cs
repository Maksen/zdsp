using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;

public class UI_CutsceneData : MonoBehaviour
{
    [SerializeField]
    Image Icon;

    [SerializeField]
    Text Name;

    [SerializeField]
    Text Description;

    private string mPath;

    public void Init(WonderfulJson wonderfulJson, bool unlocked)
    {
        GetComponent<Button>().enabled = unlocked;
        ClientUtils.LoadIconAsync(wonderfulJson.background, UpdateBackground);
        Name.text = wonderfulJson.name;
        Description.text = wonderfulJson.description;
        mPath = wonderfulJson.path;
    }

    private void UpdateBackground(Sprite sprite)
    {
        Icon.sprite = sprite;
    }

    public void OnClickPlay()
    {

    }
}
