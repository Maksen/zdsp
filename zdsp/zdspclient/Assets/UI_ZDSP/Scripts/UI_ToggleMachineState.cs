using UnityEngine;
using UnityEngine.UI;


// Play a Machine State when Toggle is On/Off

public class UI_ToggleMachineState : MonoBehaviour {

    Toggle ToggleButton;
    public string OnStateName;
    public string OffStateName;

    // Use this for initialization
    void Start () {
        ToggleButton = transform.GetComponent<Toggle>();
    }
	
	public void PlayAnimationState()
    {
        if (ToggleButton == null)
            ToggleButton = transform.GetComponent<Toggle>();
        if (ToggleButton.isOn)
        {
            ToggleButton.animator.Play(OnStateName);
        }
        else
        {
            ToggleButton.animator.Play(OffStateName);
        }
    }

}
