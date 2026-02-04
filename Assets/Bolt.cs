using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bolt : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool Free;
    public bool Dragging;

    public List<BoltHole> BoltHoles = new();

    private Rigidbody rigidBody;
    private RectTransform rectTransform;

    public Canvas parentCanvas;
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rigidBody.isKinematic = !Free || Dragging;

        if (!Dragging)
            return;

        var mousePos = MousePos();

        rectTransform.transform.localPosition = mousePos;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Free)
            return;

        Dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Dragging && Free)
        {
            rigidBody.isKinematic = false;
            rigidBody.AddForce(eventData.delta);

            foreach(var boltHole in BoltHoles)
            {
                if (boltHole.Bolt != null)
                    continue;

                if (Vector3.Distance(transform.position, boltHole.transform.position) > 0.03f)
                    continue;
                boltHole.PlaceBolt(this);
                break;
            }
        }
        Dragging = false;
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
