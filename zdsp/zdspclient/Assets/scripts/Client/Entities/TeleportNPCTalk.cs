using UnityEngine;
using System.Collections;


public class TeleportNPCTalk : MonoBehaviour
{
    public string Archetype;
    // Use this for initialization
    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter(Collider collider)
    {
        if (Archetype == null)
            return;
        TeleportNPCData.Archetype = Archetype;

        if (collider.tag == "LocalPlayer")
        {
            GameInfo.gLocalPlayer.ForceIdle();
            GameInfo.gLocalPlayer.Bot.StopBot();
            GameInfo.ResetJoystick();
            //UIManager.OpenWindow(WindowType.TeleportNPCTalk);
        }
    }
}

public struct TeleportNPCData
{
    public static string Archetype;    
}