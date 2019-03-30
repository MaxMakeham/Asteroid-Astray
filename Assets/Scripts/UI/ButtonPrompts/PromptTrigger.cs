﻿using UnityEngine;

public class PromptTrigger : VicinityTrigger
{
	private static PromptUI promptUI;
	[SerializeField] protected string text;
	[SerializeField] protected float fadeInTime = 0f, fadeOutTime = 0f;
	protected bool disablePrompt = false;
	private PromptUI PromptUI { get { return promptUI ?? (promptUI = FindObjectOfType<PromptUI>()); } }

	public void DisablePrompt(bool disable)
	{
		disablePrompt = disable;
		if (!disable) return;
		DeactivatePrompt();
	}

	protected override void EnterTrigger()
	{
		base.EnterTrigger();
		if (disablePrompt) return;

		PromptUI?.ActivatePrompt(text, fadeInTime);
	}

	protected override void ExitTrigger()
	{
		base.ExitTrigger();
		if (disablePrompt) return;

		DeactivatePrompt();
	}

	private void DeactivatePrompt() => PromptUI?.DeactivatePrompt(text, fadeOutTime);
}