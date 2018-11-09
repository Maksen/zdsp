using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;

public class EquipmentReformStats : MonoBehaviour
{
    [Header("Text")]
    public Text statsLabel;
    public Text statsValue;

    public void Init(int reformStep, string seName)
    {
        statsLabel.text = ClientUtils.GetLocalizedReformKai(reformStep);
        statsValue.text = seName;
    }

    public void Init(string seName, int seValue)
    {
        statsLabel.text = seName;
        statsValue.text = seValue.ToString();
    }
}
