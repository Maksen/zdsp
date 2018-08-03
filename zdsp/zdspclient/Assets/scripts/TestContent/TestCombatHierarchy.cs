using UnityEngine;
using UnityEngine.SceneManagement;

public class TestCombatHierarchy : MonoBehaviour
{
    private void Start()
    {
        if (GameInfo.gCombat != null)
            Destroy(gameObject);
    }

}
