﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ConversationWithActions
{
	public ConversationEvent conversationEvent;
	public List<UnityEvent> events = new List<UnityEvent>();
	public int Length { get { return conversationEvent?.conversation.Length ?? 0; } }
	public UnityEvent endEvent = new UnityEvent();

	public UnityEvent GetEndEvent() => endEvent;

	public void AddAction(int index, Action action)
	{
		if (action == null) return;
		if (index >= Length) return;
		EnsureLength();
		events[index].AddListener(new UnityAction(action));
	}

	public void InvokeEvent(int index)
	{
		if (index >= Length) return;
		EnsureLength();
		events[index]?.Invoke();
	}

	public void InvokeEndEvent() => endEvent?.Invoke();

	public void AddActionToEnd(Action action)
	{
		if (action == null) return;
		endEvent.AddListener(new UnityAction(action));
	}

	public DialogueLineEvent[] GetLines() => conversationEvent.conversation;

	public EntityProfile[] GetSpeakers() => conversationEvent.speakers;

	public ConversationEventPosition GetNextConversation()
		=> conversationEvent.GetNextConversation();

	public void EnsureLength()
	{
		while (Length > events.Count)
		{
			events.Add(new UnityEvent());
		}
	}
}
