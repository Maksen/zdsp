using UnityEngine;

public class PathWalking : MonoBehaviour
{
    public Vector3[] positions;
    public float movespeed;
    public float rotatespeed;
    int CurrentPosIndex
    {
        get
        {
            return _currentPosIndex;
        }
        set
        {
            if (value >= positions.Length)
                _currentPosIndex = 0;
            else
                _currentPosIndex = value;
        }
    }
    int _currentPosIndex;
    Vector3 currentVec;
    bool isRotate;
    // Use this for initialization
    void Start()
    {
        if (positions.Length <= 1)
        {
            Destroy(GetComponent<PathWalking>());
            return;
        }
        
        isRotate = true;
        CurrentPosIndex = 0;
        transform.position = positions[CurrentPosIndex];
        currentVec = positions[CurrentPosIndex + 1] - positions[CurrentPosIndex];

    }

    void FixedUpdate()
    {
        moveobj();
        setrotation();
    }

    void setrotation()
    {
        if (isRotate == true)
        {
            if (Vector3.Angle(transform.forward, currentVec) <= 0.0f)
            {
                isRotate = false;
                if (CurrentPosIndex >= positions.Length - 1)
                    currentVec = positions[0];
                else
                    currentVec = positions[CurrentPosIndex + 1];
            }
            else
            {
                Vector3 newforward = Vector3.RotateTowards(transform.forward, currentVec, Time.deltaTime * rotatespeed, 0);
                transform.rotation = Quaternion.LookRotation(newforward);
            }
        }

    }
    void moveobj()
    {
        if (isRotate == false)
        {
            if (Vector3.Distance(transform.position, currentVec) <= 0.1f)
            {
                isRotate = true;
                transform.position = currentVec;
                CurrentPosIndex++;
                if (CurrentPosIndex >= positions.Length - 1)
                {
                    currentVec = positions[0] - positions[CurrentPosIndex];
                }
                else
                {
                    currentVec = positions[CurrentPosIndex + 1] - positions[CurrentPosIndex];
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, currentVec, Time.deltaTime * movespeed);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < positions.Length; i++)
        {
            Gizmos.DrawWireSphere(positions[i], 0.1f);
        }
    }
}
