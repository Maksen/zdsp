using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
# endif

namespace ViewPlayer
{
    enum ModelType
    {
        Main,
        Target
    }

    public class ModelInfo
    {
        public MainModelControl modelControl;
        public GameObject listView;
        public int listItemID = 0;
        public Dictionary<string, GameObject> LoadedEffect;
        public GameObject Model
        {
            get {
                if (modelControl != null)
                    return modelControl.GetModel();
                return null;
            }
            set
            {
                if (modelControl != null)
                {
                    modelControl.RemoveModel();
                    modelControl.SetModel(value);
                }
                LoadedEffect = new Dictionary<string, GameObject>();
            }
        }

        public ModelInfo (MainModelControl mmc)
        {
            modelControl = mmc;
        }
     };


    public class ViewPlayerModelControl : MonoSingleton<ViewPlayerModelControl> {

        private MainModelControl mainModule;
        private TargetModelControl targetModule;
        private Dictionary<int, ModelInfo> modelInfos = new Dictionary<int, ModelInfo>();
        private ModelInfo curSelectModuleInfo = null;
        private bool isSelectMain = true;
        public bool IsSelectMain
        {
            get { return isSelectMain; }
        }

        public int CurrentSelectModelListItemId
        {
            get
            {
                if (curSelectModuleInfo != null)
                {
                    return curSelectModuleInfo.listItemID;
                }

                return 0;
            }

            set
            {
                if (curSelectModuleInfo != null)
                    curSelectModuleInfo.listItemID = value;
            }
            
        }

        void Start()
        {

        }

        void Update()
        {

        }

        public GameObject GetMainModel()
        {
            ModelInfo info;
            if (modelInfos.TryGetValue((int)ModelType.Main, out info))
                return info.Model;

            return null;
        }

        public MainModelControl GetMainControl()
        {
            ModelInfo info;
            if (modelInfos.TryGetValue((int)ModelType.Main, out info))
                return info.modelControl;

            return null;
        }

        public GameObject GetTargetModel()
        {
            ModelInfo info;
            if (modelInfos.TryGetValue((int)ModelType.Target, out info))
                return info.Model;

            return null;
        }

        public MainModelControl GetTargetControl()
        {
            ModelInfo info;
            if (modelInfos.TryGetValue((int)ModelType.Target, out info))
                return info.modelControl;

            return null;
        }

        public GameObject GetCurrectSelectModel()
        {
            if (curSelectModuleInfo != null)
                return curSelectModuleInfo.Model;

            return null;
        }

        public void SetMainModel(GameObject model)
        {
            ModelInfo info;
            if (modelInfos.TryGetValue((int)ModelType.Main, out info) == false)
            {
                info = new ModelInfo(new MainModelControl());
                modelInfos.Add((int)ModelType.Main, info);
            }
            info.Model = model;
            curSelectModuleInfo = info;
        }

        public void SetTargetModel(GameObject model)
        {
            ModelInfo info;
            if (modelInfos.TryGetValue((int)ModelType.Target, out info) == false)
            {
                info = new ModelInfo(new TargetModelControl());
                modelInfos.Add((int)ModelType.Target, info);
            }

            info.Model = model;
            curSelectModuleInfo = info;
        }

        public void SetModel(GameObject model)
        {
            if (isSelectMain)
                SetMainModel(model);
            else
                SetTargetModel(model);
        }

        public void RemoveModel()
        {
            foreach(KeyValuePair<int, ModelInfo> item in modelInfos)
            {
                item.Value.modelControl.RemoveModel();
            }
        }

        public void ChangeSelectModel(bool isMain)
        {
            //Debug.Log("ViewPlayerModelControl.ChangeSelectModel");
            if (isSelectMain == isMain)
                return;

            isSelectMain = isMain;
            var key = isMain ? ModelType.Main : ModelType.Target;
            ModelInfo info;
            if (modelInfos.TryGetValue((int)key, out info))
            {
                curSelectModuleInfo = info;
            }
            else
            {
                curSelectModuleInfo = null;
            }
        }

        public bool AddEffectByLoadFile(string file_name, GameObject efxObj)
        {
#if UNITY_EDITOR
            if (curSelectModuleInfo != null)
            {
                if (curSelectModuleInfo.LoadedEffect.ContainsKey(file_name) == false)
                    curSelectModuleInfo.LoadedEffect.Add(file_name, efxObj);

                return curSelectModuleInfo.modelControl.AddEffectByLoadFile(file_name, efxObj);
            }

            return false;
#else
            return false;
#endif
        }

        public List<string> GetLoadedEffectName()
        {
            if (curSelectModuleInfo != null)
                return curSelectModuleInfo.LoadedEffect.Keys.ToList();
            return null;
        }

        public void PlayEffect(string animation, string efxname, float duration = -1)
        {
            if (curSelectModuleInfo != null)
            {
                curSelectModuleInfo.modelControl.PlayEffect(animation, efxname, duration);
            }
        }

        public void StopEffect(string efxname)
        {
            if (curSelectModuleInfo != null)
                curSelectModuleInfo.modelControl.StopEffect(efxname);
        }
    }
}
