using UnityEngine;
using System.Collections;
using Zealot.Client.Entities;
using System.Collections.Generic;
using Zealot.Common;

public class ClientTrigger : MonoBehaviour {

    // Use this for initialization
    [SerializeField]
    public List<GameObject> listOfObjects;
    public ClientTriggerTypes triggerType = ClientTriggerTypes.TutorialStepTrigger;
    private SphereCollider _collider; 
    public float triggerRadius = 3.0f;
	void Start () {
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<SphereCollider>();
            _collider.radius = triggerRadius;
            _collider.isTrigger = true;
        }
        
    }
	
    /// <summary>
    /// make sure the gameobject layer setting is able to collide with the LocalPlayer layer(Entities)
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LocalPlayer"))
        {
            if (ClientTriggerTypes.TutorialStepTrigger == triggerType)
            {
                TrainingRealmContoller.Instance.OnQuestStepDone((int)Trainingstep.EncounterBoss);
                StartCoroutine(WaitAndDeactive());
                if (listOfObjects !=null && listOfObjects.Count > 0)
                {
                    for (int i =0; i < listOfObjects.Count; i++)
                    {
                        Destroy(listOfObjects[i]);
                    }
                    listOfObjects.Clear();
                }
            }
            
        }
    }

    IEnumerator WaitAndDeactive()
    {
        yield return new WaitForSeconds(0.1f);
        //this.enabled = false;//trigger event is sent even it is disabled. 
        if (_collider != null)
        {
            Destroy(_collider);
        }
    }

    void OnTriggerExit(Collider other)
    {        
        if (other.CompareTag("LocalPlayer"))
        {
            Debug.Log("Player Exit");
            if (ClientTriggerTypes.TutorialStepTrigger == triggerType)
            {

            }
        }
    }
}
