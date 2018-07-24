using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PartyMessage : MonoBehaviour
{
    [SerializeField] Text messageText;
    [SerializeField] UI_ProgressBarC progressBar;

    private Animator animator;
    private Action onClickOkCallback;
    private Action onTimeOutCallback;
    private float countDown;
    private Coroutine countdownTimer;
    private Queue<PartyMessageContent> buffer = new Queue<PartyMessageContent>();

    private struct PartyMessageContent
    {
        public string message;
        public Action okCallback;
        public Action timeoutCallback;
        public float duration;

        public PartyMessageContent(string msg, Action ok, Action timeout, float dur)
        {
            message = msg;
            okCallback = ok;
            timeoutCallback = timeout;
            duration = dur;
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ShowPartyMessage(string content, Action okCallback, Action timeOutCallback = null, float duration = 5f)
    {
        PartyMessageContent newContent = new PartyMessageContent(content, okCallback, timeOutCallback, duration);
        buffer.Enqueue(newContent);
        if (!gameObject.activeInHierarchy)
        {
            SetMessage(newContent);
            gameObject.SetActive(true);  // will call StartCountdown
        }
    }

    public void OnClickOk()
    {
        if (onClickOkCallback != null)
            onClickOkCallback();
    }

    private void SetMessage(PartyMessageContent content)
    {
        messageText.text = content.message;
        onClickOkCallback = content.okCallback;
        onTimeOutCallback = content.timeoutCallback;
        progressBar.Value = progressBar.Max;
    }

    // called by GUIAnimEvent1 when become active
    public void StartCountdown()
    {
        if (buffer.Count > 0)
        {
            PartyMessageContent content = buffer.Dequeue();
            countDown = content.duration;
            countdownTimer = StartCoroutine(Countdown(countDown));
        }
    }

    private IEnumerator Countdown(float duration)
    {
        while (countDown > 0)
        {
            progressBar.Value = (long)Mathf.Lerp(0f, progressBar.Max, countDown / duration);
            countDown -= Time.deltaTime;
            yield return null;
        }

        if (onTimeOutCallback != null)
            onTimeOutCallback();
        progressBar.Value = 0;

        if (buffer.Count > 0)  // show next message if any is queued
        {
            SetMessage(buffer.Peek());
            StartCountdown();
        }
        else
            animator.Play("PartyInviteFollow_Hide");
    }

    private void OnDisable()
    {
        onClickOkCallback = null;
        onTimeOutCallback = null;
        if (countdownTimer != null)
        {
            StopCoroutine(countdownTimer);
            countdownTimer = null;
        }
        countDown = 0;
        buffer.Clear();
    }
}