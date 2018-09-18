using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(Toggle))]
public class ToggleContent : MonoBehaviour
{
    Toggle toggle;

    public List<GameObject> list_content;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        SetContentOn(toggle.isOn);
        toggle.onValueChanged.AddListener(SetContentOn);
    }

    void SetContentOn(bool on)
    {
        if (on)
        {
            foreach (var content in list_content)
                content.SetActive(true);
        }
        else
        {
            var activeToggles = toggle.group.ActiveToggles();
            Dictionary<string, GameObject> activeGameObjects = new Dictionary<string, GameObject>();
            foreach (var active_toggle in activeToggles)
            {
                if (active_toggle == toggle)
                    continue;
                List<GameObject> contents = active_toggle.GetComponent<ToggleContent>().list_content;
                for (int index = 0; index < contents.Count; ++index)
                    activeGameObjects[contents[index].name] = contents[index];
            }
            foreach (var content in list_content)
            {
                if (activeGameObjects.ContainsKey(content.name) && activeGameObjects[content.name] == content)
                    continue;
                content.SetActive(false);
            }
        }
    }
}
