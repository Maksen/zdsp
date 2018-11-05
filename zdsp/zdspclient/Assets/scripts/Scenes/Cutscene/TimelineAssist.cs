using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Zealot.Client.Entities;
using Zealot.Common;

public class TimelineAssist : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public Transform replaceLocalPlayer;
    public GameObject malePrefab = null;
    public GameObject femalePrefab = null;
    public TimelineWeaponAssist weaponAssist;
    private Action mFinishCB = null;
    private GameObject myLocalPlayer = null;
    private bool IsCutsceneSkipped = false;

    private void ReplaceActor()
    {
        if (replaceLocalPlayer != null)
        {
            PlayerGhost localplayer = GameInfo.gLocalPlayer;
            if (localplayer != null)
            {
                GameObject prefab = localplayer.mGender == Gender.Male ? malePrefab : femalePrefab;
                if (prefab != null)
                    myLocalPlayer = Instantiate(prefab);
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

                var clipArrays = myLocalPlayer.GetComponent<Animator>().runtimeAnimatorController.animationClips;
                foreach (var kvp in playableDirector.playableAsset.outputs) //rebind all localplayer track to new object
                {
                    if (kvp.streamName.Contains("LocalPlayer")) 
                    {
                        playableDirector.SetGenericBinding(kvp.sourceObject, myLocalPlayer);
                        var track = kvp.sourceObject as AnimationTrack;
                        if (track != null)
                        {
                            var clips = track.GetClips();
                            foreach (var _timeClip in clips)
                            {
                                string adjusted_clipname = GetClipAdjusted(_timeClip.displayName, localplayer.WeaponTypeUsed);
                                var adjusted_clip = Array.Find(clipArrays, clip => clip.name == adjusted_clipname);
                                if (adjusted_clip != null)
                                {
                                    ((AnimationPlayableAsset)_timeClip.asset).clip = adjusted_clip;
                                    //_timeClip.displayName = adjusted_clipname;
                                }
                            }
                        }   
                    }
                }
                replaceLocalPlayer.gameObject.SetActive(false); //hide it so it won't in scene.
            }
        }
    }

    private static string GetClipAdjusted(string clip, PartsType type)
    {
        int index = clip.IndexOf('_');
        if (index >= 0 && ClientUtils.weaponPrefix.Contains(clip.Substring(0, index)))
            return ClientUtils.GetPrefixByWeaponType(type) + clip.Substring(index);
        return clip;
    }

    public void HideWeapon(bool hide)
    {
        if (myLocalPlayer != null)
            myLocalPlayer.GetComponent<AvatarController>().HideWeapon(hide);
    }

    public void Play(Action onFinishCB)
    {
        IsCutsceneSkipped = false;
        mFinishCB = onFinishCB;
        ReplaceActor();
        gameObject.SetActive(true);
        playableDirector.Play();
    }

    public void Skip()
    {
        IsCutsceneSkipped = true;
        OnFinished();
    }

    public bool IsPlaying()
    {
        return gameObject.activeInHierarchy && playableDirector.state == PlayState.Playing;
    }

    private void OnFinished()
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

    IEnumerator FinishCutsceneNextFrame()
    {
        yield return null;
        OnFinished();
    }

    void Update()
    {
        if (!IsCutsceneSkipped)
        {
            double currentTime = playableDirector.time;
            if (currentTime > 0 && currentTime + Time.deltaTime >= playableDirector.duration)
                StartCoroutine(FinishCutsceneNextFrame());
        }
    }
}
