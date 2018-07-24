using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AlertData
{
    public AlertComponentVersion2 component;
    public List<AlertType> childrens;

    public AlertData()
    {
        childrens = new List<AlertType>();
    }
}

public class AlertManagerVersion2
{
    Dictionary<AlertType, AlertData> map_alert = new Dictionary<AlertType, AlertData>();
    BitArray ba_AlertOn = new BitArray((int)AlertType.NUM);

    public AlertManagerVersion2()
    {
        for (int i = 0; i < (int)AlertType.NUM; ++i)
        {
            map_alert.Add((AlertType)i, new AlertData());
            ba_AlertOn.Set(i, false);
        }   
    }

    AlertType HardcodeParentType(AlertType alertType)
    {
        switch (alertType)
        {
            default:
                return AlertType.None;
        }
    }

    void OnAlert(AlertType alertType)
    {
        ba_AlertOn.Set((int)alertType, true);
        AlertData mydata = map_alert[alertType];
        AlertType parentType = AlertType.None;
        if (mydata.component == null)
            parentType = HardcodeParentType(alertType);
        else
        {
            mydata.component.On();
            parentType = mydata.component.ParentType;
        }
        while (parentType != AlertType.None)
        {
            ba_AlertOn.Set((int)parentType, true);
            AlertComponentVersion2 component = map_alert[parentType].component;
            if (component == null)
                parentType = HardcodeParentType(parentType);
            else
            {
                component.On();
                parentType = component.ParentType;
            }
        }
    }

    void OffAlert(AlertType alertType)
    {
        ba_AlertOn.Set((int)alertType, false);
        AlertData mydata = map_alert[alertType];
        AlertType parentType = AlertType.None;
        if (mydata.component == null)
            parentType = HardcodeParentType(alertType);
        else
        {
            mydata.component.Off();
            parentType = mydata.component.ParentType;
        }
        while (parentType != AlertType.None)
        {
            AlertData parentdata = map_alert[parentType];
            bool allchildrenOff = true;
            for (int index = 0; index < parentdata.childrens.Count; ++index)
            {
                AlertType childType = parentdata.childrens[index];
                if (IsAlertOn(childType))
                    allchildrenOff = false;
            }
            if (allchildrenOff)
            {
                ba_AlertOn.Set((int)parentType, false);
                AlertComponentVersion2 component = parentdata.component;
                if (component == null)
                    parentType = HardcodeParentType(parentType);
                else
                {
                    component.Off();
                    parentType = component.ParentType;
                }
            }
            else
                return;
        }
    }

    public void SetAlert(AlertType alertType, bool isOn)
    {
        if (isOn)
        {
            if (!IsAlertOn(alertType))
                OnAlert(alertType);
        }           
        else
        {
            if (IsAlertOn(alertType))
                OffAlert(alertType);
        }            
    }

    public void RegisterAlert(AlertComponentVersion2 go)
    {
        AlertType myType = go.MyType;
        if (!map_alert.ContainsKey(myType))
        {
            Debug.LogErrorFormat("AlertType not assigned!!!Names={0}", go.transform.parent.name);
            return;
        }
        AlertData mydata = map_alert[myType];
        if (mydata.component != null)
        {           
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(go.transform.parent.name + ";");
            strBuilder.Append(mydata.component.transform.parent.name);
            Debug.LogErrorFormat("AlertType {0} has already assigned!!!Names={1}", myType.ToString(), strBuilder.ToString());
        }
        mydata.component = go;

        AlertType parentType = go.ParentType;
        if (parentType != AlertType.None)
            map_alert[parentType].childrens.Add(myType);
    }

    public bool IsAlertOn(AlertType type)
    {
        return ba_AlertOn.Get((int)type);
    }
}

