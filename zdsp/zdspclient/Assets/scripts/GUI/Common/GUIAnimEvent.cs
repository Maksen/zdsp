using UnityEngine;
using UnityEngine.Events;

public class GUIAnimEvent : MonoBehaviour
{
    public UnityEvent OnFinishedClose;

    [Header("Anim Event")]
    public UnityEvent onAnimEvent1;
    public UnityEvent onAnimEvent2;
    public UnityEvent onAnimEvent3;

    public void OnFinished()
    {
        if (OnFinishedClose.GetPersistentEventCount() == 0)
            gameObject.SetActive(false);
        else
            OnFinishedClose.Invoke();
    }

    public void OnFinishedOffParent()
    {
        if (OnFinishedClose.GetPersistentEventCount() == 0)
            gameObject.transform.parent.gameObject.SetActive(false);
        else
            OnFinishedClose.Invoke();
    }

    public void OnAnimEvent1() { onAnimEvent1.Invoke(); }
    public void OnAnimEvent2() { onAnimEvent2.Invoke(); }
    public void OnAnimEvent3() { onAnimEvent3.Invoke(); }
}

