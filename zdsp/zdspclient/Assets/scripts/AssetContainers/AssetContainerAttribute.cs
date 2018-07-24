using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class CreateScriptableObject : Attribute
{
    public readonly bool CanCreate;

    public CreateScriptableObject() { CanCreate = true; }
    public CreateScriptableObject(bool create)
    {
        CanCreate = create;
    }
}