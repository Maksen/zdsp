using UnityEngine;
using System.Collections;
using Zealot.Repository;
using UnityEngine.UI;

public class CharacterSkillSelector : MonoBehaviour {

    public CharacterCreationManager manager;
    public GameObject YesNoOkDialogue, DialogueText;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ButtonOne()
    {
        var job = manager.GetSelectedJob();

        var ccjson = JobSectRepo.GetJobByType(job);
        if (ccjson != null)
        {
            // /var skill = SkillRepo.GetSkill(ccjson.skill1);

            // if (DialogueText) DialogueText.GetComponent<Text>().text = skill.skillJson.description;
            YesNoOkDialogue.SetActive(true);
        }
        else
        {
            Debug.Log("kopio entry in JobSectRepo for key " + job.ToString() + " missing or repo not loaded");
        }
    }

    public void ButtonTwo()
    {
        var job = manager.GetSelectedJob();

        var ccjson = JobSectRepo.GetJobByType(job);
        if (ccjson != null)
        { 
            //var skill = SkillRepo.GetSkill(ccjson.skill2);

            //if (DialogueText) DialogueText.GetComponent<Text>().text = skill.skillJson.description;

            YesNoOkDialogue.SetActive(true);
        }
        else
        {
            Debug.Log("kopio entry in JobSectRepo for key " + job.ToString() + " missing or repo not loaded");
        }
    }

    public void ButtonThree()
    {
        var job = manager.GetSelectedJob();

        var ccjson = JobSectRepo.GetJobByType(job);
        if (ccjson != null)
        { 
            //var skill = SkillRepo.GetSkill(ccjson.skill3); 
            //if(skill != null) {
            // if (DialogueText) DialogueText.GetComponent<Text>().text = skill.skillJson.description;
            //}

            YesNoOkDialogue.SetActive(true);
        }
        else
        {
            Debug.Log("kopio entry in JobSectRepo for key " + job.ToString() + " missing or repo not loaded");
        }
    }

    public void ConfirmSkill()
    {
        YesNoOkDialogue.SetActive(false);
        manager.ConfirmSkill();
    }
}
