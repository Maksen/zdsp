using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zealot.Common;

public class SystemMessageController : MonoBehaviour
{
    public GameObject SystemMessagePrefab;

    private List<GameObject> mListSystemMsg;
    private const int mMaxMsg = 10;
    private Queue<string> buffer = new Queue<string>();
    private const float sysMsgInterval = 0.25f;
    private Coroutine countdownTimer = null;

    void Awake()
    {
        Transform parent = GetComponent<Transform>();
        mListSystemMsg = ObjMgr.Instance.InitGameObjectPool(parent, SystemMessagePrefab, SystemMessagePrefab.transform.localPosition, SystemMessagePrefab.transform.localScale, mMaxMsg);
    }

    public void AddSystemMessage(string message, bool addToChatLog = false)
    {
        buffer.Enqueue(message);

        if (countdownTimer == null)
            countdownTimer = StartCoroutine(ShowMessage());

        if (addToChatLog)
            UIManager.AddToChat((byte)MessageType.System, message);
    }

    private IEnumerator ShowMessage()
    {
        GameObject inst = ObjMgr.Instance.GetContainerObject(mListSystemMsg);
        if (inst != null)
            inst.GetComponent<UI_SystemMessage>().ShowMessage(buffer.Dequeue());

        yield return new WaitForSeconds(sysMsgInterval);
  
        if (buffer.Count > 0)
            countdownTimer = StartCoroutine(ShowMessage());
        else
            countdownTimer = null;
    }

    void OnDestroy()
    {
        buffer.Clear();

        if (mListSystemMsg != null)
        {
            ObjMgr.Instance.DestroyContainerObject(mListSystemMsg);
            mListSystemMsg.Clear();
            mListSystemMsg = null;
        }     
    }
}
