using UnityEngine;

public class FirstGuideRealmQuestTrigger : MonoBehaviour
{
    public int questRefId;
    public GameObject playerCollider;

    private bool colliderEnabled;

    void Start()
    {
        EnablePlayerCollider(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (colliderEnabled && other.CompareTag("LocalPlayer"))
        {
            //if (GameInfo.gLocalPlayer.QuestStats.mainId == questRefId)
            //{
            //    EnablePlayerCollider(false);
            //}
        }
    }

    private void EnablePlayerCollider(bool show)
    {
        playerCollider.SetActive(show);
        colliderEnabled = show;
    }
}
