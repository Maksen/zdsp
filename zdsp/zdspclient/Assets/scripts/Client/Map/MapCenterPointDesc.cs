using UnityEngine;
using System.Collections;
using Zealot.Spawners;
using Zealot.Entities;

public class MapCenterPointDesc : ServerEntity
{
    public override ServerEntityJson GetJson()
    {
        MapInfoJson jsonclass = new MapInfoJson();
        GetJson(jsonclass);
        return jsonclass;
    }

    public void GetJson(MapInfoJson jsonclass)
    {
        RectTransform rt = GetComponent<RectTransform>();
        //jsonclass.centerPoint = transform.TransformPoint(rt.anchoredPosition3D);
        jsonclass.centerPoint = gameObject.transform.position;

        rt = transform.GetChild(0).GetComponent<RectTransform>();
        jsonclass.mapScale = rt.localScale;
        jsonclass.width = rt.sizeDelta.x;
        jsonclass.height = rt.sizeDelta.y;
      
        base.GetJson(jsonclass);
    }
}
