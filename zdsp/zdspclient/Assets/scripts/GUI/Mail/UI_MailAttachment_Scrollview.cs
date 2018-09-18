using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zealot.Common;

public class UI_MailAttachment_Scrollview : MonoBehaviour {

    private List<GameObject> mMailAttachmentList = new List<GameObject>();   //contains all the attachments
    private List<GameObject> mMailCurrencyList = new List<GameObject>();     //contains all the currencies

    public GameObject mMailAttachmentPrefab = null; //variable for assigning attachment prefab
    public GameObject mMailCurrencyAttachmentPrefab = null;

    public GameObject mAttachmentParent = null;
    public GameObject mCurrencyParent = null;

    void OnEnable()
    {
        if (mMailAttachmentPrefab == null)
        {
            Debug.LogError("UI_MailAttachment_ScrollView :: UI_MailAttachmentPrefab is not assigned!", mMailAttachmentPrefab);
        }
        if (mMailCurrencyAttachmentPrefab == null)
        {
            Debug.LogError("UI_MailCurrencyAttachment_ScrollView :: UI_MailCurrencyAttachmentPrefab is not assigned!", mMailCurrencyAttachmentPrefab);
        }
    }

	// Use this for initialization
	void Start () {
        
	}

    void OnDestroy()
    {
        ClearAllAttachmentSlots();
        ClearAllCurrencySlots();

        mMailAttachmentPrefab = null;
        mMailCurrencyAttachmentPrefab = null;
    }

    public void PopulateAttachments(List<IInventoryItem> attList)
    {
        //Read attachments from a list and create attachments ui accordingly
        IInventoryItem item = null;
        for (int i = 0; i < attList.Count; ++i)
        {
            item = attList[i];

            //Create attachment icons and set them as this's child
            GameObject attachmentClone = Instantiate(mMailAttachmentPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
            attachmentClone.transform.SetParent(mAttachmentParent.transform, false);
            attachmentClone.SetActive(true);

            //Assign item to attachment icons
            UI_MailAttachmentData attItem = attachmentClone.GetComponent<UI_MailAttachmentData>();
            attItem.SetItem(item);

            //store the attachment icon
            mMailAttachmentList.Add(attachmentClone);
        }
    }

    public void PopulateCurrencies(Dictionary<Zealot.Common.CurrencyType, int> dicCurr)
    {
        if (dicCurr == null || dicCurr.Count == 0)
            return;

        foreach (var c in dicCurr)
        {
            if (c.Value <= 0)
                continue;

            //Create gameobject from prefab, that will be responsible for displaying currency text
            GameObject currencyClone = Instantiate(mMailCurrencyAttachmentPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
            currencyClone.transform.SetParent(mCurrencyParent.transform, false);
            currencyClone.SetActive(true);

            //Set currency icon and value
            UI_MailCurrencyData attCurr = currencyClone.GetComponent<UI_MailCurrencyData>();
            attCurr.SetValue(c.Key, c.Value);

            //Add into quick reference list
            mMailCurrencyList.Add(currencyClone);
        }
    }

    public void ClearAllAttachmentSlots()
    {
        //Do nothing if empty
        if (mMailAttachmentList.Count == 0)
        {
            return;
        }

        //Loop and destroy attachment gameobjects
        for (int i = 0; i < mMailAttachmentList.Count; ++i)
        {
            Destroy(mMailAttachmentList[i]);
        }
        mMailAttachmentList.Clear();
    }

    public void ClearAllCurrencySlots()
    {
        if (mMailCurrencyList.Count == 0)
        {
            return;
        }

        //Loop and destroy currency gameobjects
        for (int i = 0; i < mMailCurrencyList.Count; ++i)
        {
            Destroy(mMailCurrencyList[i]);
        }
        mMailCurrencyList.Clear();
    }
}
