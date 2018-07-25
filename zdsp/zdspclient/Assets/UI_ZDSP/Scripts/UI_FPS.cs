using UnityEngine;
using System.Collections;

public class UI_FPS : MonoBehaviour
{
    //Modified version of FPS.cs
    //Right Click on Hierarchy, ZDGUI/FPS
    //This will add the FPS prefab, sorting layer in very very high, should be always on top.

    UnityEngine.UI.Text fpsText;
    public float updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    void Awake()
    {
        //Application.targetFrameRate = 60;
        //if (PhotonNetwork.PhotonServerSettings.LoginServerType == LoginServerIp.Live)
        //{
        //    gameObject.SetActive(false);
        //}
    }

    // Use this for initialization
    void Start()
    {
        fpsText = GetComponent<UnityEngine.UI.Text>();
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            int fps = (int)accum / frames;
            //print ("fps = " + fps);
            fpsText.text = string.Format("{0}", fps);

            if (fps < 30)
                fpsText.color = Color.yellow;
            else if (fps < 10)
                fpsText.color = Color.red;
            else
                fpsText.color = Color.green;
            
            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }

    }
}
