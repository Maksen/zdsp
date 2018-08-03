using System.Collections.Specialized;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;
using Zealot.Common;
using Kopio.JsonContracts;

public class GameObjectPoolManager
{
    public class PoolObject
    {
        public bool isFree = true;
        public GameObject obj;

        public PoolObject(GameObject objtype)
        {
            obj = GameObject.Instantiate(objtype, new Vector3(0,0,0), Quaternion.identity);
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
        m_ParentContainer.transform.parent = m_Master;
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
        if(m_Pool.Count == 0)
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
        PoolObject pObj = m_Pool[vtable[obj.GetHashCode()]];
        pObj.isFree = true;
        pObj.obj.SetActive(false);
        pObj.obj.transform.parent = m_ParentContainer.transform;
    }

    public void EmptyPool()
    {
        m_IsFilled = 0;
        m_Pool.Clear();
        vtable.Clear();
    }

    public void CheckContainerIntegrity()
    {
        if(m_IsFilled == 0)
        {
            // need to refill
            FillPool();
        }
    }
}

public class UI_SkillTree : BaseWindowBehaviour
{
    [Header("Required Gameobjects")]
    public GameObject m_ButtonPrefab;
    public GameObject m_EmptyButtonPrefab;
    public GameObject m_RowPrefab;
    public GameObject m_EquipSkillIconPrefab;
    public GameObject m_SkillPanelContentRect; // Content Panel

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
    }
    private m_Dirty m_DirtyBit;

    private int m_NewJobSelected;

    private UI_SkillButton m_CurrentActive;
    private List<UI_SkillSelectButton> m_EquipSkillInv;

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

    [Header("Debug use variables")]
    public int m_EquippableSize;
    public int m_EquipSlotGroup = 1;

    private bool isInit = false;

    public override void OnRegister()
    {
        base.OnRegister();
        
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        //clean up
        //m_ButtonPool.EmptyPool();
        //m_EmptyButtonPool.EmptyPool();
        //m_RowPool.EmptyPool();
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
            foreach(var objectlist in m_SkillTreeCache)
            {
                foreach(var obj in objectlist.Value)
                {
                    foreach(Transform child in obj.transform)
                    {
                        UI_SkillButton ui = child.GetComponent<UI_SkillButton>();
                        if (ui != null)
                            ui.UpdateButton();
                    }
                }
            }
        }

        m_DisplayType = (JobType)GameInfo.gLocalPlayer.PlayerSynStats.jobsect; 
        foreach (UI_SkillJobButton job in m_JobButtons)
        {
            if (job.m_Jobtype == m_DisplayType)
            {
                job.m_Toggle.isOn = true;
                break;
            }
        }
    }

    public void Initialise()
    {
        isInit = !isInit;
        m_ButtonPool = new GameObjectPoolManager(10, this.transform, m_ButtonPrefab);
        m_EmptyButtonPool = new GameObjectPoolManager(10, this.transform, m_EmptyButtonPrefab);
        m_RowPool = new GameObjectPoolManager(3, this.transform, m_RowPrefab);

        foreach (Transform child in m_JobPanel.transform)
        {
            m_JobButtons.Add(child.GetComponent<UI_SkillJobButton>());
            UI_SkillJobButton btn = m_JobButtons[m_JobButtons.Count - 1];
            btn.Init(m_JobButtons.Count - 1);
            btn.m_Toggle.onValueChanged.AddListener(delegate
            {
                OnJobTypeValueChanged(btn);
            });
        }

        m_DisplayType = m_JobButtons[0].m_Jobtype; ; // (JobType)GameInfo.gLocalPlayer.PlayerSynStats.jobsect;// 
        foreach(UI_SkillJobButton job in m_JobButtons)
        {
            if(job.m_Jobtype == m_DisplayType)
            {
                job.m_Toggle.isOn = true;
                break;
            }
        }
        m_JobTitle.text = JobSectRepo.GetJobLocalizedName(m_DisplayType);
        m_SkillDescriptor.Initialise(this.transform);
        m_SkillDescriptor.gameObject.SetActive(false);

        m_EquipSkillInv = new List<UI_SkillSelectButton>(m_EquippableSize + 1);

        for (int i = 0; i < m_EquippableSize; ++i)
        {
            GameObject label = Instantiate(m_EquipSkillIconPrefab, m_SelectSkillContentRect);
            UI_SkillSelectButton b = label.GetComponent<UI_SkillSelectButton>();
            m_EquipSkillInv.Add(label.GetComponent<UI_SkillSelectButton>());
            m_EquipSkillInv[i].m_parentPanel = this;
            m_EquipSkillInv[i].m_ID = i;
            m_EquipSkillInv[i].Init();
        }
        m_EquipSkillInv.Add(m_EquipSkillInv[0]);
        m_SelectSkillDDL.m_PanelPanel = this;
        m_SelectSkillDDL.Initialise(this.transform);
        m_SpecialSkillPanel.m_Parent = this;
        m_SpecialSkillPanel.Initialise(this.transform);
    }

    private void Update()
    {
        // update dirty info
        if ((m_DirtyBit & m_Dirty.JobChange) == m_Dirty.JobChange)
        {
            if(m_SkillTreeCache.Count > 0)
                // off current job panel
                foreach(GameObject panel in m_SkillTreeCache[m_DisplayType])
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

        if((m_DirtyBit & m_Dirty.ScrollPanel) == m_Dirty.ScrollPanel)
        {
            if (m_Time < 1)
            {
                m_Time += (Time.deltaTime * m_Speed);
                float x = m_TweenFunction.Evaluate(m_Time);
                x = m_StartPos * (1 - x) + m_FinalPos * x;
                m_SkillPanelContentRect.transform.localPosition = new Vector3(x, m_SkillPanelContentRect.transform.localPosition.y);
            }
            else
            {
                m_Time = 0;
                m_DirtyBit ^= m_Dirty.ScrollPanel;
                //m_SkillPanel.transform.localPosition = new Vector3(m_FinalPos, m_SkillPanel.transform.localPosition.y);
            }
        }

        if(m_SkillPanelContentRect.transform.localPosition.x <= 0)
        {
            m_Left.SetActive(false);
        }
        else
        {
            m_Left.SetActive(true);
        }
    }

    // press job -> send new type here 
    // hide current panel
    // construct
    // show panel


    public void ParseSkillDependency(JobType job, UI_SkillButton button, SkillTreeJson data)
    {
        string[] skills = data.parent.Split(';');
        foreach(string str in skills)
        {
            // parse the row
            int row = str[0] - 65;
            int col = 0;
            bool complete = System.Int32.TryParse(str.Substring(1), out col);
            if(complete)
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
            foreach(GameObject row in m_SkillTreeCache[m_DisplayType])
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
                m_SkillTreeCache[m_DisplayType][i].transform.parent = m_SkillPanelContentRect.transform;
                m_SkillTreeCache[m_DisplayType][i].transform.localPosition = new Vector3(m_SkillTreeCache[m_DisplayType][i].transform.localPosition.x, m_SkillTreeCache[m_DisplayType][i].transform.localPosition.y, 0);
                m_SkillTreeCache[m_DisplayType][i].transform.localScale = new Vector3(1, 1, 1);
            }

            if (layout == null)
            {
                return;
            }

            int col = 0, row = 0;

            int maxCol = 0;
            foreach(Dictionary<int, SkillTreeJson>dict in layout)
            {
                if (dict.Count != 0 && dict.Keys.Max() > maxCol)
                    maxCol = dict.Keys.Max();
            }
            ++maxCol; // starts 0

            for(int i = 0; i < maxCol; ++i)
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

                        button.Init(dict[col], delegate { button.OnSelected(); });
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
    }

    public void OnJobTypeValueChanged(UI_SkillJobButton change)
    {
        m_DirtyBit |= m_Dirty.JobChange;
        if (change.m_Toggle.isOn)
            m_NewJobSelected = change.m_ID;

        m_DirtyBit |= m_Dirty.ScrollPanel;
        m_StartPos = m_SkillPanelContentRect.transform.localPosition.x;
        m_FinalPos = 0;
    }

    public void OnSelectSkill(UI_SkillButton button)
    {
        if(m_CurrentActive != button && m_CurrentActive != null)
            m_CurrentActive.m_Toggle.isOn = false;

        // this function is called twice as 2 buttons has state changes
        if (button.m_Toggle.isOn)
        {
            m_CurrentActive = button;
            m_SkillDescriptor.gameObject.SetActive(true);
            m_SkillDescriptor.Show(m_CurrentActive);

            m_StartPos = m_SkillPanelContentRect.transform.localPosition.x;
            m_FinalPos = -(button.transform.localPosition.x - 179);
            if(m_FinalPos > 0.1f) 
                m_FinalPos = 0;
            m_DirtyBit |= m_Dirty.ScrollPanel;

        }
        else
        {
            m_SkillDescriptor.OnClosed();
            m_SkillDescriptor.gameObject.SetActive(false);
        }
    }

    public void CloseSkillDescriptor()
    {
        //m_SkillDescriptor.OnClosed();
        //m_SkillDescriptor.gameObject.SetActive(false);
        if(m_CurrentActive != null)
            m_CurrentActive.m_Toggle.isOn = false;
    }

    public void ReloadSkillDescriptor()
    {
        m_SkillDescriptor.OnClosed();
        m_SkillDescriptor.Show(m_CurrentActive);
    }

    public void OnSelectEquipSkill(UI_SkillSelectButton button)
    {
        
        if (m_EquipSkillInv[button.m_ID].m_Toggle.isOn == true)
        {
            // currently selected
            if(button.m_ID != m_EquipSkillInv[m_EquippableSize].m_ID && m_EquipSkillInv[m_EquippableSize].m_Toggle.isOn == true)
            {
                m_EquipSkillInv[m_EquippableSize].m_Toggle.isOn = false;
                
            }
            m_EquipSkillInv[m_EquippableSize] = button;
            m_SelectSkillDDL.GenerateSkillList();
            m_SelectSkillDDL.gameObject.SetActive(true);
        }
    }

    public void OnCloseSelectEquipSkill()
    {
        m_EquipSkillInv[m_EquippableSize].m_Toggle.isOn = false;
    }

    public void OnEquipSkill(int id)
    {
        // check for selected skill repeat status
        for (int i = 0; i < m_EquippableSize; ++i)
        {
            if(m_EquipSkillInv[i].m_Skillid == id)
            {
                
                if(m_EquipSkillInv[m_EquippableSize].m_Skillid != 0)
                {
                    // swap with selected
                    int _id = m_EquipSkillInv[m_EquippableSize].m_Skillid;
                    m_EquipSkillInv[i].EquipSkill(_id);
                    //m_EquipSkillInv[i].m_Skillid = _id;
                    //m_EquipSkillInv[i].m_Icon.sprite = ClientUtils.LoadIcon(SkillRepo.GetSkill(_id).skillgroupJson.icon);
                    //if (GameInfo.gLocalPlayer != null)
                    //{
                    //    GameInfo.gLocalPlayer.SkillStats.EquippedSkill[m_EquipSkillInv[i].m_ID] = id;
                    //}
                    break;
                }
                else
                {
                    m_EquipSkillInv[i].m_Skillid = 0;
                    m_EquipSkillInv[i].m_Icon.sprite = ClientUtils.LoadIcon("UI_ZDSP_Icons/Skill/00_ActiveEmpty.png");
                    if (GameInfo.gLocalPlayer != null)
                    {
                        GameInfo.gLocalPlayer.SkillStats.EquippedSkill[m_EquipSkillInv[i].m_ID] = 0;
                    }
                }
            }
        }

        // set the icon of selected to this skill's icon
        m_EquipSkillInv[m_EquippableSize].EquipSkill(id);
        //m_EquipSkillInv[9].m_Icon.sprite = ClientUtils.LoadIcon(SkillRepo.GetSkill(id).skillgroupJson.icon);
        //m_EquipSkillInv[9].m_Skillid = id;

        // close the drop down and filter
        m_SelectSkillDDL.OnEquipedSkill(id);

        if (GameInfo.gLocalPlayer != null)
        {
            //GameInfo.gLocalPlayer.SkillStats.EquipedSkill[m_EquipSkillInv[9].m_ID] = id;
            RPCFactory.NonCombatRPC.EquipSkill(id, m_EquipSkillInv[m_EquippableSize].m_ID, m_EquipSlotGroup);
        }
    }

    public void OnEventSkillLevelUp(byte result, int skillid, int skillpoint, int money)
    {
        if(((SkillReturnCode)result & SkillReturnCode.SUCCESS) == SkillReturnCode.SUCCESS)
        {
            // levelup of skill success, search for skill
            List<JobType> jobs = SkillRepo.GetSkillRequiredClass(skillid);
            SkillData skill = SkillRepo.GetSkill(skillid);

            //phase 1 update the level ups
            foreach(JobType tag in jobs)
            {
                bool isDone = false;
                //update buttons of all related jobs
                foreach (GameObject row in m_SkillTreeCache[m_DisplayType])
                {
                    UI_SkillButton[] obj = row.GetComponentsInChildren<UI_SkillButton>();

                    foreach (UI_SkillButton button in obj)
                    {
                        if (button.m_ID == skill.skillgroupJson.id)
                        {
                            button.OnServerVerifiedLevelUp(skillid);
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

    public void OnLevelUpSkillWithID(int skillid, int level)
    {
        m_SelectSkillDDL.GenerateSkillList();

        // update panel skills 
        foreach(GameObject row in m_SkillTreeCache[m_DisplayType])
        {
            UI_SkillButton[] obj = row.GetComponentsInChildren<UI_SkillButton>();

            foreach (UI_SkillButton button in obj)
            {
                button.UpdateButton();
            }
        }
    }

    public bool IsRequiredSkillsUnlocked(UI_SkillButton skill)
    {
        bool result = true;

        foreach(KeyValuePair<int, int> info in skill.m_RequiredSkills)
        {
            Transform pbutton = m_SkillTreeCache[m_DisplayType][info.Key].transform.GetChild(info.Value);
            if (pbutton != null)
            {
                UI_SkillButton anchor = pbutton.GetComponent<UI_SkillButton>();
                if (!anchor.IsUnlocked())
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
        if(mskill == null)
            mskill = SkillRepo.GetSkillByGroupIDOfNextLevel(skill.m_ID, 0);
        if (mskill.skillJson.requiredclass.CompareTo("#unnamed#") == 0) return true;
        string[] req_class = mskill.skillJson.requiredclass.Split(';');
        foreach(string req_class_iter in req_class)
        {
            int converted = 0;
            bool toint = System.Int32.TryParse(req_class_iter, out converted);
            if (toint)
            {
                req_jobs.Add(converted);
            }
        }

        // need to construct player job history for this to work...
        List<JobType> hist = JobSectRepo.GetJobHistoryToCurrent(GameInfo.gLocalPlayer.GetJobSect());

        //loop all the class to see if player has it
        foreach (int iter in req_jobs)
        {
            if (hist.BinarySearch((JobType)iter) < 0)
                return false;
        }

        return true;
    }
}
