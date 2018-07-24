using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ComboBoxA : MonoBehaviour
{
    [SerializeField] Transform childContent;
    [SerializeField] Text titleText;

    [SerializeField]
    [Tooltip("Prefab to be used for item in dropdown list")]
    GameObject comboBoxItem;

    private Toggle toggle;

    private int selectedIndex = -1;
    public int SelectedIndex
    {
        get { return selectedIndex; }
        set
        {
            selectedIndex = value;
            selectedValue = ItemList[value].Value;
            SelectedItem = ItemList[value];
            titleText.text = SelectedItem.Text;
        }
    }

    private string selectedValue;

    public string SelectedValue
    {
        get { return selectedValue; }
        set
        {
            selectedValue = value;
            selectedIndex = ItemList.FindIndex(x => x.Value == value);
            SelectedItem = ItemList[selectedIndex];
            titleText.text = SelectedItem.Text;
        }
    }

    public ComboBoxAItem SelectedItem { get; private set; }

    public bool SelectFirstItemOnEnable = false;

    [System.Serializable]
    public class SelectionChangedEvent : UnityEvent<int>
    {
    }

    // fires when item is changed;
    public SelectionChangedEvent OnSelectionChanged;

    private List<ComboBoxAItem> ItemList = new List<ComboBoxAItem>();

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        StartCoroutine(LateOnEnable());
    }

    private IEnumerator LateOnEnable()  // wait 1 frame so will call after Start
    {
        yield return null;

        if (ItemList.Count > 0 && SelectFirstItemOnEnable)
            OnSelectItem(0);
    }

    private void Start()
    {
        for (int i = 0; i < childContent.childCount; i++)
        {
            ComboBoxAItem item = childContent.GetChild(i).GetComponent<ComboBoxAItem>();
            if (item == null)
                Debug.LogError("ComboBox item do not have ComboBoxAItem script attached!");
            else if (!ItemList.Contains(item))
            {
                int itemIndex = i;
                item.Init("", itemIndex, itemIndex.ToString(), () => OnSelectItem(itemIndex));
                ItemList.Add(item);
            }
        }
    }

    public void AddItem(string text, string value)
    {
        GameObject child = Instantiate(comboBoxItem) as GameObject;
        child.transform.SetParent(childContent, false);
        ComboBoxAItem item = child.GetComponent<ComboBoxAItem>();
        int index = ItemList.Count;
        if (item != null)
        {
            item.Init(text, index, value, () => OnSelectItem(index));
            ItemList.Add(item);
        }
        else
            Debug.LogError("ComboBox item do not have ComboBoxAItem script attached!");
    }

    private void OnSelectItem(int index)
    {
        if (index != SelectedIndex)
        {
            //print("current index: " + SelectedIndex + " /on select index: " + index);
            SelectedIndex = index;
            if (OnSelectionChanged != null)
                OnSelectionChanged.Invoke(index);
        }

        HideDropDown();
    }

    private void OnDisable()
    {
        HideDropDown();
    }

    public void HideDropDown()
    {
        if (toggle.isOn)
            toggle.isOn = false;
    }

    public void ClearItemList()
    {
        ItemList.Clear();
        for (int i = childContent.childCount - 1; i >= 0; --i)
            Destroy(childContent.GetChild(i).gameObject);
        childContent.DetachChildren();
    }

    public void ResetSelected()
    {
        selectedIndex = -1;
        selectedValue = "";
        SelectedItem = null;
    }
}