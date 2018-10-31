using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_DialogLicenseAgreement : MonoBehaviour
{
    [SerializeField]
    Text txtContent = null;
    [SerializeField]
    Button buttonConfirm = null;

    TextAsset txtAssetEULA = null;

    // Use this for initialization
    void Start()
    {
        buttonConfirm.interactable = false;
        LoginData.Instance.HasReadLicense = false;

        txtAssetEULA = Resources.Load<TextAsset>("EULA");
        if (txtAssetEULA != null)
            txtContent.text = txtAssetEULA.text;
        else
            Debug.Log("Error TextAsset EULA not found!");

    }

    void OnDestroy()
    {
        txtContent = null;
        buttonConfirm = null;
        txtAssetEULA = null;
    }

    public void OnValueChangedToggleReadAndAgreed(bool value)
    {
        buttonConfirm.interactable = value;
    }

    public void OnClickConfirm()
    {
        LoginData.Instance.HasReadLicense = true;
        LoginData.Instance.SerializeLoginData();
        UIManager.CloseDialog(WindowType.DialogLicenseAgreement);

        if (!LoginData.Instance.IsDataValid)
            UIManager.OpenDialog(WindowType.DialogAccountLoginType);
    }
}
