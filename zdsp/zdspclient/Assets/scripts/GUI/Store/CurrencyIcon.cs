using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class CurrencyIcon : MonoBehaviour {

    Zealot.Common.CurrencyType type_ = Zealot.Common.CurrencyType.Money;

    public Zealot.Common.CurrencyType type
    {
        get { return type_; }

        set
        {
            type_ = value;
            UpdateIcon();
        }
    }

    public Dictionary<Zealot.Common.CurrencyType, GameObject> currency_icons = null;

    bool inited = false;
    void init()
    {
        if (inited == true)
            return;

        if (transform.childCount > 0)
        {
            currency_icons = new Dictionary<Zealot.Common.CurrencyType, GameObject>();
            var icons = GetComponentsInChildren<Image>(true);

            foreach (var icon in icons)
            {
                if (icon.gameObject.name == "coin")
                {
                    currency_icons.Add((Zealot.Common.CurrencyType)0, icon.gameObject);
                    continue;
                }

                if (icon.gameObject.name == "gold")
                {
                    currency_icons.Add((Zealot.Common.CurrencyType)1, icon.gameObject);
                    continue;
                }

                if (icon.gameObject.name == "skill")
                {
                    currency_icons.Add((Zealot.Common.CurrencyType)2, icon.gameObject);
                    continue;
                }
            }
        }

        inited = true;
    }

    void UpdateIcon()
    {
        init(); 

        if (currency_icons == null)
            return;

        foreach (var icon in currency_icons)
        {
            if (icon.Key != type_)
                icon.Value.gameObject.SetActive(false);
            else
                icon.Value.gameObject.SetActive(true);
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
