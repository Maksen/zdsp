using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;

public class SystemMessageController : MonoBehaviour
{
    [SerializeField] GameObject SystemMessagePrefab;
    [SerializeField] float sysMsgInterval = 0.5f;

    private List<GameObject> mListSystemMsg;
    private const int mMaxMsg = 10;
    private Queue<string> buffer = new Queue<string>();
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
        {
            List<GameObject> activeMsgObjects = ObjMgr.Instance.GetActiveContainerObjects(mListSystemMsg);
            for (int i = 0; i < activeMsgObjects.Count; ++i)
                activeMsgObjects[i].GetComponent<UI_SystemMessage>().AddTranslateQueue();

            inst.GetComponent<UI_SystemMessage>().ShowMessage(buffer.Dequeue());
        }

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