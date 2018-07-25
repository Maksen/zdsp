using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;

namespace UIWidgets {
	/// <summary>
	/// On change event.
	/// </summary>
	public class OnChangeEventInt : UnityEvent<int>
	{
	}
	/// <summary>
	/// Submit event.
	/// </summary>
	public class SubmitEventInt : UnityEvent<int>
	{
	}

	[AddComponentMenu("UI/Spinner", 270)]
	/// <summary>
	/// Spinner.
	/// http://ilih.ru/images/unity-assets/UIWidgets/Spinner.png
	/// </summary>
	public class Spinner : InputField, IPointerDownHandler {
		[SerializeField]
		int _min;

		[SerializeField]
		int _max = 100;

		[SerializeField]
		int _step = 1;

		[SerializeField]
		int _value;

        [SerializeField]
        bool _fasterStep = false;
        

		/// <summary>
		/// Delay of hold in seconds for permanent increase/descrease value.
		/// </summary>
		[SerializeField]
		public float HoldStartDelay = 0.5f;

		/// <summary>
		/// Delay of hold in seconds between increase/descrease value.
		/// </summary>
		[SerializeField]
		public float HoldChangeDelay = 0.1f;

		/// <summary>
		/// Gets or sets the minimum.
		/// </summary>
		/// <value>The minimum.</value>
		public int Min {
			get {
				return _min;
			}
			set {
				_min = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum.
		/// </summary>
		/// <value>The maximum.</value>
		public int Max {
			get {
				return _max;
			}
			set {
				_max = value;
			}
		}

		/// <summary>
		/// Gets or sets the step.
		/// </summary>
		/// <value>The step.</value>
		public int Step {
			get {
				return _step;
			}
			set {
				_step = value;
			}
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public int Value {
			get {
				return _value;
			}
			set {
				_value = InBounds(value);
				text = _value.ToString();
                UpdateButtonState(value);
			}
		}

        public bool FasterStep {
            get
            {
                return _fasterStep;
            }
            set
            {
                _fasterStep = value;
            }
        }

		[SerializeField]
		ButtonAdvanced _plusButton;

		[SerializeField]
		ButtonAdvanced _minusButton;

        [SerializeField]
        bool _holdButton = true;

        public bool HoldButton
        {
            get { return _holdButton; }
            set { _holdButton = value; }
        }

        /// <summary>
        /// Gets or sets the plus button.
        /// </summary>
        /// <value>The plus button.</value>
        public ButtonAdvanced PlusButton {
			get {
				return _plusButton;
			}
			set {
				if (_plusButton!=null)
				{
					_plusButton.onClick.RemoveListener(OnPlusClick);
					_plusButton.onPointerDown.RemoveListener(OnPlusButtonDown);
					_plusButton.onPointerUp.RemoveListener(OnPlusButtonUp);
				}
				_plusButton = value;
				if (_plusButton!=null)
				{
					_plusButton.onClick.AddListener(OnPlusClick);
					_plusButton.onPointerDown.AddListener(OnPlusButtonDown);
					_plusButton.onPointerUp.AddListener(OnPlusButtonUp);
				}
			}
		}

		/// <summary>
		/// Gets or sets the minus button.
		/// </summary>
		/// <value>The minus button.</value>
		public ButtonAdvanced MinusButton {
			get {
				return _minusButton;
			}
			set {
				if (_minusButton!=null)
				{
					_minusButton.onClick.RemoveListener(OnMinusClick);
					_minusButton.onPointerDown.RemoveListener(OnMinusButtonDown);
					_minusButton.onPointerUp.RemoveListener(OnMinusButtonUp);
				}
				_minusButton = value;
				if (_minusButton!=null)
				{
					_minusButton.onClick.AddListener(OnMinusClick);
					_minusButton.onPointerDown.AddListener(OnMinusButtonDown);
					_minusButton.onPointerUp.AddListener(OnMinusButtonUp);
				}
			}
		}

		/// <summary>
		/// onValueChange event.
		/// </summary>
		public UnityEvent<int> onValueChangeInt = new OnChangeEventInt();

		/// <summary>
		/// onEndEdit event.
		/// </summary>
		public UnityEvent<int> onEndEditInt = new SubmitEventInt();

		/// <summary>
		/// onPlusClick event.
		/// </summary>
		public UnityEvent onPlusClick = new UnityEvent();

		/// <summary>
		/// onMinusClick event.
		/// </summary>
		public UnityEvent onMinusClick = new UnityEvent();

        /// <summary>
        /// Increase value on step.
        /// </summary>
        public void Plus(int multiplier = 1)
        {
            Value += Step * multiplier;
        }

        /// <summary>
        /// Decrease value on step.
        /// </summary>
        public void Minus(int multiplier = 1)
        {
            Value -= Step * multiplier;
        }

		/// <summary>
		/// Start this instance.
		/// </summary>
		protected override void Start()
		{
			base.Start();

			onValidateInput = IntValidate;

			base.onValueChanged.AddListener(ValueChange);
			base.onEndEdit.AddListener(ValueEndEdit);

			PlusButton = _plusButton;
			MinusButton = _minusButton;
			Value = _value;
		}

		IEnumerator HoldPlus()
		{
			yield return new WaitForSeconds(HoldStartDelay);
			while (Value < Max)
			{
                if (_fasterStep)
                {
                    if (_value >= 1000)
                        Plus(100);
                    else if (_value >= 100)
                        Plus(10);
                    else
                        Plus();
                }
                else
                {
                    Plus();
                }
				yield return new WaitForSeconds(HoldChangeDelay);
			}
		}

		IEnumerator HoldMinus()
		{
			yield return new WaitForSeconds(HoldStartDelay);
			while (Value > Min)
			{

                if (_fasterStep)
                {
                    if (_value >= 1000)
                        Minus(100);
                    else if (_value >= 100)
                        Minus(10);
                    else
                        Minus();
                }
                else
                {
                    Minus();
                }

                yield return new WaitForSeconds(HoldChangeDelay);
			}
		}

		/// <summary>
		/// Raises the minus click event.
		/// </summary>
		public void OnMinusClick()
		{
			Minus();
			onMinusClick.Invoke();
		}

		/// <summary>
		/// Raises the plus click event.
		/// </summary>
		public void OnPlusClick()
		{
			Plus();
			onPlusClick.Invoke();
		}

		IEnumerator corutine;

		/// <summary>
		/// Raises the plus button down event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public void OnPlusButtonDown(PointerEventData eventData)
		{
            if(_holdButton)
            {
                if (corutine != null)
                    StopCoroutine(corutine);
                corutine = HoldPlus();
                StartCoroutine(corutine);
            }   
		}

		/// <summary>
		/// Raises the plus button up event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public void OnPlusButtonUp(PointerEventData eventData)
		{
            if(corutine != null)
			    StopCoroutine(corutine);
            corutine = null;
        }

		/// <summary>
		/// Raises the minus button down event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public void OnMinusButtonDown(PointerEventData eventData)
		{
            if(_holdButton)
            {
                if (corutine != null)
                    StopCoroutine(corutine);
                corutine = HoldMinus();
                StartCoroutine(corutine);
            }
		}

		/// <summary>
		/// Raises the minus button up event.
		/// </summary>
		/// <param name="eventData">Current event data.</param>
		public void OnMinusButtonUp(PointerEventData eventData)
		{
            if(corutine != null)
			    StopCoroutine(corutine);
            corutine = null;
        }

		/// <summary>
		/// Ons the destroy.
		/// </summary>
		protected void onDestroy()
		{
			base.onValueChanged.RemoveListener(ValueChange);
			base.onEndEdit.RemoveListener(ValueEndEdit);

			PlusButton = null;
			MinusButton = null;
		}

		void ValueChange(string value)
		{
			if (value.Length==0)
			{
				value = "0";
			}
			_value = int.Parse(value);
			onValueChangeInt.Invoke(Value);
		}

		void ValueEndEdit(string value)
		{
			if (value.Length==0)
			{
				value = "0";
			}
			_value = int.Parse(value);
			onEndEditInt.Invoke(Value);
		}

		char IntValidate(string validateText, int charIndex, char addedChar)
		{
			var number = validateText.Insert(charIndex, addedChar.ToString());

			if ((Min > 0) && (charIndex==0) && (charIndex=='-'))
			{
				return (char)0;
			}

			var min_parse_length = ((number.Length > 0) && (number[0]=='-')) ? 2 : 1;
			if (number.Length >= min_parse_length)
			{
				int new_value;
				if ((!int.TryParse(number, out new_value)))
				{
					return (char)0;
				}
				
				if (new_value!=InBounds(new_value))
				{
					return (char)0;
				}

				_value = new_value;
			}

			return addedChar;
		}

		int InBounds(int value)
		{
			if (value < _min)
			{
				return _min;
			}
			if (value > _max)
			{
				return _max;
			}
			return value;
        }

        void UpdateButtonState(int value)
        {
            MinusButton.interactable = true;
            PlusButton.interactable = true;

            if(_min == _max)
            {
                MinusButton.interactable = false;
                PlusButton.interactable = false;

                return;
            }
            if (value <= _min)
            {
                MinusButton.interactable = false;
            }
            if (value >= _max)
            {
                PlusButton.interactable = false;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/UI/Spinner", false, 1160)]
		static void CreateObject()
		{
			Utilites.CreateWidgetFromAsset("Spinner");
		}
#endif
	}
}