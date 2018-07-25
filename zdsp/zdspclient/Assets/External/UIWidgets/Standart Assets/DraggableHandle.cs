using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace UIWidgets {
	/// <summary>
	/// Draggable handle.
	/// </summary>
	public class DraggableHandle : MonoBehaviour, IDragHandler, IInitializePotentialDragHandler {
		RectTransform drag;
		Canvas canvas;
		RectTransform canvasRect;

		/// <summary>
		/// Set the specified draggable object.
		/// </summary>
		/// <param name="newDrag">New drag.</param>
		public void Drag(RectTransform newDrag)
		{
			drag = newDrag;
		}
		
		/// <summary>
		/// Raises the initialize potential drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			canvasRect = Utilites.FindCanvas(transform) as RectTransform;
			canvas = canvasRect.GetComponent<Canvas>();
		}
		
		/// <summary>
		/// Raises the drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnDrag(PointerEventData eventData)
		{
			if (canvas==null)
			{
				throw new MissingComponentException(gameObject.name + " not in Canvas hierarchy.");
			}
			var delta = Utilites.CalculateDragPosition(eventData.position, canvas, canvasRect)
				- Utilites.CalculateDragPosition(eventData.position - eventData.delta, canvas, canvasRect);
			drag.localPosition += delta;
		}
	}
}