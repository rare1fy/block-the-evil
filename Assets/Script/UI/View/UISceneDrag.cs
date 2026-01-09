using UnityEngine;
using UnityEngine.EventSystems;

public class UISceneDrag : UIBase,IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ChapterSceneManager ChapterSceneManager = ChapterSceneManager._Instance;

    private float dragSensitivity = 0.01f; // 拖拽灵敏度
    private Vector2 dragLastPostion;    //上一帧的拖拽位置
    //private bool isDragging = false;



    public void OnBeginDrag(PointerEventData eventData)
    {
        dragLastPostion = eventData.position;
        ChapterSceneManager.MoveCamare(0,0);
    }

    public void OnDrag(PointerEventData eventData)
    {
        
        // 计算拖拽偏移量
        Vector2 dragInterval = (eventData.position- dragLastPostion) * dragSensitivity;
        dragLastPostion = eventData.position;
        ChapterSceneManager.MoveCamare(-dragInterval.x, -dragInterval.y);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 dragInterval = (eventData.position - dragLastPostion) * dragSensitivity;
        dragLastPostion = eventData.position;
        ChapterSceneManager.MoveCamare(-dragInterval.x, -dragInterval.y);
    }
}
