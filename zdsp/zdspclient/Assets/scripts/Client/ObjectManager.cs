using System.Collections.Generic;
using UnityEngine;

public enum OBJTYPE : byte
{
    MODEL,
    EFFECT,
    TEXTURE,
}

public static class ObjPoolMgr
{
    public static ObjectPoolManager Instance;

    static ObjPoolMgr()
    {
        Instance = new ObjectPoolManager();
    }
}

public class ObjectPoolManager
{
    private Dictionary<string, GameObject> mModels;
    private Dictionary<string, GameObject> mEffects;
    private Dictionary<string, Object> mTextures;

    public ObjectPoolManager()
    {
        mModels = new Dictionary<string, GameObject>();
        mEffects = new Dictionary<string, GameObject>();
        mTextures = new Dictionary<string, Object>();
    }

    public GameObject GetObject(OBJTYPE objtype, string path, bool usercontainer = false)
    {
        if (objtype == OBJTYPE.MODEL)
        {
            GameObject orig = null;
            if (mModels.ContainsKey(path) == false)
            {
                if (usercontainer == false)
                    orig = (GameObject)Object.Instantiate(Resources.Load(path));
                else
                    orig = AssetManager.LoadAsset<GameObject>(path);
                if (orig == null)
                    return null;
                mModels[path] = orig;
            }
            return Object.Instantiate(mModels[path]);
        }
        return null;
    }

    public Object GetTexture(string path)
    {
        if (mTextures.ContainsKey(path) == false)
        {
            Object obj = AssetManager.LoadAsset<Object>(path);
            mTextures.Add(path, obj);
        }
        return mTextures[path];
    }

    public void Cleanup()
    {
        mModels.Clear();
        mEffects.Clear();
        mTextures.Clear();
    }
}

public static class ObjMgr
{
    public static ObjectManager Instance;

    static ObjMgr()
    {
        Instance = new ObjectManager();
    }
}

public class ObjectManager
{
    public List<GameObject> InitGameObjectPool(Transform parent, GameObject inst, Vector3 localpos, Vector3 localscale, int poolsize = 10)
    {
        List<GameObject> retval = new List<GameObject>();

        for (int i = 0; i < poolsize; ++i)
        {
            GameObject newinst = Object.Instantiate(inst, Vector3.zero, Quaternion.identity);
            newinst.transform.SetParent(parent, false);
            newinst.transform.localPosition = localpos;
            newinst.transform.localScale = localscale;

            newinst.SetActive(false);
            retval.Add(newinst);
        }
        return retval;
    }

    public Queue<GameObject> InitGameObjectPoolQueue(Transform parent, GameObject inst, Vector3 localpos, Vector3 localscale, int poolsize = 10)
    {
        Queue<GameObject> retval = new Queue<GameObject>();

        for (int i = 0; i < poolsize; ++i)
        {
            GameObject newinst = Object.Instantiate(inst, Vector3.zero, Quaternion.identity);
            newinst.transform.SetParent(parent, false);
            newinst.transform.localPosition = localpos;
            newinst.transform.localScale = localscale;

            newinst.SetActive(false);
            retval.Enqueue(newinst);
        }
        return retval;
    }

    public GameObject GetContainerObject(List<GameObject> container)
    {
        for (int i = 0; i < container.Count; ++i)
        {
            GameObject obj = container[i];
            if (!obj.activeSelf)
                return obj;
        }
        return null;
    }

    public List<GameObject> GetActiveContainerObjects(List<GameObject> container)
    {
        List<GameObject> retlist = new List<GameObject>();
        for (int i = 0; i < container.Count; ++i)
        {
            GameObject obj = container[i];
            if (obj.activeSelf)
                retlist.Add(obj);
        }
        return retlist;
    }

    public void ResetContainerObject(List<GameObject> container)
    {
        for (int i = 0; i < container.Count; ++i)
        {
            container[i].SetActive(false);
        }
    }

    public void DestroyContainerObject(List<GameObject> container)
    {
        for (int i = 0; i < container.Count; ++i)
        {
            Object.Destroy(container[i]);
        }
    }

    public void DestroyContainerObject(Queue<GameObject> container)
    {
        while (container.Count != 0)
        {
            Object.Destroy(container.Dequeue());
        }
    }
}