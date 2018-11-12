using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;


public class UI_SkillTree : BaseWindowBehaviour
{

    public class GameObjectPoolManager
    {
        public class PoolObject
        {
            public bool isFree = true;
            public GameObject obj;

            public PoolObject(GameObject objtype)
            {
                obj = GameObject.Instantiate(objtype, new Vector3(0, 0, 0), Quaternion.identity);
            }

            public PoolObject(GameObject objtype, Transform parent)
            {
                obj = GameObject.Instantiate(objtype, parent);
            }

            public int GetInstanceID()
            {
                return obj.GetInstanceID();
            }
        }

        private List<PoolObject> m_Pool;
        private Dictionary<int, int> vtable;
        private GameObject m_type;
        private GameObject m_ParentContainer;
        private Transform m_Master;
        private int m_PoolSize;
        private byte m_IsFilled;

        public GameObjectPoolManager(int size, Transform master, GameObject type, Transform parent = null)
        {
            m_Pool = new List<PoolObject>(size);
            vtable = new Dictionary<int, int>();
            m_type = type;
            m_PoolSize = size;
            if (parent == null)
                m_ParentContainer = new GameObject(m_type.name + "_SwimmingPool");
            else
                m_ParentContainer = parent.gameObject;

            FillPool();

            if (parent != null)
                m_ParentContainer = new GameObject(m_type.name + "_SwimmingPool");

            m_Master = master;
            m_ParentContainer.transform.SetParent(m_Master, false);
            m_ParentContainer.transform.localPosition = new Vector3(0, 0, 1);
            m_ParentContainer.transform.localScale = new Vector3(1, 1, 1);
        }

        private void FillPool()
        {
            for (int i = 0; i < m_PoolSize; ++i)
            {
                m_Pool.Add(new PoolObject(m_type, m_ParentContainer.transform));
                vtable[m_Pool[i].GetInstanceID()] = i;
                m_Pool[i].obj.SetActive(false);
            }
            m_IsFilled = 1;
        }

        public GameObject RequestObject()
        {
            if (m_Pool.Count == 0)
            {
            }
            for (int i = 0; i < m_Pool.Count; ++i)
            {
                if (m_Pool[i].isFree)
                {
                    m_Pool[i].isFree = false;
                    m_Pool[i].obj.SetActive(true);
                    return m_Pool[i].obj;
                }
            }

            int idx = m_Pool.Count;
            // not enough
            for (int j = 0; j < idx; ++j)
            {
                m_Pool.Add(new PoolObject(m_type, m_ParentContainer.transform));
                vtable[m_Pool[j + idx].GetInstanceID()] = j + idx;
            }

            m_Pool[idx].isFree = false;
            return m_Pool[idx].obj;
        }

        public void ReturnObject(GameObject obj)
        {
            PoolObject pObj = m_Pool[vtable[obj.GetInstanceID()]];
            pObj.isFree = true;
            pObj.obj.SetActive(false);
            pObj.obj.transform.SetParent(m_ParentContainer.transform, false);
        }

        public void EmptyPool()
        {
            m_IsFilled = 0;
            m_Pool.Clear();
            vtable.Clear();
        }

        public void CheckContainerIntegrity()
        {
            if (m_IsFilled == 0)
            {
                // need to refill
                FillPool();
            }
        }
    }


    [Header("Required Gameobjects")]
    public GameObject m_ButtonPrefab;

    public GameObject m_EmptyButtonPrefab;
    public GameObject m_RowPrefab;
    public GameObject m_EquipSkillIconPrefab;
    public GameObject m_SkillPanelContentRect; // Content Panel
    public GameObject m_JobButtonPrefab;

    [Header("Windows")]
    public Dictionary<string, UI_SkillTreeWindow> m_Windows = new Dictionary<string, UI_SkillTreeWindow>();

    [Header("Connectors")]
    public List<GameObject> m_Connectors;

    [Header("Skill Details Panel")]
    public UI_SkillExpandUI m_SkillDescriptor; // Skill Description Panel

    [Header("Job Selection Panel")]
    public GameObject m_JobPanel;

    public Text m_JobTitle;

    [Header("Select Skill / Equip Skill Panel")]
    public Transform m_SelectSkillContentRect;

    public UI_SkillSelectDropDownList m_SelectSkillDDL;

    [Header("Special Skill Panel")]
    public UI_SkillSpecialUI m_SpecialSkillPanel;

    [Header("Equip skill Panel")]
    public GameObject m_EquipSkillPanel;
    public UI_SkillEquipIconHelper m_EquipIconHelper;

    public Dictionary<JobType, List<GameObject>> m_SkillTreeCache = new Dictionary<JobType, List<GameObject>>();

    private GameObjectPoolManager m_ButtonPool;
    private GameObjectPoolManager m_EmptyButtonPool;
    private GameObjectPoolManager m_RowPool;

    private JobType m_DisplayType;

    private List<UI_SkillJobButton> m_JobButtons = new List<UI_SkillJobButton>();

    private enum m_Dirty : byte
    {
        Clean = 0,
        JobChange = 1 << 0,
        ScrollPanel = 1 << 1,
        PanelUpdated = 1 << 2,
    }

    private m_Dirty m_DirtyBit;

    private int m_NewJobSelected;

    private UI_SkillButton m_CurrentActive;
    //private List<UI_SkillSelectButton> m_EquipSkillInv;

    private UI_SkillButton[] m_SkillButtonChanged;

    // scrolling panel
    private float m_StartPos;

    private float m_FinalPos;
    private float m_Time = 0;

    [Header("Scrolling Ease")]
    public AnimationCurve m_TweenFunction;

    public float m_Speed = 1;

    [Header("Scrolling Arrows")]
    public GameObject m_Left;

    public GameObject m_Right;

    [Header("Bottom Scrolls")]
    public Button m_CloseEquip;

    public GameObject m_ScrollPanel;
    public UIWidgets.Spinner m_ScrollPanelGroup;

    [Header("Debug use variables")]
    public int m_EquippableSize;

    public int m_EquipSlotGroup = 1;
    public int m_AutoSlotGroup = 1;
    public float m_LastIconXPos;
    public UIWidgets.Spinner m_Spinner;

    private bool isInit = false;
    private bool isPlayerEquip = true;

    public override void OnRegister()
    {
        base.OnRegister();
    }

    public override void OnCloseWindow()
    {
        // update server on slot groups
        RPCFactory.NonCombatRPC.UpdateEquipSlots(m_EquipSlotGroup, m_AutoSlotGroup);

        //m_SelectSkillDDL.OnDeselected();
        //m_SelectSkillDDL.CloseUI();

        //CloseWindows();

        base.OnCloseWindow();
        //clean up

        m_SkillDescriptor.OnClosed();
        m_CurrentActive = null;
        m_SkillDescriptor.gameObject.SetActive(false);
        m_SpecialSkillPanel.gameObject.SetActive(false);
        
        //m_CloseEquip.gameObject.SetActive(false);
        m_ScrollPanel.SetActive(false);
    }

    public override void OnOpenWindow()
    {
        base.OnOpenWindow();

        if (!isInit)
        {
            Initialise();
        }
        else
        {
            foreach (var objectlist in m_SkillTreeCache)
            {
                foreach (var obj in objectlist.Value)
                {
                    foreach (Transform child in obj.transform)
                    {
                        UI_SkillButton ui = child.GetComponent<UI_SkillButton>();
                        if (ui != null)
                            ui.UpdateButton();
                    }
                }
            }
        }
        if (m_DisplayType != (JobType)GameInfo.gLocalPlayer.PlayerSynStats.jobsect)
        {
            if (m_SkillTreeCache.Count > 0)
                // off current job panel
                foreach (GameObject panel in m_SkillTreeCache[m_DisplayType])
                {
                    foreach (Transform obj in panel.transform)
                    {
                        UI_SkillButton btn = obj.GetComponent<UI_SkillButton>();
                        if (btn)
                            btn.m_Toggle.isOn = false;
                    }

                    panel.SetActive(false);
                }
        }

        m_DisplayType = (JobType)GameInfo.gLocalPlayer.PlayerSynStats.jobsect;
        bool isFound = false;
        foreach (UI_SkillJobButton job in m_JobButtons)
        {
            if (job.m_Jobtype == m_DisplayType)
            {
                job.m_Toggle.isOn = isFound = !isFound;
                m_NewJobSelected = job.m_ID;
                break;
            }
        }
        if (!isFound)
        {
            // class is new, construct it
            List<JobType> hist = JobSectRepo.GetJobHistoryToCurrent(m_DisplayType);
            hist.Reverse();
            foreach(JobType job in hist)
            {
                if (job == m_JobButtons[m_JobButtons.Count - 1].m_Jobtype) continue;
                GameObject button = GameObject.Instantiate(m_JobButtonPrefab, m_JobPanel.transform);
                UI_SkillJobButton jobbtn = button.GetComponent<UI_SkillJobButton>();
                m_JobButtons.Add(jobbtn);
                jobbtn.Init(job, m_JobButtons.Count - 1);
                jobbtn.AddListener(OnJobTypeValueChanged);
                m_NewJobSelected = jobbtn.m_ID;
            }
            foreach (UI_SkillJobButton job in m_JobButtons)
            {
                if (job.m_Jobtype == m_DisplayType)
                {
                    job.m_Toggle.isOn = true;
                    break;
                }
            }
        }
        m_DirtyBit |= m_Dirty.JobChange;
    }

    public void Initialise()
    {
        isInit = !isInit;
        m_ButtonPool = new GameObjectPoolManager(10, this.transform, m_ButtonPrefab);
        m_EmptyButtonPool = new GameObjectPoolManager(10, this.transform, m_EmptyButtonPrefab);
        m_RowPool = new GameObjectPoolManager(3, this.transform, m_RowPrefab);

        //foreach (Transform child in m_JobPanel.transform)
        //{
        //    m_JobButtons.Add(child.GetComponent<UI_SkillJobButton>());
        //    UI_SkillJobButton btn = m_JobButtons[m_JobButtons.Count - 1];
        //    btn.Init(m_JobButtons.Count - 1);
        //    btn.m_Toggle.onValueChanged.AddListener(delegate
        //    {
        //        OnJobTypeValueChanged(btn);
        //    });
        //}
        m_DisplayType = (JobType)GameInfo.gLocalPlayer.PlayerSynStats.jobsect;

        List<JobType> hist = JobSectRepo.GetJobHistoryToCurrent(m_DisplayType);
        hist.Reverse();
        foreach(JobType job in hist)
        {
            GameObject button = GameObject.Instantiate(m_JobButtonPrefab, m_JobPanel.transform);
            UI_SkillJobButton jobbtn = button.GetComponent<UI_SkillJobButton>();
            m_JobButtons.Add(jobbtn);
            jobbtn.Init(job, m_JobButtons.Count - 1);
            jobbtn.AddListener(OnJobTypeValueChanged);
            m_NewJobSelected = jobbtn.m_ID;
        }

        foreach (UI_SkillJobButton job in m_JobButtons)
        {
            if (job.m_Jobtype == m_DisplayType)
            {
                job.m_Toggle.isOn = true;
                break;
            }
        }
        m_DirtyBit |= m_Dirty.JobChange;

        m_JobTitle.text = JobSectRepo.GetJobLocalizedName(m_DisplayType);
        m_SkillDescriptor.Initialise(this.transform);
        m_SkillDescriptor.CloseUI();

        m_EquippableSize = GameInfo.gLocalPlayer.SkillStats.EquipSize;
        //m_EquipSkillInv = new List<UI_SkillSelectButton>(m_EquippableSize + 1);
        m_AutoSlotGroup = GameInfo.gLocalPlayer.SkillStats.AutoGroup;
        m_EquipSlotGroup = GameInfo.gLocalPlayer.SkillStats.EquipGroup;
        m_ScrollPanelGroup.Value = m_EquipSlotGroup;

        m_EquipIconHelper.Init(this);
        m_EquipIconHelper.CreateSkillEquipPanel(UI_SkillEquipIconHelper.ePanelType.ePlayerSkill, m_EquippableSize);
        m_EquipIconHelper.CreateSkillEquipPanel(UI_SkillEquipIconHelper.ePanelType.eAutoSkill, m_EquippableSize);
        //m_EquipIconHelper.DisplayPanelOption(UI_SkillEquipIconHelper.ePanelType.ePlayerSkill);
        //for (int i = 0; i < m_EquippableSize; ++i)
        //{
        //    GameObject label = Instantiate(m_EquipSkillIconPrefab, m_SelectSkillContentRect);
        //    //UI_SkillSelectButton b = label.GetComponent<UI_SkillSelectButton>();
        //    m_EquipSkillInv.Add(label.GetComponent<UI_SkillSelectButton>());
        //    m_EquipSkillInv[i].m_parentPanel = this;
        //    m_EquipSkillInv[i].m_skgID = i;
        //    m_EquipSkillInv[i].Init(OnSelectEquipSkill);
        //}

        //m_EquipSkillInv.Add(m_EquipSkillInv[0]);
        m_SelectSkillDDL.m_PanelPanel = this;
        m_SelectSkillDDL.Initialise(this.transform);
        m_SpecialSkillPanel.m_Parent = this;
        m_SpecialSkillPanel.Initialise(this.transform);
        m_SpecialSkillPanel.GenerateList(m_DisplayType);

        m_SkillButtonChanged = new UI_SkillButton[2];
        m_SkillButtonChanged[0] = m_SkillButtonChanged[1] = null;

    }

    private void Update()
    {
        if ((m_DirtyBit & m_Dirty.PanelUpdated) == m_Dirty.PanelUpdated)
        {
            FindLastIcon();
            m_DirtyBit ^= m_Dirty.PanelUpdated;
        }
        // update dirty info
        if ((m_DirtyBit & m_Dirty.JobChange) == m_Dirty.JobChange)
        {
            if (m_SkillTreeCache.Count > 0 && m_SkillTreeCache.ContainsKey(m_DisplayType))
                // off current job panel
                foreach (GameObject panel in m_SkillTreeCache[m_DisplayType])
                {
                    foreach (Transform obj in panel.transform)
                    {
                        UI_SkillButton btn = obj.GetComponent<UI_SkillButton>();
                        if (btn)
                            btn.m_Toggle.isOn = false;
                    }

                    panel.SetActive(false);
                }

            m_DisplayType = m_JobButtons[m_NewJobSelected].m_Jobtype;
            m_JobTitle.text = JobSectRepo.GetJobLocalizedName(m_DisplayType);

            // check if panel exist
            ConstructPanel();

            m_DirtyBit ^= m_Dirty.JobChange;
        }

        if ((m_DirtyBit & m_Dirty.ScrollPanel) == m_Dirty.ScrollPanel)
        {
            if (m_Time < 1)
            {
                m_Time += (Time.deltaTime * m_Speed);
                float x = m_TweenFunction.Evaluate(m_Time);
                x = m_StartPos * (1 - x) + m_FinalPos * x;
                m_SkillPanelContentRect.transform.localPosition = new Vector3(x, m_SkillPanelContentRect.transform.localPosition.y, m_SkillPanelContentRect.transform.localPosition.z);
            }
            else
            {
                m_Time = 0;
                m_DirtyBit ^= m_Dirty.ScrollPanel;
                //m_SkillPanel.transform.localPosition = new Vector3(m_FinalPos, m_SkillPanel.transform.localPosition.y);
            }
        }
        
        if (m_SkillPanelContentRect.transform.localPosition.x >= 0)
        {
            m_Left.SetActive(false);
        }
        else
        {
            m_Left.SetActive(true);
        }

        if (m_SkillPanelContentRect.transform.localPosition.x <= -(m_LastIconXPos - 280))
        {
            m_Right.SetActive(false);
        }
        else
        {
            m_Right.SetActive(true);
        }

        
    }

    // press job -> send new type here
    // hide current panel
    // construct
    // show panel

    public void ParseSkillDependency(JobType job, UI_SkillButton button, SkillTreeJson data)
    {
        string[] skills = data.parent.Split(';');
        foreach (string str in skills)
        {
            // parse the row
            int row = str[0] - 65;
            int col = 0;
            bool complete = System.Int32.TryParse(str.Substring(1), out col);
            if (complete)
            {
                Transform pbutton = m_SkillTreeCache[job][row].transform.GetChild(col);
                if (pbutton != null)
                {
                    button.m_RequiredSkills.Add(new KeyValuePair<int, int>(row, col));
                    UI_SkillButton anchor = pbutton.GetComponent<UI_SkillButton>();
                    // find which labels to use
                    // Get the row diff
                    int rowdiff = anchor.m_Row - button.m_Row;
                    int connectoridx = 0;
                    switch (rowdiff)
                    {
                        case -2:
                            connectoridx = 1;
                            break;

                        case -1:
                            if (anchor.m_Row == 1)
                                connectoridx = 3;
                            else
                                connectoridx = 0;
                            break;

                        case 0:
                            // check if short or long
                            if (button.m_Col - anchor.m_Col > 1)
                            {
                                connectoridx = 7;
                                // long
                            }
                            else
                            {
                                connectoridx = 6;
                            }
                            break;

                        case 1:
                            if (anchor.m_Row == 1)
                                // middle
                                connectoridx = 2;
                            else
                                connectoridx = 5;
                            break;

                        case 2:
                            connectoridx = 4;
                            break;
                    }

                    GameObject obj = GameObject.Instantiate(m_Connectors[connectoridx], anchor.m_Anchor);
                }
            }
        }
    }

    public void ConstructPanel()
    {
        if (m_SkillTreeCache.ContainsKey(m_DisplayType))
        {
            // show from cache
            foreach (GameObject row in m_SkillTreeCache[m_DisplayType])
            {
                row.SetActive(true);
            }
        }
        else
        {
            // construct panel
            // get list of jobs the player can learn
            List<Dictionary<int, SkillTreeJson>> layout = SkillTreeRepo.GetSkillTreeInfo(m_DisplayType);

            //create list of the buttons
            //contruct the current panel
            m_SkillTreeCache[m_DisplayType] = new List<GameObject>();
            for (int i = 0; i < 3; ++i)
            {
                m_SkillTreeCache[m_DisplayType].Add(m_RowPool.RequestObject());
                m_SkillTreeCache[m_DisplayType][i].transform.position = new Vector3(5, 0, 0);
                m_SkillTreeCache[m_DisplayType][i].transform.SetParent(m_SkillPanelContentRect.transform, false);
                //m_SkillTreeCache[m_DisplayType][i].transform.localPosition = new Vector3(m_SkillTreeCache[m_DisplayType][i].transform.localPosition.x, m_SkillTreeCache[m_DisplayType][i].transform.localPosition.y, 0);
                //m_SkillTreeCache[m_DisplayType][i].transform.localScale = new Vector3(1, 1, 1);
            }

            if (layout == null)
            {
                return;
            }

            int col = 0, row = 0;

            int maxCol = 0;
            foreach (Dictionary<int, SkillTreeJson> dict in layout)
            {
                if (dict.Count != 0 && dict.Keys.Max() > maxCol)
                    maxCol = dict.Keys.Max();
            }
            ++maxCol; // starts 0

            for (int i = 0; i < maxCol; ++i)
            {
                foreach (Dictionary<int, SkillTreeJson> dict in layout)
                {
                    if (dict.ContainsKey(col))
                    {
                        //create skill icon
                        GameObject go = m_ButtonPool.RequestObject();
                        UI_SkillButton button = go.GetComponent<UI_SkillButton>();
                        button.m_parentPanel = this;

                        button.m_Row = row;
                        button.m_Col = col;

                        if (dict[col].parent.Length > 0)
                            // check for dependancy
                            ParseSkillDependency(m_DisplayType, button, dict[col]);

                        button.Init(dict[col]);
                        button.AddListener(OnSelectSkill);
                        button.transform.SetParent(m_SkillTreeCache[m_DisplayType][row].transform);
                        button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y, 0);
                        button.transform.localScale = new Vector3(1, 1, 1);
                    }
                    else
                    {
                        GameObject go = m_EmptyButtonPool.RequestObject();
                        go.transform.SetParent(m_SkillTreeCache[m_DisplayType][row].transform);
                    }
                    ++row;
                }
                row = 0;
                ++col;
            }
        }
        //FindLastIcon();
        m_DirtyBit |= m_Dirty.PanelUpdated;
    }

    public void RegisterWindow(string identifier, UI_SkillTreeWindow obj)
    {
        m_Windows.Add(identifier, obj);
    }

    public void CloseWindows(string identifier)
    {
        foreach(var window in m_Windows)
        {
            if(window.Key.CompareTo(identifier) != 0)
            {
                window.Value.CloseWindow();
            }
        }
    }

    public void CloseWindows(bool isBottom = false)
    {
        //m_SkillDescriptor.OnClosed();
        //m_SkillDescriptor.CloseUI();
        //m_CurrentActive = null;
        ////m_SelectSkillDDL.CloseUI();
        //m_SelectSkillDDL.gameObject.SetActive(false);
        //m_SpecialSkillPanel.m_SkillDescriptor.gameObject.SetActive(false);
        //m_SpecialSkillPanel.CloseUI();
        ////m_SpecialSkillPanel.gameObject.SetActive(false);
        ////m_EquipSkillPanel.SetActive(false);

        //if (!isBottom)
            //m_CloseEquip.onClick.Invoke();

        m_CurrentActive = null;
        foreach (var window in m_Windows)
            window.Value.CloseWindow();
    }

    public void CloseWindowsOnSkillSelect()
    {
        m_SelectSkillDDL.CloseUI();
        m_SelectSkillDDL.gameObject.SetActive(false);
        //m_SpecialSkillPanel.CloseUI();
        //m_SpecialSkillPanel.gameObject.SetActive(false);
        m_EquipSkillPanel.SetActive(false);
        m_CloseEquip.onClick.Invoke();
    }

    public void FindLastIcon()
    {
        int higestC = 0;
        foreach (var tree in m_SkillTreeCache[m_DisplayType])
        {
            if (tree.transform.childCount == 0) continue;
            UI_SkillButton button = tree.transform.GetChild(tree.transform.childCount - 1).gameObject.GetComponent<UI_SkillButton>();
            if (button != null && button.m_Col > higestC)
            {
                m_LastIconXPos = button.GetComponent<RectTransform>().anchoredPosition.x;
            }
        }
    }

    public void OnJobTypeValueChanged(UI_SkillJobButton change)
    {
        m_DirtyBit |= m_Dirty.JobChange;
        if (change.m_Toggle.isOn)
        {
            m_JobButtons[m_NewJobSelected].m_Toggle.isOn = false;
            m_NewJobSelected = change.m_ID;
        }

        m_DirtyBit |= m_Dirty.ScrollPanel;
        m_StartPos = m_SkillPanelContentRect.transform.localPosition.x;
        m_FinalPos = 0;
        //m_SkillDescriptor.CloseUI();
        CloseWindows("SkillEquip");
    }

    public void OnSelectSkill(UI_SkillButtonBase button)
    {
        if (m_CurrentActive == null)
        {
            // no selected button
            m_CurrentActive = (UI_SkillButton)button;
            CloseWindowsOnSkillSelect();
            // first select
            m_SkillDescriptor.gameObject.SetActive(true);
            CloseWindows("SkillUI");
            m_SkillDescriptor.Show((UI_SkillButton)button);
        }

        else if (m_CurrentActive != button)
        {
            m_SkillDescriptor.OnClosed();
            m_CurrentActive.m_Toggle.isOn = false;
            m_SkillDescriptor.Show((UI_SkillButton)button);
            m_CurrentActive = (UI_SkillButton)button;

            m_StartPos = m_SkillPanelContentRect.transform.localPosition.x;
            m_FinalPos = -(button.transform.localPosition.x - 179);
            if (m_FinalPos > 0.1f)
                m_FinalPos = 0;
            m_DirtyBit |= m_Dirty.ScrollPanel;
        }
        else
        {
            button.m_Toggle.isOn = false;
            //m_SkillDescriptor.OnClosed();
            m_SkillDescriptor.CloseUI();
            m_CurrentActive = null;
        }

        //// this function is called twice as 2 buttons has state changes
        //if (button.m_Toggle.isOn)
        //{
        //    //CloseWindows();
        //    CloseWindowsOnSkillSelect();
        //    m_CurrentActive = button;
        //    m_SkillDescriptor.gameObject.SetActive(true);
        //    m_SkillDescriptor.Show(m_CurrentActive);

        //    m_StartPos = m_SkillPanelContentRect.transform.localPosition.x;
        //    m_FinalPos = -(button.transform.localPosition.x - 179);
        //    if (m_FinalPos > 0.1f)
        //        m_FinalPos = 0;
        //    m_DirtyBit |= m_Dirty.ScrollPanel;
        //}
        //else if (!button.m_Toggle.isOn && button == m_CurrentActive)
        //{
        //    m_SkillDescriptor.OnClosed();
        //    //m_SkillDescriptor.CloseUI();
        //}
    }

    public void CloseSkillDescriptor()
    {
        if (m_CurrentActive != null)
        {
            m_CurrentActive.m_Toggle.isOn = false;
            m_SkillDescriptor.OnClosed();
            m_CurrentActive = null;
        }
    }

    public void ReloadSkillDescriptor(int newskillpoint, int newmoney)
    {
        m_SkillDescriptor.OnClosed();
        m_SkillDescriptor.Reload(newskillpoint, newmoney, m_CurrentActive);
    }


    public void OnCloseSelectEquipSkill()
    {
        m_EquipIconHelper.ClosePanel();
    }

    public void OnEquipSkill(int id)
    {

        m_EquipIconHelper.OnSelectEquip(id);

        // close the drop down and filter
        m_SelectSkillDDL.OnEquipedSkill(id);
    }

    public void OnEventSkillLevelUp(byte result, int skillid, int skillpoint, int money)
    {
        if (((SkillReturnCode)result & SkillReturnCode.SUCCESS) == SkillReturnCode.SUCCESS)
        {
            // levelup of skill success, search for skill
            List<JobType> jobs = SkillRepo.GetSkillRequiredClass(skillid);
            SkillData skill = SkillRepo.GetSkill(skillid);

            //phase 1 update the level ups
            foreach (JobType tag in jobs)
            {
                bool isDone = false;
                //update buttons of all related jobs
                foreach (GameObject row in m_SkillTreeCache[m_DisplayType])
                {
                    UI_SkillButton[] obj = row.GetComponentsInChildren<UI_SkillButton>();

                    foreach (UI_SkillButton button in obj)
                    {
                        if (button.m_skgID == skill.skillgroupJson.id)
                        {
                            button.OnServerVerifiedLevelUp(skillid, money, skillpoint);
                            isDone = true;
                            break;
                        }
                    }
                    if (isDone)
                        break;
                }
            }

            //phase 2 update all buttons
            m_SelectSkillDDL.GenerateSkillList();

            // update panel skills
            foreach (GameObject row in m_SkillTreeCache[m_DisplayType])
            {
                UI_SkillButton[] obj = row.GetComponentsInChildren<UI_SkillButton>();

                foreach (UI_SkillButton button in obj)
                {
                    button.UpdateButton();
                }
            }
        }
    }

    public void LevelMaxed()
    {
        m_SkillDescriptor.m_Upgrade.interactable = false;
    }

    public bool IsRequiredSkillsUnlocked(UI_SkillButton skill)
    {
        bool result = true;

        foreach (KeyValuePair<int, int> info in skill.m_RequiredSkills)
        {
            Transform pbutton = m_SkillTreeCache[m_DisplayType][info.Key].transform.GetChild(info.Value);
            if (pbutton != null)
            {
                UI_SkillButton anchor = pbutton.GetComponent<UI_SkillButton>();
                if (anchor != null && !anchor.IsUnlocked())
                {
                    result = false;
                    break;
                }
            }
        }
        return result;
    }

    public bool IsRequiredJobUnlocked(UI_SkillButton skill)
    {
        List<int> req_jobs = new List<int>();
        SkillData mskill = skill.m_SkillData;
        if (mskill == null)
            mskill = SkillRepo.GetSkillByGroupIDOfNextLevel(skill.m_skgID, 0);
        if (mskill.skillJson.requiredclass.CompareTo("#unnamed#") == 0) return true;
        string[] req_class = mskill.skillJson.requiredclass.Split(';');
        foreach (string req_class_iter in req_class)
        {
            int converted = 0;
            bool toint = System.Int32.TryParse(req_class_iter, out converted);
            if (toint)
            {
                req_jobs.Add(converted);
            }
        }

        List<JobType> hist = new List<JobType>();
        // need to construct player job history for this to work...
        if (GameInfo.gLocalPlayer != null)
            hist = JobSectRepo.GetJobHistoryToCurrent(GameInfo.gLocalPlayer.GetJobSect());

        //loop all the class to see if player has it
        foreach (int iter in req_jobs)
        {
            if (hist.Exists(x => x == (JobType)iter))
                return true;
        }

        return false;
    }

    public int GetEquipGroup()
    {
        return m_EquipSlotGroup;
    }

    public int GetAutoGroup()
    {
        return m_AutoSlotGroup;
    }

    public int GetMaxEquipSize()
    {
        return m_EquippableSize;
    }

    public void OnSelectSkillEquip(bool isPlayerEquip)
    {
        this.isPlayerEquip = isPlayerEquip;


        // load the equipskillinv
        if (isPlayerEquip)
        {
            m_ScrollPanelGroup.Value = m_EquipSlotGroup;
            //for (int i = 0; i < m_EquippableSize; ++i)
            //{
            //    m_EquipSkillInv[i].EquipSkill((int)GameInfo.gLocalPlayer.SkillStats.EquippedSkill[m_EquippableSize * (m_EquipSlotGroup - 1) + m_EquipSkillInv[i].m_skgID]);
            //}
            m_EquipIconHelper.DisplayPanelOption(UI_SkillEquipIconHelper.ePanelType.ePlayerSkill);
            m_EquipIconHelper.ShowPanel();
        }
        else
        {
            m_ScrollPanelGroup.Value = m_AutoSlotGroup;
            //for (int i = 0; i < m_EquippableSize; ++i)
            //{
            //    m_EquipSkillInv[i].EquipSkill((int)GameInfo.gLocalPlayer.SkillStats.AutoSkill[m_EquippableSize * (m_AutoSlotGroup - 1) + m_EquipSkillInv[i].m_skgID]);
            //}
            m_EquipIconHelper.DisplayPanelOption(UI_SkillEquipIconHelper.ePanelType.eAutoSkill);
            m_EquipIconHelper.ShowPanel();
        }
    }

    public void OnSelectGroupEquipDown()
    {
        if (isPlayerEquip)
        {
            if (m_EquipSlotGroup == 1) return;
            else
            {
                --m_EquipSlotGroup;
                m_EquipIconHelper.DisplayPanelOption(UI_SkillEquipIconHelper.ePanelType.ePlayerSkill);
                m_EquipIconHelper.ShowPanel();

                //for (int i = 0; i < m_EquippableSize; ++i)
                //{
                //    m_EquipSkillInv[i].GetComponent<UI_SkillSelectButton>().EquipSkill((int)GameInfo.gLocalPlayer.SkillStats.EquippedSkill[m_EquippableSize * (m_EquipSlotGroup - 1) + m_EquipSkillInv[i].m_skgID]);
                //}

                //GameInfo.gLocalPlayer.SkillStats.EquipGroup = m_EquipSlotGroup;
            }
        }
        else
        {
            if (m_AutoSlotGroup == 1) return;
            else
            {
                --m_AutoSlotGroup;
                m_EquipIconHelper.DisplayPanelOption(UI_SkillEquipIconHelper.ePanelType.eAutoSkill);
                m_EquipIconHelper.ShowPanel();

                //for (int i = 0; i < m_EquippableSize; ++i)
                //{
                //    m_EquipSkillInv[i].GetComponent<UI_SkillSelectButton>().EquipSkill((int)GameInfo.gLocalPlayer.SkillStats.AutoSkill[m_EquippableSize * (m_AutoSlotGroup - 1) + m_EquipSkillInv[i].m_skgID]);
                //}

                //GameInfo.gLocalPlayer.SkillStats.AutoGroup = m_AutoSlotGroup;
            }
        }

        //m_EquipSkillInv[m_EquippableSize].m_Toggle.isOn = false;
    }

    public void OnSelectGroupEquipUp()
    {
        if (isPlayerEquip)
        {
            if (m_EquipSlotGroup == 6) return;
            else
            {
                ++m_EquipSlotGroup;
                m_EquipIconHelper.DisplayPanelOption(UI_SkillEquipIconHelper.ePanelType.ePlayerSkill);
                m_EquipIconHelper.ShowPanel();

                //for (int i = 0; i < m_EquippableSize; ++i)
                //{
                //    m_EquipSkillInv[i].GetComponent<UI_SkillSelectButton>().EquipSkill((int)GameInfo.gLocalPlayer.SkillStats.EquippedSkill[m_EquippableSize * (m_EquipSlotGroup - 1) + m_EquipSkillInv[i].m_skgID]);
                //}

                //GameInfo.gLocalPlayer.SkillStats.EquipGroup = m_EquipSlotGroup;
            }
        }
        else
        {
            if (m_AutoSlotGroup == 6) return;
            else
            {
                ++m_AutoSlotGroup;
                m_EquipIconHelper.DisplayPanelOption(UI_SkillEquipIconHelper.ePanelType.eAutoSkill);
                m_EquipIconHelper.ShowPanel();

                //for (int i = 0; i < m_EquippableSize; ++i)
                //{
                //    m_EquipSkillInv[i].GetComponent<UI_SkillSelectButton>().EquipSkill((int)GameInfo.gLocalPlayer.SkillStats.AutoSkill[m_EquippableSize * (m_AutoSlotGroup - 1) + m_EquipSkillInv[i].m_skgID]);
                //}

                //GameInfo.gLocalPlayer.SkillStats.AutoGroup = m_AutoSlotGroup;
            }
        }

        //m_EquipSkillInv[m_EquippableSize].m_Toggle.isOn = false;
        m_SelectSkillDDL.CloseUI();
    }

    

    public void UpdateBasicAttack(int skid)
    {
        if(m_SpecialSkillPanel != null)
            m_SpecialSkillPanel.UpdateBasicAttack(skid);
    }
}