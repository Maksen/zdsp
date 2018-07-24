using UnityEngine;
using System.Collections;

public class DragSpin3DAvatar : MonoBehaviour {

    [SerializeField]
    Transform targetTransform = null;

    [SerializeField]
    UI_DragEvent uiDragEvent = null;

    private float _fDegree;
    [SerializeField]
    private int _fRotateSpeed;
    private Quaternion _qOriRotation;
    private Quaternion _qTargetRotation;

    Quaternion starting_rotation;
    void Awake()
    {
        starting_rotation = new Quaternion { x = targetTransform.localRotation.x, y = targetTransform.localRotation.y, z = targetTransform.localRotation.z, w = targetTransform.localRotation.w };
        if (uiDragEvent != null)
            uiDragEvent.onDragging = DragSpin;
    }

    void OnEnable()
    {
        Reset();
    }

    public void Reset()
    {
        targetTransform.localRotation = starting_rotation;
    }

    public void DragSpin(Vector2 delta)
    {
        if (targetTransform == null)
            return;

        int direction = 0;

        if (delta.x > 0)
            direction = 1;
        if (delta.x < 0)
            direction = -1;

        _fDegree = targetTransform.localRotation.eulerAngles.y;
        _fDegree -= direction * _fRotateSpeed;
        _qOriRotation = targetTransform.localRotation;
        _qTargetRotation = Quaternion.Euler(0, _fDegree, 0);
        targetTransform.localRotation = Quaternion.Lerp(_qOriRotation, _qTargetRotation, Time.deltaTime * _fRotateSpeed);
    }
}
