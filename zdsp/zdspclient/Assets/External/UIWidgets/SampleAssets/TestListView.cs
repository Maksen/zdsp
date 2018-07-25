using UnityEngine;
using UnityEngine.UI;
using UIWidgets;

namespace UIWidgetsSamples {
	[RequireComponent(typeof(Button))]
	public class TestListView : MonoBehaviour {
		public ListView listView;

		// Use this for initialization
		void Start()
		{
			var button = GetComponent<Button>();
			button.onClick.AddListener(Click);
		}
		
		void Click()
		{
			Debug.Log(listView.Add("Added from script"));
			Debug.Log(listView.Add("Added from script"));
			Debug.Log(listView.Add("Added from script"));
			listView.Remove("Caster");
		}
		
	}
}