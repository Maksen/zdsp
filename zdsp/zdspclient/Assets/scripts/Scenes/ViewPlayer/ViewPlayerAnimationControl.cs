using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UIWidgets;
using Kopio.JsonContracts;
using Zealot.Repository;
#if UNITY_EDITOR
using UnityEditor;
# endif

namespace ViewPlayer
{
    public class ViewPlayerAnimationControl : MonoBehaviour
    {

        public GameObject inputAnimatName;
        public GameObject inputEffectName;
        public GameObject inputDuration;
        public GameObject mainObject;

        public GameObject listViewMain;
        public GameObject listViewTarget;
        public GameObject prototypeListViewItem;

        private bool isAutoStandby = false;
        private bool isPlayLoop = false;
        int totalItemId = 0;
        GameObject curListView;
        GameObject customList;
        Coroutine playAnimation;

        private string mAnimateName = "";
        Vector3 listPos = Vector3.zero;
        Vector3 hidePos = new Vector3(-999f, -999f, 0f);

        public bool PlayLoop
        {
            get { return isPlayLoop; }
            set { isPlayLoop = value; }
        }

        public bool AutoStandby
        {
            get { return isAutoStandby; }
            set { isAutoStandby = value; }
        }

        void Start()
        {
            //customList = listViewMain.transform.Find("Mask").Find("List").gameObject;
        }

        void Update()
        {

        }

        void SetCombox(List<string> custom_cb2_list = null)
        {
            GameObject model = ViewPlayerModelControl.Instance.GetCurrectSelectModel();
            if (model)
            {
                Combobox combox1 = inputAnimatName.GetComponent<Combobox>();
                if (combox1)
                {
                    combox1.Clear();
                    Animation animation = model.GetComponent<Animation>();
                    if (animation.GetClipCount() > 0)
                    {
                        foreach (AnimationState state in animation)
                        {
                            combox1.Set(state.name);
                        }
                        combox1.ListView.Select(0);
                    }
                }

                Combobox combox2 = inputEffectName.GetComponent<Combobox>();
                if (combox2 && SkillRepo.mSkillGroupsRaw != null)
                {
                    combox2.Clear();
                    foreach (KeyValuePair<int, SkillGroupJson> entry in SkillRepo.mSkillGroupsRaw)
                    {
                        SkillGroupJson skillgroup = entry.Value;
                        combox2.Set(skillgroup.name);
                    }

                    if (custom_cb2_list != null && custom_cb2_list.Count > 0)
                    {
                        foreach(string effect_name in custom_cb2_list)
                        {
                            combox2.Set(effect_name);
                        }
                    }
                    combox2.ListView.Select(0);
                }
            }
        }

        public void OnModelLoaded()
        {
            SetCombox();

            inputEffectName.GetComponent<InputField>().text = "";
            GUI.FocusControl("");

            if (listPos == Vector3.zero)
                listPos = listViewMain.transform.position;

            if (curListView == null)
                ChangeSelectModel(ViewPlayerModelControl.Instance.IsSelectMain);
        }

        public void ChangeSelectModel(bool isMain)
        {
            //Debug.Log("ViewPlayerAnimationControl.ChangeSelectModel");
            if (isMain)
            {
                curListView = listViewMain;
                listViewMain.transform.position = listPos;
                listViewTarget.transform.position = hidePos;
            }
            else
            {
                curListView = listViewTarget;
                listViewMain.transform.position = hidePos;
                listViewTarget.transform.position = listPos;
            }

            customList = curListView.transform.Find("Mask").Find("List").gameObject;
            ResetPlaying();
            SetCombox(ViewPlayerModelControl.Instance.GetLoadedEffectName());
        }

        public void LoadEffectFile()
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("select .prefeb file of model", "Assets/", "prefab");
            if (path.Length > 0)
            {
                int subLen = Application.dataPath.Length - 6; // Assets/開始
                string effectpath = path.Substring(subLen, path.Length - subLen).Replace("\\", "/");
                GameObject inst = AssetDatabase.LoadAssetAtPath<GameObject>(effectpath) as GameObject;
                if (inst != null)
                {
                    GameObject efxObj = GameObject.Instantiate(inst) as GameObject;
                    if (efxObj != null)
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                        subLen = ("Assets/").Length;
                        // 以下是從EffectController.AddEffect裡面抄過來的, 要是改了就....
                        efxObj.name = effectpath.Substring(subLen, effectpath.Length - subLen);
                        efxObj.SetActive(true);
                        efxObj.layer = 9;//enities layer
                        bool result = ViewPlayerModelControl.Instance.AddEffectByLoadFile(fileName, efxObj);
                        if (result)
                        {
                            Combobox combox2 = inputEffectName.GetComponent<Combobox>();
                            if (combox2)
                            {
                                combox2.Set(fileName);
                            }
                        }
                        else
                        {
                            mainObject.SendMessage("ShowDialog", "Try to add effect failed !", SendMessageOptions.DontRequireReceiver);
                        }
                    }

                }
                else
                {
                    mainObject.SendMessage("ShowDialog", "Load effect failed !", SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                mainObject.SendMessage("ShowDialog", "Load effect file error !", SendMessageOptions.DontRequireReceiver);
            }
#endif
        }

        void ResetPlaying()
        {
            if (playAnimation != null)
            {
                StopCoroutine(playAnimation);
                playAnimation = null;
            }

            //StopEffect();
        }

        void StopEffect()
        {
            if (mAnimateName.Length != 0)
            {
                ViewPlayerModelControl.Instance.StopEffect(mAnimateName);
            }
        }
        public void OnPlayAnimation()
        {
            ResetPlaying();
            playAnimation = StartCoroutine(PlayAnimation());
        }

        IEnumerator PlayAnimation()
        {
            ResetPlaying();

            mAnimateName = inputAnimatName.GetComponent<InputField>().text;
            string efname = inputEffectName.GetComponent<InputField>().text;
            if (mAnimateName.Length == 0)
            {
                mainObject.SendMessage("ShowDialog", "Need animation name !", SendMessageOptions.DontRequireReceiver);
                yield break;
            }
            if (efname.Length == 0)
            {
                efname = mAnimateName;
                inputEffectName.GetComponent<InputField>().text = mAnimateName;
            }
            string strDuration = inputDuration.GetComponent<InputField>().text;
            float floatDuration = -1.0f;
            if (strDuration.Length != 0)
            {
                floatDuration = Mathf.Max(float.Parse(strDuration), floatDuration);
            }
            ViewPlayerModelControl.Instance.PlayEffect(mAnimateName, efname, -1);

            yield return new WaitForSeconds(floatDuration);

            if (isPlayLoop)
                OnPlayAnimation();

            if (isAutoStandby)
                OnStopAnimation();
        }

        public void OnStopAnimation()
        {
            ResetPlaying();
            ViewPlayerModelControl.Instance.PlayEffect("standby", "standby");
        }

        bool CheckAnimateAndEffectName(string animate_name, string effect_name, out string last_effect_name)
        {
            last_effect_name = "";

            if (animate_name.Length == 0)
            {
                mainObject.SendMessage("ShowDialog", "Need animation name !", SendMessageOptions.DontRequireReceiver);
                return false;
            }

            if (effect_name.Length == 0)
            {
                last_effect_name = animate_name;
            }
            else
            {
                last_effect_name = effect_name;
            }

            return true;
        }


        bool CheckDuration(string duration, out float duration_time)
        {
            duration_time = 0.1f;
            if (duration.Length != 0)
            {
                duration_time = Mathf.Max(float.Parse(duration), duration_time);
            }
            return true;
        }

        public void OnAddAnimateList()
        {
            string amname = inputAnimatName.GetComponent<InputField>().text;
            string efname;
            if (!CheckAnimateAndEffectName(amname, inputEffectName.GetComponent<InputField>().text, out efname))
            {
                return;
            }

            string strDuration = inputDuration.GetComponent<InputField>().text;
            float floatDuration;
            if (!CheckDuration(strDuration, out floatDuration))
            {
                return;
            }

            GameObject item = GameObject.Instantiate(prototypeListViewItem) as GameObject;
            ViewPlayerAnimateListViewItem itemScript = item.GetComponent<ViewPlayerAnimateListViewItem>();
            itemScript.Init(this);
            itemScript.AddItems(amname, efname, floatDuration);
            item.transform.parent = customList.transform;
            item.SetActive(true);
            item.name = "LVI" + (++ViewPlayerModelControl.Instance.CurrentSelectModelListItemId);
            item.transform.localScale = new Vector3(1, 1, 1);
        }

        public void RemoveListItem(string item_name)
        {
            GameObject item = customList.transform.Find(item_name).gameObject;
            if (item)
            {
                item.transform.parent = null;
                Destroy(item);
            }
        }

        public bool OnAnimateListViewItemEdit(string item_name)
        {
            GameObject item = customList.transform.Find(item_name).gameObject;
            if (item)
            {
                ViewPlayerAnimateListViewItem itemScript = item.GetComponent<ViewPlayerAnimateListViewItem>();
                if (itemScript)
                {
                    string effect_name;
                    string animate_name = itemScript.GetAnimationName();
                    if (!CheckAnimateAndEffectName(animate_name, itemScript.GetEffectName(), out effect_name))
                        return false;

                    float duration;
                    if (!CheckDuration(itemScript.GetDurationTime(), out duration))
                    {
                        return false;
                    }

                    itemScript.AddItems(animate_name, effect_name, duration);
                }
            }

            return true;
        }

        public void OnListPlay()
        {
            ResetPlaying();
            playAnimation = StartCoroutine(PlayByList());
        }

        float PlayAnimationByListItem(GameObject item, MainModelControl custom_effect_player = null)
        {
            if (item && item.GetActive())
            {
                ViewPlayerAnimateListViewItem itemScript = item.GetComponent<ViewPlayerAnimateListViewItem>();
                if (itemScript)
                {
                    float duration = float.Parse(itemScript.GetDurationTime());
                    if (custom_effect_player != null)
                        custom_effect_player.PlayEffect(itemScript.GetAnimationName(), itemScript.GetEffectName(), -1);
                    else
                        ViewPlayerModelControl.Instance.PlayEffect(itemScript.GetAnimationName(), itemScript.GetEffectName(), -1);

                    Selectable se = item.GetComponent<Selectable>();
                    se.Select();
                    return duration;
                }
                //Debug.Log("scrollbar.numberOfSteps : " + scrollbar.numberOfSteps);
            }

            return -999;
        }

        IEnumerator PlayByList()
        {
            var customList1 = listViewMain.transform.Find("Mask").Find("List").gameObject;
            var customList2 = listViewTarget.transform.Find("Mask").Find("List").gameObject;
            int count1 = customList1.transform.childCount;
            int count2 = customList2.transform.childCount;
            //int count = customList.transform.childCount;
            ListView lvScript = curListView.GetComponent<ListView>();
            Scrollbar scrollbar = lvScript.ScrollRect.verticalScrollbar;

            for (int i = 0; i < count1; i++)
            {
                GameObject item1 = customList1.transform.GetChild(i).gameObject;
                var duration = PlayAnimationByListItem(item1, ViewPlayerModelControl.Instance.GetMainControl());
                if (duration != -999)
                {
                    if (i < count2)
                    {
                        GameObject item2 = customList2.transform.GetChild(i).gameObject;
                        PlayAnimationByListItem(item2, ViewPlayerModelControl.Instance.GetTargetControl());
                    }
                    yield return new WaitForSeconds(duration);
                }
            }

            if (isPlayLoop)
                OnListPlay();

            if (isAutoStandby)
                OnStopAnimation();
        }

        void OnDestroy()
        {
            inputAnimatName = null;
            inputEffectName = null;
            inputDuration = null;
            mainObject = null;
            listViewMain = null;
            curListView = null;
            listViewTarget = null;
            prototypeListViewItem = null;
            customList = null;
        }
    }
}
