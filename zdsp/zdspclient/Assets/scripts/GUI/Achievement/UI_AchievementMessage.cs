using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_AchievementMessage : MonoBehaviour
{
    [SerializeField] Text messageText;

    private Queue<string> buffer = new Queue<string>();
    private bool show = true;

    public void ShowMessage(AchievementKind type, int id)
    {
        BaseAchievementObjective obj = AchievementRepo.GetObjectiveByTypeAndId(type, id);
        if (obj != null)
        {
            string prefix = type == AchievementKind.Collection ? "ach_collection_completed" : "ach_achievement_completed";
            buffer.Enqueue(GUILocalizationRepo.GetLocalizedString(prefix) + obj.localizedName);
            if (!gameObject.activeInHierarchy && show)
                gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (buffer.Count > 0 && show)
            messageText.text = buffer.Dequeue();
    }

    // Called by animation on last frame
    public void OnFinished()
    {
        if (buffer.Count > 0 && show)
            gameObject.SetActive(true);
    }

    public void EnableShowMessages(bool value)
    {
        show = value;
        if (show && buffer.Count > 0 && !gameObject.activeInHierarchy)
            gameObject.SetActive(true);
    }
}
