using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Zealot.Repository;

public class Localizer_Tab : MonoBehaviour
{
    // Text component to edit
    private UIWidgets.Tabs tabs;

    // String to localize
    public UIWidgets.Tab[] tabsToEdit = new UIWidgets.Tab[] {};

    void Awake ()
    {
        bool resTabs = CheckForTabs();
        if (!resTabs)
        {
            Debug.Log("No Tabs found! Please make sure the component you attach to exists!");
            return;
        }
        else
        {
            UpdateTabName();
        }
    }

    bool CheckForTabs()
    {
        tabs = gameObject.GetComponent<UIWidgets.Tabs>();
        if(tabs == null)
        {
            tabs = gameObject.GetComponentInChildren<UIWidgets.Tabs>(true);
            if (tabs == null)
            {
                Debug.Log("Tabs not found!");
                return false;
            }
        }

        return true;
    }

    void UpdateTabName()
    {
        UIWidgets.Tab[] tabstoFind = tabs.TabObjects;
        int tabsToEditLen = tabsToEdit.Length;
        List<Button> activeButtons = tabs.activeButtons;
        List<Button> defaultButtons = tabs.defaultButtons;
        // Check if tab buttons already exist
        if(activeButtons.Count == tabsToEditLen && defaultButtons.Count == tabsToEditLen)
        {
            for(int i=0; i<tabsToEditLen; ++i)
            {
                string localizedTxt = GUILocalizationRepo.GetLocalizedString(tabsToEdit[i].Name);
                Text[] activeText = activeButtons[i].GetComponentsInChildren<Text>(true);
                activeText[0].text = localizedTxt;
                Text[] defaultText = defaultButtons[i].GetComponentsInChildren<Text>(true);
                defaultText[0].text = localizedTxt;
            }
        }
        else
        {
            foreach(UIWidgets.Tab tabFind in tabstoFind)
            {
                foreach(UIWidgets.Tab tabEdit in tabsToEdit)
                {
                    if(tabFind.TabObject.Equals(tabEdit.TabObject))
                    {
                        tabFind.Name = GUILocalizationRepo.GetLocalizedString(tabEdit.Name);
                    }
                }
            }
        }
    }
}
