using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace UIWidgets
{
    [Serializable]
    public class OnTabChangeEventInt : UnityEvent<int>
    {
    }

    [Serializable]
    /// <summary>
    /// Tab.
    /// </summary>
    public class Tab
    {
        /// <summary>
        /// Text of button for this tab.
        /// </summary>
        public string Name;

        /// <summary>
        /// The tab object.
        /// </summary>
        public GameObject TabObject;

        public bool Activated = true;
    }

    [AddComponentMenu("UI/Tabs", 290)]
    /// <summary>
    /// Tabs.
    /// http://ilih.ru/images/unity-assets/UIWidgets/Tabs.png
    /// </summary>
    public class Tabs : MonoBehaviour
    {
        [SerializeField]
        /// <summary>
        /// The container for tab toggle buttons.
        /// </summary>
        public Transform Container;

        [SerializeField]
        /// <summary>
        /// The default tab button.
        /// </summary>
        public Button DefaultTabButton;

        [SerializeField]
        /// <summary>
        /// The active tab button.
        /// </summary>
        public Button ActiveTabButton;

        [SerializeField]
        Tab[] tabObjects = new Tab[] { };

        /// <summary>
        /// Gets or sets the tab objects.
        /// </summary>
        /// <value>The tab objects.</value>
        public Tab[] TabObjects
        {
            get
            {
                return tabObjects;
            }
            set
            {
                tabObjects = value;
                if (DefaultTabIdx < 0 || DefaultTabIdx >= tabObjects.Length)
                {
                    DefaultTabIdx = 0;
                }
                UpdateButtons();
                if (_initialized)
                {
                    SetSelectedTab(ResetIdxOnOpen ? DefaultTabIdx : 0);
                }
            }
        }

        [SerializeField]
        public int DefaultTabIdx = 0;

        [SerializeField]
        [Tooltip("If true will select default tab when open.")]
        public bool ResetIdxOnOpen = true;

        [SerializeField]
        [Tooltip("If true does not deactivate hidden tabs.")]
        public bool KeepTabsActive = false;

        public List<Button> defaultButtons = new List<Button>();
        public List<Button> activeButtons = new List<Button>();
        List<UnityAction> callbacks = new List<UnityAction>();

        public OnTabChangeEventInt onSelectedChange = new OnTabChangeEventInt();

        private int _selectedTab = 0;
        private bool _initialized = false;

        public void SetSelectedTab(int tab)
        {
            if (!_initialized)
            {
                defaultButtons = new List<Button>();
                activeButtons = new List<Button>();
                Init();
            }
            if (_selectedTab != tab)
            {
                onSelectedChange.Invoke(tab);
                _selectedTab = tab;
                UpdateSelection();
            }
        }

        public void SetSelectedTabByName(string name)
        {
            int index = Array.FindIndex(tabObjects, x => x.Name == name);
            if (index >= 0 && index < tabObjects.Length)
            {
                SetSelectedTab(index);
            }
        }

        void Awake()
        {
            //Debug.Log("Tabs Awake");
            Init();
        }

        void Init()
        {
            if (!_initialized)
            {
                if (Container == null)
                {
                    throw new NullReferenceException("Container is null. Set object of type GameObject to Container.");
                }
                if (DefaultTabButton == null)
                {
                    throw new NullReferenceException("DefaultTabButton is null. Set object of type GameObject to DefaultTabButton.");
                }
                if (ActiveTabButton == null)
                {
                    throw new NullReferenceException("ActiveTabButton is null. Set object of type GameObject to ActiveTabButton.");
                }
                DefaultTabButton.gameObject.SetActive(false);
                ActiveTabButton.gameObject.SetActive(false);
                if (DefaultTabIdx < 0 || DefaultTabIdx >= tabObjects.Length)
                {
                    DefaultTabIdx = 0;
                }
                _selectedTab = DefaultTabIdx;
                UpdateButtons();
                _initialized = true;
            }            
        }

        public void ForceInit()
        {
            Init();
        }

        void OnEnable()
        {
            if (ResetIdxOnOpen && _selectedTab != DefaultTabIdx)
            {
                _selectedTab = DefaultTabIdx;
                if (_initialized)
                {
                    UpdateSelection();
                }
            }
        }

        void UpdateButtons()
        {
            if (tabObjects.Length == 0)
            {
                throw new ArgumentException("TabObjects array is empty. Fill it.");
            }

            //remove callbacks
            defaultButtons.ForEach((x, index) => x.onClick.RemoveListener(callbacks[index]));
            callbacks.Clear();

            //update buttons
            CreateButtons();

            //set callbacks
            tabObjects.ToList().ForEach((x, index) =>
            {
                UnityAction callback = () => SetSelectedTab(index);
                callbacks.Add(callback);

                defaultButtons[index].onClick.AddListener(callbacks[index]);
            });
        }

        /// <summary>
        /// Selects the tab and update the avtive and default buttons.
        /// </summary>
        /// <param name="tabName">Tab name.</param>
        private void UpdateSelection()
        {
            if (_selectedTab < 0 || _selectedTab >= tabObjects.Length)
            {
                throw new ArgumentException(string.Format("Tab with index \"{0}\" not found.", _selectedTab));
            }
            if (KeepTabsActive)
            {
                tabObjects[_selectedTab].TabObject.transform.SetAsLastSibling();
            }
            else
            {
                tabObjects.ForEach(x => x.TabObject.SetActive(false));
                tabObjects[_selectedTab].TabObject.SetActive(true);
            }
            for (int i= 0;i < tabObjects.Length;i++)
                defaultButtons[i].gameObject.SetActive(tabObjects[i].Activated);
            defaultButtons[_selectedTab].gameObject.SetActive(false);

            activeButtons.ForEach(x => x.gameObject.SetActive(false));
            activeButtons[_selectedTab].gameObject.SetActive(true);
        }

        void CreateButtons()
        {
            if (KeepTabsActive)
            {
                tabObjects[_selectedTab].TabObject.transform.SetAsLastSibling();
            }
            else
            {
                tabObjects.ForEach(x => x.TabObject.SetActive(false));
                tabObjects[_selectedTab].TabObject.SetActive(true);
            }
            if (tabObjects.Length > defaultButtons.Count)
            {
                for (var i = defaultButtons.Count; i < tabObjects.Length; i++)
                {
                    var defaultButton = Instantiate(DefaultTabButton) as Button;
                    defaultButton.transform.SetParent(Container, false);
                    if (i == _selectedTab || !TabObjects[i].Activated) defaultButton.gameObject.SetActive(false);
                    else defaultButton.gameObject.SetActive(true);

                    Utilites.FixInstantiated(DefaultTabButton, defaultButton);

                    defaultButtons.Add(defaultButton);

                    var activeButton = Instantiate(ActiveTabButton) as Button;
                    activeButton.transform.SetParent(Container, false);
                    if (i == _selectedTab) activeButton.gameObject.SetActive(true);
                    else activeButton.gameObject.SetActive(false);

                    Utilites.FixInstantiated(ActiveTabButton, activeButton);

                    activeButtons.Add(activeButton);
                }
            }
            //del existing ui elements if necessary
            if (tabObjects.Length < defaultButtons.Count)
            {
                for (var i = defaultButtons.Count; i > tabObjects.Length; i--)
                {
                    Destroy(defaultButtons[i]);
                    Destroy(activeButtons[i]);

                    defaultButtons.RemoveAt(i);
                    activeButtons.RemoveAt(i);
                }
            }

            defaultButtons.ForEach(SetButtonName);
            activeButtons.ForEach(SetButtonName);            
        }

        void SetButtonName(Button button, int index)
        {
            Text[] text = button.GetComponentsInChildren<Text>(true);
            if (text.Length > 0)
            {
                text[0].text = tabObjects[index].Name;
            }
        }

        public int GetSelectedTab()
        {
            return _selectedTab;
        }

        public void SetTabActiveStatus(int idx, bool active)
        {
            if (tabObjects.Length < idx)
                return;
            tabObjects[idx].Activated = active;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/UI/Tabs", false, 1180)]
        static void CreateObject()
        {
            Utilites.CreateWidgetFromAsset("Tabs");
        }
#endif

        public void RefreshTabButtons()
        {
            for (var i = 0; i < defaultButtons.Count; i++)
            {
                if (tabObjects[i].Activated)
                {
                    if (i == _selectedTab)
                    {
                        defaultButtons[i].gameObject.SetActive(false);
                        activeButtons[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        defaultButtons[i].gameObject.SetActive(true);
                        activeButtons[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    defaultButtons[i].gameObject.SetActive(false);
                    activeButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void EnableHintIcon(int idx, bool status)
        {
            defaultButtons[idx].GetComponentInChildren<Animator>(true).gameObject.SetActive(status);
            activeButtons[idx].GetComponentInChildren<Animator>(true).gameObject.SetActive(status);
        }
    }
}