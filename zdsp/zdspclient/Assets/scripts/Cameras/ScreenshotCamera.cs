using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScreenshotCamera : MonoBehaviour
{
    Camera cam;
    public Slider zoom_slider;
    public GameObject ui_to_hide = null;
    public float maximum_x_angle = 22.5f;

    // Use this for initialization
    void Start()
    {
        if (cam == null) cam = gameObject.GetComponent<Camera>();
        gameObject.SetActive(false);
    }

    float time_passed = 0.0f;
    float transit_time = 0.0f;

    Vector3 eulerangles = new Vector3();
    Vector3 initial_offset = new Vector3();

    float offset_distance = 10.0f;
    float offset_height = 10.0f;
    Vector3 local_offset = new Vector3();

    bool entering = false, exiting = false;
    Vector3 initial_position = new Vector3();
    Quaternion initial_rotation = new Quaternion();
    Quaternion target_rotation = new Quaternion();
    Transform Subject;

    public void init(Transform initial_cam_transform, float offsetdistance, Transform subject, float transition_time)
    {
        entering = true;
        first_frame = true;

        transit_time = transition_time;
        time_passed = 0.0f;
        gameObject.SetActive(true);

        initial_position.x = initial_cam_transform.localPosition.x;
        initial_position.y = initial_cam_transform.localPosition.y;
        initial_position.z = initial_cam_transform.localPosition.z;

        initial_rotation.x = initial_cam_transform.localRotation.x;
        initial_rotation.y = initial_cam_transform.localRotation.y;
        initial_rotation.z = initial_cam_transform.localRotation.z;
        initial_rotation.w = initial_cam_transform.localRotation.w;

        Subject = subject;

        zoom_slider.value = 0.5f;
        eulerangles = new Vector3();
        offset_distance = offsetdistance;
        initial_offset = new Vector3(0, offset_distance * Mathf.Sin(Mathf.PI / 8), offset_distance * Mathf.Cos(Mathf.PI / 8));
        var offset = subject.localToWorldMatrix * initial_offset;

        local_offset = offset;

        gameObject.transform.localPosition = initial_position;
        gameObject.transform.localRotation = initial_rotation;

        UpdateSnapshotPosition();
    }

    public delegate void OnExit();
    OnExit exitcallback;

    Camera to_set_active = null;
    GameObject background;
    public void exit(Camera to_enable, OnExit onexitcallback = null, Transform exit_transform = null)
    {
        exiting = true;
        entering = false;
        to_set_active = to_enable;
        exitcallback = onexitcallback;

        if (exit_transform != null)
        {
            initial_position.x = exit_transform.localPosition.x;
            initial_position.y = exit_transform.localPosition.y;
            initial_position.z = exit_transform.localPosition.z;

            initial_rotation.x = exit_transform.localRotation.x;
            initial_rotation.y = exit_transform.localRotation.y;
            initial_rotation.z = exit_transform.localRotation.z;
            initial_rotation.w = exit_transform.localRotation.w;
        }
    }

    public Texture2D last_screenshot = null;
    public void TakeSnapshot()
    {
        // Hide UI
        if (ui_to_hide != null) ui_to_hide.SetActive(false);

        last_screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] data = last_screenshot.EncodeToPNG();

        var name = System.DateTime.Now.ToString().Replace(':', '_').Replace('/', '_').Replace('.', '_').Replace(' ', '_');
        var path = Application.persistentDataPath + "/" + name + ".png";
        System.IO.File.WriteAllBytes(path, data);

        // Unhide UI
        if (ui_to_hide != null) ui_to_hide.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (entering)
        {
            if (time_passed < transit_time)
                time_passed += Time.deltaTime;
            else
            {
                time_passed = transit_time;
                entering = false;

                UpdateSnapshotPosition();
            }
        }
        else
        if (exiting)
        {
            if (time_passed > 0.0f)
                time_passed -= Time.deltaTime;
            else
            {
                time_passed = 0.0f;
                exiting = false;
                to_set_active.enabled = (true);

                gameObject.SetActive(false);

                if (exitcallback != null)
                {
                    exitcallback();
                    exitcallback = null;
                }

                UpdateSnapshotPosition();
            }
        }

        var delta = Mathf.Sin(time_passed / transit_time * (Mathf.PI / 2.0f));
        var theta = 1.0f - delta;

        var curroffset = local_offset * delta + initial_position * theta;

        if (entering == false && exiting == false)
        {
            UpdateSnapshotPosition();
        }

        target_rotation = Quaternion.LookRotation(-curroffset);
        var currrotation = Quaternion.Lerp(initial_rotation, target_rotation, delta);

        transform.localPosition = curroffset;
        transform.localRotation = currrotation;
    }

    Vector3 last_mouse = new Vector3();
    bool first_frame = true;
    bool ui_operation = false;
    void UpdateSnapshotPosition()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                ui_operation = true;
            else
                ui_operation = false;
        }

        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            Vector3 delta = new Vector3(0.0f, 0.0f, 0.0f);

            if (first_frame == false && ui_operation == false)
            {
                delta = Input.mousePosition - last_mouse;
            }

            eulerangles.x = Mathf.Clamp(eulerangles.x + delta.y, -maximum_x_angle, maximum_x_angle);
            eulerangles.y = eulerangles.y + delta.x;

            var rot = Matrix4x4.Rotate(Quaternion.Euler(eulerangles.x, eulerangles.y, 0.0f));

            initial_offset = new Vector3(0, offset_distance * Mathf.Sin(Mathf.PI / 8), (0.5f + zoom_slider.value) * offset_distance * Mathf.Cos(Mathf.PI / 8));

            //initial_offset = new Vector3(0, 0, (0.5f + zoom_slider.value) * offset_distance);
            var newoffset = Subject.localToWorldMatrix * rot * initial_offset;
            var gradient = Mathf.Abs(newoffset.y) / ((newoffset.x + newoffset.z) / 2);

            local_offset = newoffset;

            last_mouse = Input.mousePosition;
            first_frame = false;
        }
        else
            first_frame = true;
    }
}

