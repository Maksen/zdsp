using UnityEngine;
using UnityEngine.UI;

public class UI_QuestListData : MonoBehaviour
{
    [SerializeField]
    Text Name;
    
    public void Init(string name)
    {
        Name.text = name;
    }
}
