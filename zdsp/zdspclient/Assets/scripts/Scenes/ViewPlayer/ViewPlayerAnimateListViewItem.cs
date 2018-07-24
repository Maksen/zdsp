using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ViewPlayer
{
    public class ViewPlayerAnimateListViewItem : MonoBehaviour
    {

        InputField animateName;
        InputField effectName;
        InputField durationTime;
        GameObject ctrlEdit;
        GameObject ctrlNormal;
        int id;

        ViewPlayerAnimationControl controller;

        // Use this for initialization
        void Start()
        {

        }

        public void Init(ViewPlayerAnimationControl controller)
        {
            this.controller = controller;
            animateName = this.transform.Find("PALVIanimation").GetComponent<InputField>();
            effectName = this.transform.Find("PALVIeffect").GetComponent<InputField>();
            durationTime = this.transform.Find("PALVIduration").GetComponent<InputField>();
            ctrlNormal = this.transform.Find("Panel").Find("PanelNormal").gameObject;
            ctrlEdit = this.transform.Find("Panel").Find("PanelEdit").gameObject;

            OpenEditMode(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddItems(string animation_name, string effect_name, float duration)
        {
            AddItems(animation_name, effect_name, duration.ToString());
        }

        public void AddItems(string animation_name, string effect_name, string duration)
        {
            animateName.text = animation_name;
            effectName.text = effect_name;
            durationTime.text = duration;
        }

        public int GetId()
        {
            return id;
        }

        public string GetAnimationName()
        {
            return animateName.text;
        }

        public string GetEffectName()
        {
            return effectName.text;
        }

        public string GetDurationTime()
        {
            return durationTime.text;
        }

        void SetInputable(bool can_input)
        {
            animateName.interactable = can_input;
            effectName.interactable = can_input;
            durationTime.interactable = can_input;
        }

        void OpenEditMode(bool edit_mode)
        {
            if (edit_mode)
            {
                ctrlNormal.SetActive(false);
                ctrlEdit.SetActive(true);
                SetInputable(true);
            }
            else
            {
                ctrlNormal.SetActive(true);
                ctrlEdit.SetActive(false);
                SetInputable(false);
            }
        }

        public void OnEdit()
        {
            OpenEditMode(true);
        }

        public void OnDelete()
        {
            controller.RemoveListItem(this.name);
        }

        public void OnSave()
        {
            if (controller.OnAnimateListViewItemEdit(this.name))
            {
                OpenEditMode(false);
            }
        }

        public void OnUp()
        {

        }

        public void OnDown()
        {

        }

        void OnDestroy()
        {
            animateName = null;
            effectName = null;
            durationTime = null;
            ctrlEdit = null;
            ctrlNormal = null;
        }
    }
}