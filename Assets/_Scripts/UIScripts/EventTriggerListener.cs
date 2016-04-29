using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
/// <summary>
/// 监听UGUI的事件
/// </summary>
public class EventTriggerListener : UnityEngine.EventSystems.EventTrigger
{
    public delegate void TouchDelegate(GameObject target);
    public TouchDelegate onClick;
    public TouchDelegate onEnter;
    public TouchDelegate onExit;
    public TouchDelegate onUp;
    static public EventTriggerListener Get(GameObject target)
    {
        EventTriggerListener listener = target.GetComponent<EventTriggerListener>();
        if (listener == null) listener = target.AddComponent<EventTriggerListener>();
        return listener;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
        {
            onClick(gameObject);
        }
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        
        if (onEnter != null) onEnter(gameObject);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject);
    }
}