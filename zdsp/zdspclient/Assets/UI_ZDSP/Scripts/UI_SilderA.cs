using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_SilderA : MonoBehaviour {

    public int Min = 1;
    public int Max = 100;
    
    public int Value;
    public bool ResetValue = true;

    public GameObject   slider      = null;
    public Image        SliderBar   = null;
    public Button       ButtonPlus  = null;
    public Button       ButtonMinus = null;
    public Text         SliderText  = null;

    private float   realRange   = 180f; // Maximum slider movement range is always 180
    private float   tick        = 0f;
    private float   fMin        = 0f;
    private float   fMax        = 0f;
    private float   fValue      = 0f;
    private float   minMaxRange = 0f;

    // Use this for initialization
    void Start () {
        if (ResetValue)
            Value = Min;

        fMin = Min;
        fMax = Max;
        fValue = Value;
        minMaxRange = (fMax - fMin) + 1f;

        tick = (realRange / minMaxRange);

        if (SliderText != null)
        {
            SliderText.text = Value.ToString();
        }

        if (ButtonPlus != null)
        {
            ButtonPlus.onClick.AddListener(() => OnClickPlus());
        }

        if (ButtonMinus != null)
        {
            ButtonMinus.onClick.AddListener(() => OnClickMinus());
        }

        if (slider != null && slider.GetComponent<Slider>() != null)
        {
            slider.GetComponent<Slider>().onValueChanged.AddListener(UpdateText);
            //temp.transform.Find("crossButton").GetComponent<Button>().onClick.AddListener(() => CloseBtn(temp, id));
        }
    }

    public void OnClickPlus()
    {
        if(ButtonPlus != null && SliderText != null)
        {
            Value += 1;

            SetText();
            UpdateSlider();
        }
    }

    public void OnClickMinus()
    {
        if (ButtonMinus != null && SliderText != null)
        {
            Value -= 1;

            SetText();
            UpdateSlider();
        }
    }

    public void ResetSlider()
    {
        fMin = Min;
        fMax = Max;
        fValue = Value;
        minMaxRange = (fMax - fMin) + 1f;
        SliderText.text = Value.ToString();
        tick = (realRange / minMaxRange);
        UpdateSlider();
    }

    private void UpdateSlider()
    {
        float t = ((fValue - fMin) / (fMax - fMin)) * minMaxRange;
        float newPos = (t * tick) - 90;
        if(SliderBar != null)
            SliderBar.rectTransform.localPosition = new Vector3(newPos, 0, 0);
    }

    private void UpdateText(float value)
    {
        float maxSliderValue = slider.GetComponent<Slider>().maxValue;
        float t = (value / maxSliderValue);
        float realValue = fMin + t * (fMax - fMin);
       

        Value = (int)realValue;
        SetText();
       UpdateSlider();
    }

    private void SetText()
    {
        if (Value < Min)
        {
            Value = Min;
        }
        else if (Value > Max)
        {
            Value = Max;
        }

        fValue = Value;
        if (SliderText != null)
            SliderText.text = Value.ToString();
    }
}
