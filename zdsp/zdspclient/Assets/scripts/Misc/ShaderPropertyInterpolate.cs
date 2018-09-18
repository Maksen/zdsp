using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPropertyInterpolate : MonoBehaviour {

    float value = 0.0f;
    float interpolationtime = 0.0f;
    float current = 0.0f;
    string propertyname = "";
    bool reverse = false;
    string removedname;
    public void Activate(string propertyname, float interpolationtime = 1.0f, float value = 0.0f, bool reverse = false, string removedname = "")
    {
        //start = Time.realtimeSinceStartup;
        current = 0.0f; 
        this.value = value;
        this.reverse = reverse;
        this.removedname = removedname;

        this.interpolationtime = interpolationtime;
        this.propertyname = propertyname;

        var _propBlock = new MaterialPropertyBlock();
        var _renderer = GetComponentInChildren<Renderer>();
        _renderer.GetPropertyBlock(_propBlock);

        if(this.value == 0.0f)
            this.value = _propBlock.GetFloat(propertyname);

        _propBlock.SetFloat(propertyname, 0.0f);

        _renderer.SetPropertyBlock(_propBlock);
    }
    
    private void Update()
    { 
        if (interpolationtime != 0.0f)
        {
            current += Time.deltaTime;
            var theta = current / interpolationtime;                        
            if (theta >= 1.0f)
                theta = 1.0f;

            var delta = theta;

            if (reverse)
                delta = 1.0f - delta;

            var renderers = gameObject.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                var propBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propBlock);
                propBlock.SetFloat(propertyname, value * delta);
                renderer.SetPropertyBlock(propBlock);

                if (theta == 1.0f)
                {
                    if (removedname != "")
                    {
                        List<Material> newmatlist = new List<Material>();
                        foreach (var material in renderer.materials)
                        {
                            if (material.name.Contains(removedname))
                                continue;

                            newmatlist.Add(material);
                        }

                        renderer.materials = newmatlist.ToArray();
                    }
                }
            }

            if (theta == 1.0f)
            {
                Destroy(this);
            }
        }
    }

    public void AddMaterial(Material loadedmat)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            List<Material> newmatlist = new List<Material>();
            foreach (var material in renderer.materials)
            {
                if (material.name == loadedmat.name)
                    return;

                newmatlist.Add(material);
            }

            newmatlist.Add(loadedmat);

            renderer.materials = newmatlist.ToArray();
        }
    }
}
