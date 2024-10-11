using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Neo_SparseMDD
{
    class WindowDrag : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        public RectTransform dragRect;

        public void OnDrag(PointerEventData eventData)
        {
            dragRect.anchoredPosition += eventData.delta;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            dragRect.SetAsLastSibling();
        }
    }
}
