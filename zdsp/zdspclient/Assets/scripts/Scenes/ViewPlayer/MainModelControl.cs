using UnityEngine;

namespace ViewPlayer
{
    public class MainModelControl
    {
        protected GameObject mModel = null;
        protected EffectController mEffectController = null;

        public GameObject GetModel()
        {
            return mModel;
        }

        public void SetModel(GameObject model)
        {
            OnDestroyGameObject();

            mModel = model;
            mEffectController = model.GetComponent<EffectController>();
        }

        public void RemoveModel()
        {
            OnDestroyGameObject();
        }

        public bool AddEffectByLoadFile(string file_name, GameObject efxObj)
        {
#if UNITY_EDITOR
            if (efxObj != null)
            {
                // 以下是從EffectController.AddEffect裡面抄過來的, 要是改了就....
                EffectRef efxRef = efxObj.GetComponent<EffectRef>();
                efxRef.Attach(mEffectController.gameObject);
                efxRef.Deactive();
                //string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                mEffectController.mEfxMap.Add(file_name, efxRef);


                //string fileName = System.IO.Path.GetFileNameWithoutExtension(effectpath);
                //EfxSystem.Instance.AddEffectToEfxList(fileName, effectpath);

                return true;
            }

            return false;
#else
        return false;
#endif
        }

        public virtual void PlayEffect(string animation, string efxname, float duration = -1)
        {
            if (mEffectController)
            {
                mEffectController.PlayEffect(animation, efxname, null, duration);
            }
        }

        public void StopEffect(string efxname)
        {
            mEffectController.StopEffect(efxname);
        }

        void OnDestroyGameObject()
        {
            if (mModel)
                mModel.name = mModel.name + "_Expired";
            Object.Destroy(mModel);
            //mModel = null;
            //mEffectController = null;
        }

        void OnDestroy()
        {
            OnDestroyGameObject();

            mModel = null;
            mEffectController = null;
        }
    }
}
