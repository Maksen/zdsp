using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class Dialog_LicenseAgreement : MonoBehaviour
{
    // Editor Linked Gameobjects
    [SerializeField]
    Text txtLicenseContent = null;
    [SerializeField]
    Toggle toggleReadAll = null;
    [SerializeField]
    Button buttonAgree = null;

    TextAsset txtAssetEULA = null;

    // Use this for initialization
    void Start()
    {
        toggleReadAll.isOn = false;
        buttonAgree.interactable = false;
        LoginData.Instance.HasReadLicense = false;

        txtAssetEULA = Resources.Load<TextAsset>("EULA");
        if (txtAssetEULA != null)
            txtLicenseContent.text = txtAssetEULA.text;
        else
            Debug.Log("Error TextAsset EULA not found!");

    }

    void OnDestroy()
    {
        txtAssetEULA = null;
    }

    public void OnValueChangedReadAll(bool value)
    {
        buttonAgree.interactable = value;
    }

    public void OnClickAgree()
    {
        LoginData.Instance.HasReadLicense = true;
        LoginData.Instance.SerializeLoginData();
        UIManager.CloseDialog(WindowType.DialogLicenseAgreement);
    }
}
