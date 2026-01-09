using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragRay : MonoBehaviour
{
    private Canvas curCanvas;
    private RectTransform rect;
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private PointerEventData pointerData;
    private List<RaycastResult> _results = new List<RaycastResult>();
    
    public void Init() 
    {
        rect = GetComponent<RectTransform>();
        eventSystem = EventSystem.current;
        pointerData = new PointerEventData(eventSystem);
    }

    public void SetCanvas(Canvas canvas)
    {
        curCanvas = canvas;
        raycaster = canvas.GetComponent<GraphicRaycaster>();
    }

    public Vector2Int RayCast()
    {
        var screenPos = RectTransformUtility.WorldToScreenPoint(curCanvas.worldCamera, rect.position);
        _results.Clear();
        pointerData.position = screenPos;
        raycaster.Raycast(pointerData, _results);
        
        foreach (RaycastResult result in _results)
        {
            var script = result.gameObject.GetComponentInParent<BlockItem>();
            if (script != null)
            {
                return script.Pos;
            }
        }
        return Vector2Int.one * -1;
    }
}