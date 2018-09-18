using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Party_RequestData : UI_Party_RequestBase
{
    [SerializeField] Text jobText;

    private UnityAction<string, bool> processRequestAction;

    public void Init(PartyRequest request, UnityAction<string, bool> action)
    {
        InitBase(request.name, request.level);
        portraitImage.sprite = ClientUtils.LoadIcon(JobSectRepo.GetJobPortraitPath(request.jobType));
        jobText.text = JobSectRepo.GetJobLocalizedName(request.jobType);
        processRequestAction = action;
    }

    public void OnClickProcessRequest(bool isAccept)
    {
        if (processRequestAction != null)
            processRequestAction(nameText.text, isAccept);
    }
}