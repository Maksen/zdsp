using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var type = typeof(T);
                var objects = FindObjectsOfType<T>();

                if (objects.Length > 0)
                {
                    _instance = objects[0];
                    if (objects.Length > 1)
                    {
                        Debug.LogWarning("There is more than one instance of Singleton of type \"" + type + "\". Keeping the first. Destroying the others.");
                        for (var i = 1; i < objects.Length; i++)
                            DestroyImmediate(objects[i].gameObject);
                    }
                    return _instance;
                }

                var gameObject = new GameObject();
                gameObject.name = type.ToString();

                _instance = gameObject.AddComponent<T>();
                DontDestroyOnLoad(gameObject);
            }
            return _instance;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
            _instance = null;
        } 
    }
}


public abstract class LevelMonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var type = typeof(T);
                var objects = FindObjectsOfType<T>();

                if (objects.Length > 0)
                {
                    _instance = (T)objects[0];
                    if (objects.Length > 1)
                    {
                        Debug.LogWarning("There is more than one instance of Singleton of type \"" + type + "\". Keeping the first. Destroying the others.");
                        for (var i = 1; i < objects.Length; i++)
                            DestroyImmediate(objects[i].gameObject);
                    }
                     
                }
                else
                {
                    Debug.LogError("No Singleton of  type \"" + type + "\". found in the level, which is WRONG.");

                } 
            }
            if(_instance == null)
                Debug.LogError("No Singleton of  type \"" + typeof(T) + "\". found in the level, which is WRONG.");

            return _instance;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
            _instance = null;
        }
    }
}