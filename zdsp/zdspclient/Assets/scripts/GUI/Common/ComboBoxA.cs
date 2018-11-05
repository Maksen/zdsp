using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ComboBoxA : MonoBehaviour
{
    [SerializeField] Transform childContent;
    [SerializeField] Text titleText;

    [SerializeField]
    [Tooltip("Prefab to be used for item in dropdown list")]
    GameObject comboBoxItem;

    private Toggle toggle;
    private List<GameObject> raycastTargetObjects = new List<GameObject>();
    private List<RaycastResult> raycastHitsCache = new List<RaycastResult>();

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
            int foundIndex = ItemList.FindIndex(x => x.Value == value);
            if (foundIndex == -1)
                foundIndex = 0;
            selectedIndex = foundIndex;
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

        Image[] images = GetComponentsInChildren<Image>();
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].raycastTarget)
                raycastTargetObjects.Add(images[i].gameObject);
        }
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
        GameObject child = Instantiate(comboBoxItem);
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

    public int GetIndexByValue(string value)
    {
        int foundIndex = ItemList.FindIndex(x => x.Value == value);
        return foundIndex == -1 ? 0 : foundIndex;
    }

    private void Update()
    {
        if (!toggle.isOn)
            return;

        Vector2 pointerPosition = Vector2.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonUp(0))
            pointerPosition = Input.mousePosition;
#elif UNITY_IOS || UNITY_ANDROID
        int touchCount = Input.touchCount;
        for (int i = 0; i < touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Ended)
            {
                pointerPosition = Input.GetTouch(i).position;
                break;
            }
        }
#endif
        if (pointerPosition != Vector2.zero)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = pointerPosition;

            EventSystem.current.RaycastAll(eventData, raycastHitsCache);
            bool clickOutside = true;
            for (int i = 0; i < raycastHitsCache.Count; i++)
            {
                if (raycastTargetObjects.Contains(raycastHitsCache[i].gameObject))
                {
                    clickOutside = false;
                    break;
                }
            }
            if (clickOutside)
                HideDropDown();
        }
    }
}