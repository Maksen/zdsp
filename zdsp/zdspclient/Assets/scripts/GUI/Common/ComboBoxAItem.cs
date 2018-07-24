using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ComboBoxAItem : MonoBehaviour
{
    [SerializeField]
    Text displayText;

    private Button button;
    private string _value;
    private int _index;

    public string Text
    {
        get { return displayText.text; }
        set { displayText.text = value; }
    }

    public string Value
    {
        get { return _value; }
        set { _value = value; }
    }

    public int Index
    {
        get { return _index; }
        set { _index = value; }
    }

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Init(string text, int index, string value, UnityAction onSelect)
    {
        if (!string.IsNullOrEmpty(text))
            Text = text;
        Index = index;
        Value = value;
        if (button != null)
            button.onClick.AddListener(onSelect);
        else
            Debug.LogWarning("ComboBoxAItem button not found! Item cannot be selected.");
    }
}
