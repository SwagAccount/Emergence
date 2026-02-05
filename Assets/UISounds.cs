using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverSound?.Play();
    }
    public static SoundEvent ClickSound;
    public static SoundEvent HoverSound;
    // Start is called before the first frame update
    void Awake()
    {
        ClickSound = Resources.Load<SoundEvent>("Sounds/UI/MenuClick");
        HoverSound = Resources.Load<SoundEvent>("Sounds/UI/MenuHover");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ClickSound?.Play();
    }
}
