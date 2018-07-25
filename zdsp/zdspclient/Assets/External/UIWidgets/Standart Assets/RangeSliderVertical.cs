﻿using UnityEngine;

namespace UIWidgets
{
	[AddComponentMenu("UI/RangeSliderVertical", 300)]
	/// <summary>
	/// Vertical range slider.
	/// </summary>
	public class RangeSliderVertical : RangeSlider
	{
		protected override bool IsHorizontal ()
		{
			return false;
		}

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("GameObject/UI/RangeSliderVertical", false, 1150)]
		static void CreateObject()
		{
			Utilites.CreateWidgetFromAsset("RangeSliderVertical");
		}
		#endif
	}
}