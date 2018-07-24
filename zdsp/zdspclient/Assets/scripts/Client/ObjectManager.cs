using UnityEngine;
using System.Collections.Generic;

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

    public GameObject GetObject(OBJTYPE objtype, string path, bool usercontainer=false)
    {
        if (objtype == OBJTYPE.MODEL)
        {
            GameObject orig = null;
            if (mModels.ContainsKey(path) == false)
            {
                if (usercontainer == false)
                    orig = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(path));
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
            Object obj = (Object)AssetManager.LoadAsset<Object>(path);
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


public class ObjectManager{

    public List<GameObject> InitGameObjectPool(Transform parent, GameObject inst, Vector3 localpos, Vector3 localscale, int poolsize = 10)
    {
        List<GameObject> retval = new List<GameObject>();

        for (int i = 0; i < poolsize; i++)
        {            
            GameObject newinst = (GameObject)UnityEngine.Object.Instantiate(inst, Vector3.zero, Quaternion.identity);
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

        for (int i = 0; i < poolsize; i++)
        {
            GameObject newinst = (GameObject)UnityEngine.Object.Instantiate(inst, Vector3.zero, Quaternion.identity);
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
        foreach (GameObject val in container)
        {
            if (val.activeSelf == false)
                return val;
        }
        return null;
    }

    public void ResetContainerObject(List<GameObject> container)
    {
        foreach (GameObject val in container)
        {
            val.SetActive(false); 
        }
    }

    public void DestroyContainerObject(List<GameObject> container)
    {
        foreach (GameObject val in container)
        {
            UnityEngine.Object.Destroy(val);
        }
    }

    public void DestroyContainerObject(Queue<GameObject> container)
    {
        while (container.Count != 0)
        {
            UnityEngine.Object.Destroy(container.Dequeue());
        }
    }
}
