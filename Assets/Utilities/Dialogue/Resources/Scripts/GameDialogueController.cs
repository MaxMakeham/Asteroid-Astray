﻿using InputHandlerSystem;
using UnityEngine;

namespace DialogueSystem
{
	public class GameDialogueController : DialogueController
	{
		protected override int GetTextLength(string text)
		{
			return text.CountAfterActionTagFormatting(true);
		}

		protected override void Update()
		{
			if (InputManager.GetInputDown("Scroll Dialogue"))
			{
				Next();
			}

			if (Input.GetKeyDown(KeyCode.K))
			{
				Skip();
			}

			base.Update();
		}
	}
}