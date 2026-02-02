using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Wheel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool Dragging;
    public Canvas parentCanvas;
    private RectTransform rectTransform;
    private Vector2 prevMousePos;
    public float RotateSpeed = 0.5f;
    public float OptimalDistance = 150;
    public float rot;

    public void OnBeginDrag(PointerEventData eventData)
    {
        prevMousePos = MousePos();
        Dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Dragging = false;
    }

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Update()
    {
        if (!Dragging)
            return;

       

        var mousePos = MousePos();

        var dis = Vector3.Distance(mousePos, rectTransform.anchoredPosition) / OptimalDistance;
        var disMult = Mathf.Clamp01(dis);

        var lastDir = (prevMousePos - rectTransform.anchoredPosition).normalized;
        var thisDir = (mousePos - rectTransform.anchoredPosition).normalized;

        float delta = Vector2.SignedAngle(lastDir, thisDir);
        rot += delta * RotateSpeed * disMult;

        rectTransform.rotation = Quaternion.Euler(0,0,rot);

        prevMousePos = mousePos;

    }

    public Vector2 MousePos()
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
           rectTransform.parent as RectTransform,
           Input.mousePosition,
           parentCanvas.worldCamera,
           out Vector2 localPos))
        {
            return localPos;
        }

        return Vector2.zero;
    }
}
