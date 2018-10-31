using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class DNAData : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject dnaIconPrefab;
    public Transform dnaIconParent;

    [Header("Text")]
    public Text dnaNameText;
    public Text dnaMutateText;
    public Text dnaStatsLeftText;
    public Text dnaStatsRightText;

    // Private variables
    private DNAType _dnaType;

    public void Init(DNAType dnaType)
    {
        _dnaType = dnaType;

        dnaNameText.text        = "";
        dnaMutateText.text      = "";
        dnaStatsLeftText.text   = "";
        dnaStatsRightText.text  = "";
    }

    public DNAType GetDNAType()
    {
        return _dnaType;
    }
}
