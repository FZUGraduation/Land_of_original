using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseEventCenter
{
    private class EventListener
    {
        public Action<object[]> Listener { get; }
        public object Owner { get; }

        public EventListener(Action<object[]> listener, object owner)
        {
            Listener = listener;
            Owner = owner;
        }
    }

    private Dictionary<string, List<EventListener>> eventDictionary = new Dictionary<string, List<EventListener>>();
    private Dictionary<object, HashSet<string>> objectEventDictionary = new Dictionary<object, HashSet<string>>();

    /// <summary> 注册事件 </summary>
    public void On(string eventName, Action<object[]> listener, object owner)
    {
        if (listener == null)
        {
            Debug.LogError($"Listener for event {eventName} is null.");
            return;
        }

        if (owner == null)
        {
            Debug.LogError($"Owner for event {eventName} is null.");
            return;
        }
        if (!eventDictionary.TryGetValue(eventName, out var listeners))
        {
            listeners = new List<EventListener>();
            eventDictionary[eventName] = listeners;
        }
        listeners.Add(new EventListener(listener, owner));

        if (!objectEventDictionary.TryGetValue(owner, out var events))
        {
            events = new HashSet<string>();
            objectEventDictionary[owner] = events;
        }
        events.Add(eventName);
    }

    /// <summary> 无参数的事件 </summary>
    public void On(string eventName, Action listener, object owner)
    {
        On(eventName, _ => listener(), owner);
    }

    /// <summary>/ 注销事件 </summary>
    public void Off(string eventName, Action<object[]> listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName].RemoveAll(e => e.Listener == listener);
            if (eventDictionary[eventName].Count == 0)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    /// <summary> 注销某个对象注册的所有事件 </summary>
    public void OffAll(object owner)
    {
        if (objectEventDictionary.ContainsKey(owner))
        {
            foreach (var eventName in objectEventDictionary[owner])
            {
                if (eventDictionary.ContainsKey(eventName))
                {
                    eventDictionary[eventName].RemoveAll(e => e.Owner == owner);
                    if (eventDictionary[eventName].Count == 0)
                    {
                        eventDictionary.Remove(eventName);
                    }
                }
            }
            objectEventDictionary.Remove(owner);
        }
    }

    /// <summary>分发事件</summary>
    public void Emit(string eventName, params object[] parameters)
    {
        if (eventDictionary.TryGetValue(eventName, out var listeners))
        {
            //在Emit中，如果某个监听者的回调修改了事件列表（如注销自身），会导致foreach遍历崩溃。所以遍历时使用副本，避免直接操作原集合：ToList()
            foreach (var listener in listeners.ToList())
            {
                if (listener.Owner == null)
                {
                    listeners.Remove(listener);
                    continue;
                }
                try
                {
                    listener.Listener?.Invoke(parameters);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Event {eventName} error: {e}");
                }
            }
            if (listeners.Count == 0)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    public void ClearEvent()
    {
        eventDictionary.Clear();
        objectEventDictionary.Clear();
    }
    private static int eventNumber = 0;
    /// <summary> 生成唯一标志 </summary>
    public static string GetEventName(string str = "")
    {
        return $"Event_{eventNumber++}_{str}";
    }
}