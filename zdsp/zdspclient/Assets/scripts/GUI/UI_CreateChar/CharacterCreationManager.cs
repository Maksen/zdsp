using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zealot.Repository;
using Zealot.Common;
using Kopio.JsonContracts;

public class CharacterCreationManager : MonoBehaviour
{
    //public CharacterModelSelector selector, platform_selector;
    [SerializeField]
    GameObject inputField;
    string current_name;

    private JobType jobSelected = JobType.Warrior;
    private int initial_skill = 0;
    [SerializeField]
    Image Skill1Image;
    [SerializeField]
    Image Skill2Image;
    [SerializeField]
    Image Skill3Image;
    [SerializeField]
    DragSpin3DAvatar dragSpin3DAvatar;
    [SerializeField]
    UI_DragEvent ui_dragevent;
    [SerializeField]
    Model_3DAvatar avatarModel;
    [SerializeField]
    List<Toggle> SPSToggle;//sic,stone,cloth

    public int FactionRewardID = 0;

    public static FactionType RecommendedFaction;

    // Use this for initialization
    void Start()
    {
       // UIManager.OpenDialog(WindowType.DialogSceneMovie, (window) => { window.GetComponent<DialogMovie>().StartPlay(GameLoader.MovieComics, null); });

        string res = GameConstantRepo.GetConstant("RecommendedFactionRewardItemID");
        if (string.IsNullOrEmpty(res) || int.TryParse(res, out FactionRewardID) == false)
        {
            FactionRewardID = 1045;
        }

        ui_dragevent.onDragging = null;//clear
        ui_dragevent.onDragging += dragSpin3DAvatar.DragSpin;
        ui_dragevent.onDragging += SpinDizzy;
        ui_dragevent.onClicked += PlayAttack;

        //System.Random rand = GameUtils.GetRandomGenerator();
        //TalentType random = (TalentType)rand.Next(1, 4);
        //SPSToggle[(int)random - 1].isOn = true;
        //UpdateSelectedStyle(random);
    }

    public void SelectKnife(bool isOn)
    {
        if (isOn)
            UpdateSelectedCharacter(JobType.Warrior);
    }
    public void SelectSword(bool isOn)
    {
        if (isOn)
            UpdateSelectedCharacter(JobType.Soldier);
    }
    public void SelectSpear(bool isOn)
    {
        if (isOn)
            UpdateSelectedCharacter(JobType.Tactician);
    }
    public void SelectHammer(bool isOn)
    {
        if (isOn)
            UpdateSelectedCharacter(JobType.Killer);
    }

    void AfterLoadModel(GameObject model, JobType type)
    {
        var ec = model.AddComponent<EffectController>();
        ec.Anim = model.GetComponent<Animation>();
        ec.ShowAnim(true);
        ec.ShowEfx(true);

        list_perform.Add(PlayShow(type));
    }

    void UpdateSelectedCharacter(JobType type)
    {
        StopStunCoroutine();
        StopAtkCoroutine();
        atkCoroutine = StartCoroutine(ProcessAtkTask());

        //JobsectJson jobJson = JobSectRepo.GetJobByType(type);
        //avatarModel.Change(jobJson, (model) => AfterLoadModel(model, type));

        jobSelected = type;

        var ccjson = JobSectRepo.GetJobByType(jobSelected);
        if (ccjson == null)
            return;

        performanceDic.Clear(); 
        dragSpin3DAvatar.Reset();
        //PlayIdle();
    }

    void AddToPerformDict(int atkIndex, int skillIndex)
    {
        var data = SkillRepo.GetSkill(skillIndex);
        if (data != null)
        {
            //performanceDic.Add(atkIndex, new EffectPerform() { Effect = data.skillgroupJson.name, Anim = data.skillgroupJson.action, Duration = data.skillgroupJson.skillduration });
        }
    }

    Dictionary<int, EffectPerform> performanceDic = new Dictionary<int, EffectPerform>();
    class EffectPerform
    {
        public string Effect, Anim;
        public float Duration;
    }

    public JobType GetSelectedJob()
    {
        return jobSelected;
    }

    public void ExitCharacterCreation()
    {
        PhotonNetwork.networkingPeer.Disconnect();
    }

    FactionType selected_faction = FactionType.None;
    FactionType current_faction = FactionType.None;
    public void SetFaction(FactionType faction)
    {
        current_faction = faction;
    }
    public void ConfirmFaction()
    {
        selected_faction = current_faction;
    }
    public FactionType GetSelectedFaction()
    {
        return selected_faction;
    }

    string selected_skill = "";
    string current_skill = "";
    public void SetSkill(string skill)
    {
        current_skill = skill;
    }
    public void ConfirmSkill()
    {
        selected_skill = current_skill;
    }

    public void OnPickRandomName()
    {
        string name;
        name = CharacterCreationRepo.GetRandomName();
        inputField.GetComponent<InputField>().text = name;
        current_name = name;
    }

    public void OnEndEditingName(Text inputfield)
    {
        current_name = inputfield.text;
    }

    public void OnCharCreation()
    {
        if (current_name == null || current_name == "")
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CharCreation_EnterName", null));
            return;
        }
        string filteredTxt = "";
        if (WordFilterRepo.FilterString(current_name, '*', FilterType.Naming, out filteredTxt))
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CharCreation_ForbiddenWord", null));
            return;
        }

        if (jobSelected == JobType.Newbie)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CharCreation_ChooseJob", null));
            return;
        }

        if (current_name.Length > 10)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CharCreation_NameToLong", null));
            return;
        }
        selected_faction = FactionType.Dragon;
        if (selected_faction == FactionType.None)
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CharCreation_ChooseFaction", null));
            return;
        }

        GameInfo.gLobby.InsertCharacter(current_name, (byte)jobSelected, 0, (byte)selected_faction, initial_skill);
        Debug.Log(string.Format("Inserting character name: {0} class: {1}", current_name, jobSelected));
    }

    public void OpenHelpSkill()
    {
        UIManager.OpenOkDialog(GUILocalizationRepo.GetLocalizedString("createchar_helpskill"), CloseYesNoDialog);
    }

    void CloseYesNoDialog()
    {
        UIManager.CloseDialog(WindowType.DialogYesNoOk);
    }

    public void OpenHelpTalent()
    {
        //UIManager.OpenOkDialog(GUILocalizationRepo.GetLocalizedString("createchar_helptalent"), CloseYesNoDialog);
    }

    public void OpenHelpFaction()
    {
       // UIManager.OpenOkDialog(GUILocalizationRepo.GetLocalizedString("createchar_helpfaction"), CloseYesNoDialog);
    }

    //-----------------------------------Animation and effects-----------------------------------

    //-----------------------------------Stun-----------------------------------
    [Header("Stun"), SerializeField]
    float dizzylimit = 5000.0f;
    [SerializeField]
    float dizzyPeriod = 2.0f;

    Coroutine stunCoroutine;
    float curDizzy = 0;

    bool isPlayingAtk = false, isPlayingStun = false;
    void SpinDizzy(Vector2 delta)
    {
        if (!isPlayingStun)
        {
            curDizzy += delta.x > 0 ? delta.x : -delta.x;
            if (!isPlayingAtk && curDizzy > dizzylimit)
            {
                StopStunCoroutine();
                curDizzy = 0;
                stunCoroutine = StartCoroutine(PlayStun(jobSelected));
            }
        }
    }

    void StopStunCoroutine()
    {
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = null;
        isPlayingStun = false;
        curDizzy = 0;
    }

    IEnumerator PlayStun(JobType job)
    {
        isPlayingStun = true;
        GameObject outfitModel = avatarModel.GetOutfitModel();
        if (outfitModel != null)
        {
            var anim = outfitModel.GetComponent<Animation>();
            if (anim != null)
            {
                EffectController ec = anim.GetComponent<EffectController>();

                //hardcoded, refer to EffectManager.mEffectPaths
                string stunEffectString = "";
                if (job == JobType.Warrior)
                    stunEffectString = "stunkn";
                else if (job == JobType.Killer)
                    stunEffectString = "stunha";
                else if (job == JobType.Tactician)
                    stunEffectString = "stunsp";
                else if (job == JobType.Soldier)
                    stunEffectString = "stunsw";

                if (ec != null)
                    ec.PlayEffect("stun",stunEffectString);

                yield return new WaitForSeconds(dizzyPeriod);

                //finished animation!
                ec.StopEffect(stunEffectString);
                ec.PlayEffect("standby",  "");
            }
        }
        yield return null;
        isPlayingStun = false;
    }

    //-----------------------------------Attacks-----------------------------------
    int index = 0;
    Coroutine atkCoroutine;
    List<IEnumerator> list_perform = new List<IEnumerator>();
    void PlayAttack()
    {
        if (!isPlayingStun)
        {
            //if is not playing stun
            if (list_perform.Count < 5)
            {
                ++index;
                if (index >= performanceDic.Count) index = 0;
                list_perform.Add(PlayAtkCoroutine(index));
            }
        }
        //StopAnimCoroutine();
        //animCoroutine = StartCoroutine(PlayAtkCoroutine(index));
    }


    IEnumerator PlayAtkCoroutine(int _index)
    {
        yield break;
        isPlayingAtk = true;
        GameObject outfitModel = avatarModel.GetOutfitModel();
        if (outfitModel != null)
        {
            var anim = outfitModel.GetComponent<Animation>();
            if (anim != null)
            {
                EffectController ec = anim.gameObject.GetComponent<EffectController>();
                if (ec != null)
                    ec.PlayEffect(performanceDic[_index].Anim, performanceDic[_index].Effect, null, performanceDic[_index].Duration);
                yield return new WaitForSeconds(performanceDic[_index].Duration);
            }
        }
        yield return null;
    }

    void PlayStandby()
    {
        GameObject outfitModel = avatarModel.GetOutfitModel();
        if (outfitModel != null)
        {
            var anim = outfitModel.GetComponent<Animation>();
            if (anim != null)
            {
                EffectController ec = anim.gameObject.GetComponent<EffectController>();
                if (ec != null)
                    ec.PlayEffect("standby",   "" );
            }
        }
        isPlayingAtk = false;
    }

    [SerializeField]
    List<float> showAnimationDuration = new List<float>();
    [SerializeField]
    List<string> showEffectString = new List<string>();
    IEnumerator PlayShow(JobType type)
    {
        GameObject outfitModel = avatarModel.GetOutfitModel();
        if (outfitModel != null)
        {
            var anim = outfitModel.GetComponent<Animation>();
            if (anim != null)
            {
                EffectController ec = anim.gameObject.GetComponent<EffectController>();
                if (ec != null)
                    ec.PlayEffect("show",  showEffectString[(int)type - 1]);
                yield return new WaitForSeconds(showAnimationDuration[(int)type-1]);
            }
        }
        yield return null;
    }

    IEnumerator ProcessAtkTask()
    {
        while (true)
        {
            while (list_perform.Count > 0)
            {
                yield return StartCoroutine(list_perform[0]);
                list_perform.RemoveAt(0);
                if (list_perform.Count == 0)
                {
                    PlayStandby();
                }
            }
            yield return null;
        }
    }

    void StopAtkCoroutine()
    {
        if (atkCoroutine != null)
            StopCoroutine(atkCoroutine);
        atkCoroutine = null;
        isPlayingAtk = false;
        list_perform.Clear();
    }
}
