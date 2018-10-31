using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[ExecuteInEditMode]
public class MaterialPropertyController : MonoBehaviour
{    
    public string PropertyName0;
    public float PropertyValue0;

    public string PropertyName1;
    public float PropertyValue1;

    public string PropertyName2;
    public float PropertyValue2;

    public string PropertyName3;
    public float PropertyValue3;

    public string PropertyName4;
    public float PropertyValue4;

    public Dictionary<string, string> propmap;
    public Dictionary<string, FieldInfo> names;

    // Use this for initialization
    void Start ()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        propmap = new Dictionary<string, string>();
        names = new Dictionary<string, FieldInfo>();

        string nametoken = "PropertyName";        
        string valuetoken = "PropertyValue";

        GetProperties(nametoken, valuetoken);
    }

    void GetProperties(string nametoken, string valuetoken)
    {
        var props = typeof(MaterialPropertyController).GetFields();
        
        foreach (var prop in props)
        {
            if (prop.Name.Contains(nametoken))
            {
                var name = (string)prop.GetValue(this);

                propmap.Add(prop.Name, name);

                if (name != null && name.Length > 0)
                    if (names.ContainsKey(name) == false)
                        names.Add(name, null);
            }
        }
        
        foreach (var prop in props)
        {
            if (prop.Name.Contains(valuetoken))
            {
                var count = prop.Name.Substring(valuetoken.Length);
                var key = nametoken + count;

                if (propmap.ContainsKey(key))
                {
                    var propname = propmap[key];

                    if (names.ContainsKey(propname))
                    {
                        names[propname] = prop;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update ()
    {
        var renderer = GetComponent<Renderer>();

        if (renderer != null) 
        {            
            MaterialPropertyBlock block = new MaterialPropertyBlock();
                        
            renderer.GetPropertyBlock(block);

            foreach (var name in names)
            {
                var val = (float)name.Value.GetValue(this);
                block.SetFloat(name.Key, val);
            }

            renderer.SetPropertyBlock(block);          
        }
    }
}
