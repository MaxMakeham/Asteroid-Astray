﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class InputIconTextMesh : MonoBehaviour
{
	public TextMeshProUGUI textMesh;
	private static List<SpriteAssetInputActionPair> spriteAssetActions = new List<SpriteAssetInputActionPair>(20);
	[SerializeField] [TextArea(1, 3)] private string text;
	private static InputIconSO keyboardIcons, ps4Icons;
	private const string keyboardIconsString = "Keyboard Icons", ps4IconsString = "Ps4 Icons";
	private const string spriteAssetString = " TmpSpriteAsset";

	private void Awake()
	{
		InputHandler.InputModeChanged += UpdateIcon;
		UpdateIcon();
	}

	private void OnDestroy()
	{
		InputHandler.InputModeChanged -= UpdateIcon;
	}

	private void GetIconSoReferences()
	{
		keyboardIcons = keyboardIcons ?? GetInputIconSO(keyboardIconsString);
		ps4Icons = ps4Icons ?? GetInputIconSO(ps4IconsString);
	}

	private InputIconSO GetInputIconSO(string name)
	{
		return Resources.Load<InputIconSO>(name);
	}

	private void UpdateIcon()
	{
		textMesh = textMesh ?? GetComponent<TextMeshProUGUI>();
		string s = ReformatText(text);
		textMesh.text = s;
		textMesh.gameObject.SetActive(false);
		textMesh.gameObject.SetActive(true);
	}

	public string GetText()
	{
		return text;
	}

	public void SetText(string s)
	{
		text = s;
		UpdateIcon();
	}

	private string ReformatText(string input)
	{
		string s = input;

		InputAction[] actions =
			(InputAction[])System.Enum.GetValues(typeof(InputAction));
		for (int i = 0; i < actions.Length; i++)
		{
			InputAction action = actions[i];
			string actionString = InputHandler.GetActionString(actions[i]);
			string check = $"[{actionString}]";
			if (s.Contains(check))
			{
				InputIconSO iconSet = GetIconSet();
				if (iconSet == null) continue;
				KeyCode kc = InputHandler.GetBinding(actions[i]);
				TMP_SpriteAsset tmpSpriteAsset = GetSpriteAsset(actions[i]);
				tmpSpriteAsset.spriteSheet = iconSet.GetSprite(kc).texture;
				tmpSpriteAsset.material.mainTexture = tmpSpriteAsset.spriteSheet;
				s = s.Replace(check, $"<sprite=\"{tmpSpriteAsset.name}\" index=0>" +
					$" <color=#00FFFF>{actionString}</color>");
			}

			check = $"[{actionString}:]";
			if (s.Contains(check))
			{
				InputIconSO iconSet = GetIconSet();
				if (iconSet == null) continue;
				KeyCode kc = InputHandler.GetBinding(actions[i]);
				TMP_SpriteAsset tmpSpriteAsset = GetSpriteAsset(actions[i]);
				tmpSpriteAsset.spriteSheet = iconSet.GetSprite(kc).texture;
				tmpSpriteAsset.material.mainTexture = tmpSpriteAsset.spriteSheet;
				s = s.Replace(check, $"<sprite=\"{tmpSpriteAsset.name}\" index=0>");
			}

			check = $"[:{actionString}]";
			if (s.Contains(check))
			{
				s = s.Replace(check, $"<color=#00FFFF>{actionString}</color>");
			}
		}

		return s;
	}

	private TMP_SpriteAsset GetSpriteAsset(InputAction action)
	{
		for (int i = 0; i < spriteAssetActions.Count; i++)
		{
			if (spriteAssetActions[i].action == action)
			{
				return spriteAssetActions[i].tmpSpriteAsset;
			}
		}
		TMP_SpriteAsset newAsset = Resources.Load<TMP_SpriteAsset>(
			$"Sprite Assets/{InputHandler.GetActionString(action)}{spriteAssetString}");
		spriteAssetActions.Add(new SpriteAssetInputActionPair(newAsset, action));
		return newAsset;
	}

	private InputIconSO GetIconSet()
	{
		GetIconSoReferences();
		switch (InputHandler.GetMode())
		{
			case InputHandler.InputMode.Keyboard: return keyboardIcons;
			case InputHandler.InputMode.Ps4: return ps4Icons;
			default: return null;
		}
	}

	private struct SpriteAssetInputActionPair
	{
		public TMP_SpriteAsset tmpSpriteAsset;
		public InputAction action;

		public SpriteAssetInputActionPair(TMP_SpriteAsset tmpSpriteAsset, InputAction action)
		{
			this.tmpSpriteAsset = tmpSpriteAsset;
			this.action = action;
		}
	}
}