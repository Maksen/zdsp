using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public enum ProgressbarCTypes
{
    Text = 0,
    NoText = 1,
}

public enum ProgressbarCTextTypes
{
    None = 0,
    Percent = 1,
    Range = 2,
}

public class UI_ProgressBarC : MonoBehaviour {

    public enum ProgressbarType
    {
        TypeA = 1,
        TypeB = 2,
    }

    [SerializeField]
    private float _max;

    [SerializeField]
    private bool _canExceedMax = false;

    public bool CanExceedMax
    {
        get
        {
            return _canExceedMax;
        }
        set
        {
            _canExceedMax = value;
            UpdateProgressbar();
            UpdateTips();
        }
    }

    public float Max
    {
        get
        {
            return _max;
        }
        set
        {
            _max = value;
            UpdateProgressbar();
            UpdateTips();
        }
    }

    [SerializeField]
    private float _value;
    
    public float Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (value > Max && !_canExceedMax)
            {
                value = Max;
            }
            _value = value;
            UpdateProgressbar();
            UpdateTips();

        }
    }

    //[SerializeField]
    //private int _decimal;


    [SerializeField]
    private Image barImage;

    public Image BarImage
    {
        get
        {
            return barImage;
        }
        set
        {
            barImage = value;
        }
    }

    [SerializeField]
    private ProgressbarCTypes type = ProgressbarCTypes.Text;
    
    public ProgressbarCTypes Type
    {
        get
        {
            return type;
        }
        set
        {
            type = value;
            ToggleType();
        }
    }

    [SerializeField]
    public Text BarText;

    [SerializeField]
    private ProgressbarCTextTypes textType = ProgressbarCTextTypes.None;

    [SerializeField]
    public ProgressbarCTextTypes TextType
    {
        get
        {
            return textType;
        }
        set
        {
            textType = value;
            ToggleTextType();
        }
    }

    [SerializeField]
    public bool showMax = false;

    [Header("Any parented gameobject need to have their X axis to be 0 and anchor min max X axis to be 0")]
    [Tooltip("Object follow tip of bar")]
    [SerializeField]
    RectTransform typeA;

    [Header("Any parented gameobject need to have their X axis to be 0 and anchor min max X axis to be 0")]
    [Tooltip("Object  follow tip and play animation 1 time when value changes")]
    [SerializeField]
    RectTransform typeB;

    Func<UI_ProgressBarC, string> textFunc = TextPercent;

    public Func<UI_ProgressBarC, string> TextFunc
    {
        get
        {
            return textFunc;
        }
        set
        {
            textFunc = value;
            UpdateText();
        }
    }

    public static string TextNone(UI_ProgressBarC bar)
    {
        return string.Empty;
    }

    public static string TextPercent(UI_ProgressBarC bar)
    {
        float percent = bar.Value / bar.Max;
        //return percent.ToString("P");
        return string.Format("{0:P0}", percent);
    }

    public static string TextRange(UI_ProgressBarC bar)
    {
        return string.Format("{0} / {1}", bar.Value, bar.Max);
    }

    public static string TextMax(UI_ProgressBarC bar)
    {
        return string.Format("{0}", "MAX");
    }

    [SerializeField]
    private ProgressbarType _ProgressBarTypeFlagMask;

    [SerializeField]
    public ProgressbarType ProgressBarTypeFlagMask
    {
        get
        {
            return _ProgressBarTypeFlagMask;
        }
        set
        {
            _ProgressBarTypeFlagMask = value;
        }
    }

    float progressbarwidth;
    float lastValue;
    bool isEverything;
    bool isNothing;
    
    void Start () {

        Init();

        lastValue = Value;
        progressbarwidth = Mathf.Abs(GetComponent<RectTransform>().sizeDelta.x);
        UpdateTips();
    }

   
    void Init()
    {
        int maxvalue = Enum.GetValues(typeof(ProgressbarType)).Cast<int>().Max();
        maxvalue <<= 1;
        maxvalue -= 1;

        isEverything = false;
        isNothing = false;

        if ((int)ProgressBarTypeFlagMask == 0)//nothing
        {
            if (typeA != null)
                typeA.gameObject.SetActive(false);
            if (typeB != null)
                typeB.gameObject.SetActive(false);

            isNothing = true;
        }
        else if (((int)ProgressBarTypeFlagMask & maxvalue) == maxvalue)//everything
        {
            if (typeA != null)
                typeA.gameObject.SetActive(true);
            if (typeB != null)
                typeB.gameObject.SetActive(true);

            isEverything = true;
        }
        else if ((ProgressBarTypeFlagMask & ProgressbarType.TypeA) == ProgressbarType.TypeA)//type A
        {
            if (typeA != null)
                typeA.gameObject.SetActive(true);
            if (typeB != null)
                typeB.gameObject.SetActive(false);
        }
        else if ((ProgressBarTypeFlagMask & ProgressbarType.TypeB) == ProgressbarType.TypeB)//type B
        {
            if (typeA != null)
                typeA.gameObject.SetActive(false);
            if (typeB != null)
                typeB.gameObject.SetActive(true);
        }

       
    }

    public void Refresh()
    {
        ToggleType();
        ToggleTextType();
        UpdateProgressbar();
        UpdateTips();
        Init();
    }

    // Update is called once per frame
    void UpdateProgressbar ()
    {
	    if (Max <= 0)
        {
            return;
        }

        if (barImage != null)
        {
            float percentage = Value / Max;
            if (percentage > 1f) percentage = 1f;
            barImage.fillAmount = percentage;
        }

        UpdateText();
    }

    void UpdateTips()
    {
        if(isNothing == true)
        {
            return;
        }
        else if(isEverything == true && typeA != null && typeB != null)
        {
            if (Application.isPlaying == false)
                progressbarwidth = GetComponent<RectTransform>().sizeDelta.x;

            if ((ProgressBarTypeFlagMask & ProgressbarType.TypeA) == ProgressbarType.TypeA)
            {
                typeA.anchoredPosition = GetUpdatedPosition(typeA);
            }
            if ((ProgressBarTypeFlagMask & ProgressbarType.TypeB) == ProgressbarType.TypeB)
            {
                typeB.anchoredPosition = GetUpdatedPosition(typeB);
                if (lastValue != Value)
                    typeB.gameObject.SetActive(true);
            }
        }
        else if(ProgressBarTypeFlagMask == ProgressbarType.TypeA && typeA != null)
        {
            if (Application.isPlaying == false)
                progressbarwidth = GetComponent<RectTransform>().sizeDelta.x;
           
            typeA.anchoredPosition = GetUpdatedPosition(typeA);
        }
        else if (ProgressBarTypeFlagMask == ProgressbarType.TypeB && typeB != null)
        {
            if (Application.isPlaying == false)
                progressbarwidth = GetComponent<RectTransform>().sizeDelta.x;

            typeB.anchoredPosition = GetUpdatedPosition(typeB);
            if (lastValue != Value)
                typeB.gameObject.SetActive(true);
        }

        lastValue = Value;
    }

    Vector2 GetUpdatedPosition(RectTransform type)
    {
        float posx = 0;
        float posy = 0;
        posx = (Value * progressbarwidth) / Max;
        posy = type.anchoredPosition.y;
        return new Vector2(posx, posy);
    }

    void UpdateText()
    {
        if (Value >= Max && showMax)
        {
            textFunc = TextMax;
        }

        var text = textFunc(this);
        if (BarText != null)
        {
            BarText.text = text;
        }
    }

    void ToggleType()
    {
        bool is_deterimate = (type == ProgressbarCTypes.Text);

        if (BarText != null)
        {
            BarText.gameObject.SetActive(is_deterimate);
        }
    }

    void ToggleTextType()
    {
        if (TextType == ProgressbarCTextTypes.None)
        {
            textFunc = TextNone;
        }
        else if (TextType == ProgressbarCTextTypes.Percent)
        {
            textFunc = TextPercent;
        }
        else if (TextType == ProgressbarCTextTypes.Range)
        {
            textFunc = TextRange;
        }
    }
}
