using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.iOS;

/* Referenced from Boca Alexandru "alexandru.boca@assist.ro"
 * @company: Assist Software
 * https://assist-software.net/blog/rotate-game-object-using-unity-3d-gyroscope
 */
public class GyroRotateController : MonoBehaviour
{
    [SerializeField]
    GameObject[] objectList = null;
    [SerializeField]
    float sensitivity = 5.0f;
    [SerializeField]
    float horizontalClamp = 180.0f;
    [SerializeField]
    float verticalClamp = 180.0f;
    [SerializeField]
    float resetSpeed = 8.0f;
    [SerializeField]
    float resetCooldown = 0.8f;

    bool isGyroEnabled = false;
    Vector3 rotationAngles;
    
    float currTimer = 0.8f; 
    bool rotating = false;

    [Header("Debug UI")]
    [SerializeField]
    Text[] DebugTexts = null;

    // Use this for initialization
    void Awake()
    {
#if UNITY_EDITOR
        Debug.LogWarning("No gyro in the Unity Editor, use Unity Remote to test the functionality!");
#elif UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE || UNITY_WINDOWS_PHONE
        InitGyro();
#endif
        rotationAngles = Vector3.zero;
        currTimer = resetCooldown;
    }

    void OnDestroy()
    {
        objectList = null;
    }

    void InitGyro()
    {
        // Enables gyroscope for android, iOS is enabled by default
        Input.gyro.enabled = true;

        if (Input.gyro.enabled)
            isGyroEnabled = true;
        else
            Debug.LogError("Fail to enable gyroscope for this device");
    }

    // Update is called once per frame
	void Update()
    {
        // Enable gyroscope movement and use the rotation rate of the device to aim
        if (isGyroEnabled)
        {
            GyroRotation();

            // Update Debug text
            //Vector3 attitudeVec = Input.gyro.attitude.eulerAngles;
            //DebugTexts[0].text = string.Format("Attitude Angles: x: {0}, y: {1}, z: {2}", attitudeVec.x, attitudeVec.y, attitudeVec.z);
            //Vector3 rotRateVec = Input.gyro.rotationRate;
            //DebugTexts[1].text = string.Format("Rotation rate: x: {0}, y: {1}, z: {2}", rotRateVec.x, rotRateVec.y, rotRateVec.z);
            //Vector3 gravityVec = Input.gyro.gravity;
            //DebugTexts[2].text = string.Format("Gravity Vec: x: {0}, y: {1}, z: {2}", gravityVec.x, gravityVec.y, gravityVec.z);
        }
        //DebugTexts[3].text = string.Format("RotationAngles: x: {0}, y: {1}, z: {2}", rotationAngles.x, rotationAngles.y, rotationAngles.z);
        //Vector3 rotVec = objectList[0].transform.localEulerAngles;
        //DebugTexts[4].text = string.Format("Rot Angles: x: {0}, y: {1}, z: {2}", rotVec.x, rotVec.y, rotVec.z);

#if UNITY_EDITOR
        TestRotateInEditor();
#endif
    }

    private float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    void TestRotateInEditor()
    {
        bool input = false;
        if (Input.GetKey("w"))
        {
            RotateUp(3.0f);
            input = true;
        }
        if (Input.GetKey("s"))
        {
            RotateDown(-3.0f);
            input = true;
        }
        if (Input.GetKey("a"))
        {
            RotateLeft(3.0f);
            input = true;
        }
        if (Input.GetKey("d"))
        {
            RotateRight(-3.0f);
            input = true;
        }

        if (input)
        {
            rotating = false;
            currTimer = resetCooldown;
        }
        else
        {
            currTimer -= Time.deltaTime;
            if (currTimer < 0 && !rotating)
                rotating = true;
        }

        if (rotating)
        {
            int rotateEnd = 0;
            int len = objectList.Length;
            float dx = 0.0f, dy = 0.0f; 
            for (int i = 0; i < len; ++i)
            {
                Transform objTransform = objectList[i].transform;
                if (objTransform.rotation != Quaternion.identity)
                {
                    Quaternion tempQuat = objTransform.rotation;
                    objTransform.rotation = Quaternion.Slerp(tempQuat, Quaternion.identity, Time.deltaTime * resetSpeed);
                    dx = WrapAngle(tempQuat.eulerAngles.x) - WrapAngle(objTransform.rotation.eulerAngles.x);
                    dy = WrapAngle(tempQuat.eulerAngles.y) - WrapAngle(objTransform.rotation.eulerAngles.y);
                }
                else
                    ++rotateEnd;
            }
            rotationAngles = new Vector3(rotationAngles.x-dy, rotationAngles.y-dx, rotationAngles.z);
            if (rotateEnd == len)
            {
                rotating = false;
                currTimer = resetCooldown;
            }
        }
    }

    public void GyroRotation()
    {
        // Get values from the gyroscope
        // Map the rotationRate for continuous rotation when the device is moving
        float rrx = Input.gyro.rotationRate.x;
        float xFiltered = FilterGyroValues(rrx);
        if (xFiltered > 0)
            RotateUp(xFiltered);
        else
            RotateDown(xFiltered);

        float rry = Input.gyro.rotationRate.y;
        float yFiltered = FilterGyroValues(rry);
        if (yFiltered > 0)
            RotateLeft(yFiltered);
        else
            RotateRight(yFiltered);
        
        // No movement in gyroscope
        if (xFiltered != 0 || yFiltered != 0)
        {
            rotating = false;
            currTimer = resetCooldown;
        }
        else
        {
            currTimer -= Time.deltaTime;
            if (currTimer < 0 && !rotating)
                rotating = true;
        }

        if (rotating)
        {
            int rotateEnded = 0;
            int len = objectList.Length;
            float dx = 0.0f, dy = 0.0f;
            for (int i = 0; i < len; ++i)
            {
                Transform objTransform = objectList[i].transform;
                if (objTransform.rotation != Quaternion.identity)
                {
                    Quaternion tempQuat = objTransform.rotation;
                    objTransform.rotation = Quaternion.Slerp(tempQuat, Quaternion.identity, Time.deltaTime * resetSpeed);
                    dx = WrapAngle(tempQuat.eulerAngles.x) - WrapAngle(objTransform.rotation.eulerAngles.x);
                    dy = WrapAngle(tempQuat.eulerAngles.y) - WrapAngle(objTransform.rotation.eulerAngles.y);
                }
                else
                    ++rotateEnded;
            }
            rotationAngles = new Vector3(rotationAngles.x-dy, rotationAngles.y-dx, rotationAngles.z);
            if (rotateEnded == len)
            {
                rotating = false;
                currTimer = resetCooldown;
            }
        }
    }

    float FilterGyroValues(float axis)
    {
	    return (axis < -0.1 || axis > 0.1) ? axis : 0;
    }

    void RotateUp(float angle)
    {
        if (rotationAngles.y > -verticalClamp)
            RotateUpDown(angle);
    }

    void RotateDown(float angle)
    {
        if (rotationAngles.y < verticalClamp)
            RotateUpDown(angle);
    }

    void RotateLeft(float angle)
    {
        if (rotationAngles.x > -horizontalClamp)
            RotateRightLeft(angle);
    }

    void RotateRight(float angle)
    {
        if (rotationAngles.x < horizontalClamp)
            RotateRightLeft(angle);
    }

    void RotateUpDown(float axis)
    {
        float realAngle = -axis * Time.deltaTime * sensitivity;
        int len = objectList.Length;
        for (int i = 0; i < len; ++i)
        {
            Transform objTransform = objectList[i].transform;
            objTransform.RotateAround(objTransform.position, objTransform.right, realAngle);
        }
        rotationAngles = new Vector3(rotationAngles.x, rotationAngles.y+realAngle, rotationAngles.z);
    }

    void RotateRightLeft(float axis)
    {
        float realAngle = -axis * Time.deltaTime * sensitivity;
        int len = objectList.Length;
        for (int i = 0; i < len; ++i)
        {
            Transform objTransform = objectList[i].transform;
            objTransform.RotateAround(objTransform.position, Vector3.up, realAngle);
        }
        rotationAngles = new Vector3(rotationAngles.x+realAngle, rotationAngles.y, rotationAngles.z);
    }
}
