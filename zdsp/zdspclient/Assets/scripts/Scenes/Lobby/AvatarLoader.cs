using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class AvatarLoader : MonoBehaviour {

	public bool avatarRotate;
	public float rotateSpeed = 20.0f;
	private float xDeg;
	private float yDeg;
	private Quaternion fromRotation;
	private Quaternion toRotation;

    [SerializeField]
    UI_DragEvent drag = null;
    
    GameObject avatar;
    // Use this for initialization
    void Start()
    {
        xDeg = 0;
        if (drag != null)
            drag.onDragging += Drag;
    }

    void Drag(Vector2 delta)
    {
        if(avatar == null)//find once and store
            avatar = GameObject.Find("AvatarSelected");

        int direction = 0;

        if (delta.x > 0)
            direction = 1;
        if (delta.x < 0)
            direction = -1;

        xDeg = avatar.transform.rotation.eulerAngles.y;

        xDeg -= direction * rotateSpeed;
        //yDeg += Input.GetAxis("Mouse Y") * rotateSpeed;
        fromRotation = avatar.transform.rotation;
        //toRotation = Quaternion.Euler(yDeg,xDeg,0);
        toRotation = Quaternion.Euler(0, xDeg, 0);
        avatar.transform.rotation = Quaternion.Lerp(fromRotation, toRotation, Time.deltaTime * rotateSpeed);
    }
    
}
