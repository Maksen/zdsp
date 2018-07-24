using UnityEngine;
using System.Collections;

public class CombatDigitsAnimation : MonoBehaviour {
	
	public float DriftDuration;
	public float ScaleDuration;
	public float ScaleFactor;
	public Transform hostTransform;
	public Vector2 combatDigitsDriftDistance;
	public Vector2 initialPos;
	public Vector2 initialOffsetPos;

	float driftSpeed;
	float scaleSpeed;
	float startTime;
	Vector3 initialScale;
	Camera cam;
	RectTransform gameCanvas;
	RectTransform _rect;
	// Use this for initialization
	void Awake()
	{
		Destroy(gameObject,DriftDuration);
		gameCanvas = UIManager.GetHUDGameCanvas();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		_rect = GetComponent<RectTransform>();
		initialScale = transform.localScale;
		if(gameCanvas == null)
		{
			Debug.LogError("No canvas found. Check scene settings....");
		}
		else if(cam == null)
		{
			Debug.LogError("No UI camera found. Check scene settings....");
		}
	}

	void Start () {
		startTime = Time.time;
		driftSpeed = combatDigitsDriftDistance.magnitude / DriftDuration;
		scaleSpeed = ScaleFactor / ScaleDuration;
	}
	
	// Update is called once per frame
	void Update () {
        if (hostTransform == null)
            return;
		float disCovered = (Time.time - startTime) * driftSpeed;
		float scaleCovered = (Time.time - startTime) * scaleSpeed;
		float fracJourney = disCovered / combatDigitsDriftDistance.magnitude;
		float scaleJourney = scaleCovered / ScaleFactor;
		_rect.localScale = Vector3.Lerp(initialScale, initialScale*ScaleFactor , scaleJourney);
		_rect.anchoredPosition = getCanvasPosition(Vector2.Lerp(initialOffsetPos, initialOffsetPos+combatDigitsDriftDistance,fracJourney));
	}

	public Vector2 getCanvasPosition(Vector2 UIOffset)
	{
		Vector2 ViewportPosition = cam.WorldToViewportPoint(hostTransform.position);
		Vector2 WorldObject_ScreenPosition= new Vector2(
			((ViewportPosition.x*gameCanvas.sizeDelta.x)-(gameCanvas.sizeDelta.x*0.5f))+UIOffset.x,
			((ViewportPosition.y*gameCanvas.sizeDelta.y)-(gameCanvas.sizeDelta.y*0.5f))+UIOffset.y);
		return WorldObject_ScreenPosition;
	}
}
