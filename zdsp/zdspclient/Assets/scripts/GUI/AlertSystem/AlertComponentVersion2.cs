using UnityEngine;

public class AlertComponentVersion2 : MonoBehaviour
{
    public AlertType MyType;
    public AlertType ParentType = AlertType.None;

    public void On()
    {
        gameObject.SetActive(true);
    }

    public void Off()
    {
        gameObject.SetActive(false);
    }
}