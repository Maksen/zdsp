using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    [SerializeField]
    int time;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(time);
        yield return SceneManager.LoadSceneAsync("Dialog_SplashLoadingScreen", LoadSceneMode.Single);
    }
}
