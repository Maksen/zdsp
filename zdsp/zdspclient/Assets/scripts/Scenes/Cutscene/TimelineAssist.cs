using System;
using UnityEngine;
using UnityEngine.Playables;
using Zealot.Client.Entities;
using Zealot.Common;

public class TimelineAssist : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public Transform replaceLocalPlayer;
    public string replaceTrackName;
    public GameObject malePrefab;
    public GameObject femalePrefab;
    public TimelineWeaponAssist weaponAssist;
    private Action mFinishCB = null;
    private GameObject myLocalPlayer = null;

    private void ReplaceActor()
    {
        if (replaceLocalPlayer != null)
        {
            PlayerGhost localplayer = GameInfo.gLocalPlayer;
            if (localplayer != null)
            {
                GameObject prefab = localplayer.mGender == Gender.Male ? malePrefab : femalePrefab;
                if (prefab != null)
                    myLocalPlayer = GameObject.Instantiate(prefab);
                else
                    myLocalPlayer = ClientUtils.InstantiatePlayer(localplayer.mGender);
                myLocalPlayer.GetComponent<AvatarController>().InitAvatar(localplayer.mEquipmentInvData, (JobType)localplayer.PlayerSynStats.jobsect, localplayer.mGender);
                if (weaponAssist != null && !weaponAssist.gameObject.activeSelf)
                    myLocalPlayer.GetComponent<AvatarController>().HideWeapon(true);

                Transform myTransform = myLocalPlayer.transform;
                myTransform.SetParent(replaceLocalPlayer.parent, false);
                myTransform.localPosition = replaceLocalPlayer.localPosition;
                myTransform.localRotation = replaceLocalPlayer.localRotation;
                myTransform.localScale = replaceLocalPlayer.localScale;

                foreach (var kvp in playableDirector.playableAsset.outputs)
                {
                    if (kvp.streamName == replaceTrackName)
                    {
                        playableDirector.SetGenericBinding(kvp.sourceObject, myLocalPlayer);
                        break;
                    }
                }
                replaceLocalPlayer.gameObject.SetActive(false); //hide it so it won't in scene.
            }
        }
    }

    public void HideWeapon(bool hide)
    {
        if (myLocalPlayer != null)
            myLocalPlayer.GetComponent<AvatarController>().HideWeapon(hide);
    }

    public void Play(Action onFinishCB)
    {
        mFinishCB = onFinishCB;
        ReplaceActor();
        gameObject.SetActive(true);
        playableDirector.Play();
    }

    public void Skip()
    {
        playableDirector.time = playableDirector.duration;
        //OnFinished();
    }

    public bool IsPlaying()
    {
        return gameObject.activeInHierarchy && playableDirector.state == PlayState.Playing;
    }

    void OnFinished()
    {
        playableDirector.Stop();
        gameObject.SetActive(false);
        if (myLocalPlayer != null)
        {
            Destroy(myLocalPlayer);
            myLocalPlayer = null;
        }
        if (mFinishCB != null)
            mFinishCB();
    }

    void Update()
    {
        if (playableDirector.time >= playableDirector.duration)
            OnFinished();
    }
}

