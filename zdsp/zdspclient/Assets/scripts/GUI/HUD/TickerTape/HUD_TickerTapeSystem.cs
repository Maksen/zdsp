using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SoftMasking;

public class HUD_TickerTapeSystem : MonoBehaviour
{
    public Text annoucementText;
    public RectTransform annoucementRectTransform;
    public RectTransform backgroundRectTransform;
    public float moveSpeed;
    private Coroutine queryCoroutine;

    void Start()
    {
        Wait();
    }

    void OnDestroy()
    {
        if (gameObject != null)
        {
            SoftMask softmask = gameObject.transform.GetChild(0).GetComponent<SoftMask>();
            //if (softmask != null)
            //    softmask.Clear();
        }
    }

    private void DestroyCoroutine()
    {
        if (queryCoroutine != null)
        {
            StopCoroutine(queryCoroutine);
            queryCoroutine = null;
        }
    }

    private void Wait()
    {
        DestroyCoroutine();
        queryCoroutine = StartCoroutine(QueryGMMessage());
    }

    private IEnumerator QueryGMMessage()
    {
        string message = null;
        yield return new WaitUntil
        (
            () =>
            {
                TickerTapeSystem.Instance.Reset();
                bool result = TickerTapeSystem.Instance.QueryMessage(out message);
                backgroundRectTransform.gameObject.SetActive(result);
                return result;
            }
        );

        annoucementText.text = message;
    }
    
	void Update()
    {
        if (backgroundRectTransform.gameObject.activeInHierarchy == false)
            return;

        if (annoucementRectTransform.anchoredPosition.x < -annoucementRectTransform.rect.width)
        {
            annoucementRectTransform.anchoredPosition = new Vector2(backgroundRectTransform.rect.width, 0);
            Wait();
        }
        else
            annoucementRectTransform.transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
    }
}
