using UnityEngine;
using UnityEngine.SceneManagement;

public class TestCombatHierarchy : MonoBehaviour
{
    private void Start()
    {
        Scene scene = SceneManager.GetSceneByName("UI_CombatHierarchy");
        GameObject[] sceneObj = scene.GetRootGameObjects();
        for (int i = 0; i < sceneObj.Length; i++)
        {
            GameObject obj = sceneObj[i];
            if (obj.name == "Canvas_ssOverlay_FPS")
                DontDestroyOnLoad(obj);
        }

        if (GameInfo.gCombat != null)
            Destroy(gameObject);
    }

}
