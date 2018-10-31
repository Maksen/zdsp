using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;
using Zealot.Client.Entities;
using Zealot.Repository;

public class UI_DNA : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject dnaEmptyDataPrefab;
    public Transform dnaEmptyDataParent;

    [Header("DNA Slots Toggle")]
    public ToggleGroup dnaSlotTG;
    public List<Toggle> dnaSlots;

    [Header("General UI")]
    public Animator centralPanelAnimator;
    public GameObject dnaCentralPanel;
    public GameObject dnaEmptyScrollView;
    public GameObject dnaSlottedPanel;

    // Private variables
    private string _dnaGroupOpen = "DNAGroup_Open";
    private string _dnaGroupClose = "DNAGroup_Close";
    private int _previousSelSlot;

    void OnEnable()
    {
        InitDNA();
    }

    public void InitDNA()
    {
        if(dnaCentralPanel.activeSelf == true)
        {
            dnaCentralPanel.SetActive(false);
        }

        ResetDNASlots();
    }

    public void OpenDNASlot(bool isOn, DNAType dnaType)
    {
        List<Toggle> activeToggles = dnaSlotTG.ActiveToggles().Distinct().ToList();

        // No more active toggles means self is turned off
        if (activeToggles.Count == 0)
        {
            CloseCentralPanel();
        }
        else if (activeToggles.Count == 1)
        {
            if (isOn)
            {
                if (dnaCentralPanel.activeSelf == false)
                {
                    // To do: Check Inventory for isEmpty.
                    OpenCentralPanel(true);
                }
                else
                {
                    // Load DNAEmptyData
                }
            }
        }

        //if (isOn)
        //{
        //    //if (dnaCentralPanel.activeSelf == true)
        //    //{
        //    //    CloseCentralPanel();
        //    //}

        //    if (dnaCentralPanel.activeSelf == false)
        //    {
        //        // To do: Check Inventory for isEmpty.
        //        OpenCentralPanel(true);
        //    }
        //    else
        //    {
        //        // Load DNAEmptyData
        //    }

        //    if (_previousSelSlot != (int)dnaType)
        //    {
        //        _previousSelSlot = (int)dnaType;
        //    }
        //}
        //else
        //{
        //    // Close central panel only when it's same slot is clicked
        //    if (_previousSelSlot != -1 && _previousSelSlot == (int)dnaType)
        //    {
        //        CloseCentralPanel();
        //    }
        //}


    }

    public void CloseDNASlot()
    {
        // To do: Check Inventory for isEmpty.
        CloseCentralPanel();
    }

    private void ResetDNASlots()
    {
        _previousSelSlot = -1;

        for (int i = 0; i < dnaSlots.Count; ++i)
        {
            Toggle slot = dnaSlots[i];
            if (slot != null)
            {
                DNAType dnaType = (DNAType)(i + 1);
                slot.isOn = false;
                slot.onValueChanged.RemoveAllListeners();
                slot.onValueChanged.AddListener((bool isOn) => { OpenDNASlot(isOn, dnaType); });

                DNAData dnaData = slot.gameObject.GetComponent<DNAData>();
                if (dnaData != null)
                {
                    // Init to empty toggle button for now
                    // To do: Change to load data from inventory if DNA already slotted.
                    dnaData.Init(dnaType);
                }
            }
        }
    }

    private void OpenCentralPanel(bool isEmpty)
    {
        centralPanelAnimator.Play(_dnaGroupOpen);

        if(isEmpty)
        {
            // Open empty panel
            dnaEmptyScrollView.SetActive(true);
            dnaSlottedPanel.SetActive(false);

            // Load DNA empty data from inventory, filter by DNA and DNA type
        }
        else
        {
            dnaSlottedPanel.SetActive(true);
            dnaEmptyScrollView.SetActive(false);
        }
    }

    private void CloseCentralPanel()
    {
        dnaEmptyScrollView.SetActive(false);
        dnaSlottedPanel.SetActive(false);

        centralPanelAnimator.Play(_dnaGroupClose);
    }
}
