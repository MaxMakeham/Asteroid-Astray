﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputHandler
{
	[System.Serializable]
	public class ActionCombination
	{
		public string actionName;
		[SerializeField] private InputCombination defaultCombination, currentCombination;

		public InputCombination Combination
		{
			get
			{
				if (currentCombination.IsValid) return currentCombination;
				if (defaultCombination.IsValid) return defaultCombination;
				return null;
			}
		}

		public void SetDefaultCombination(InputCombination comb)
			=> defaultCombination = comb;

		public void SetCurrentCombination(InputCombination comb)
			=> currentCombination = comb;

		public InputCombination GetDefaultCombination() => defaultCombination;

		public InputCombination GetCurrentCombination() => currentCombination;

		public InputCombination GetValidCombination()
		{
			if (currentCombination.IsValid) return currentCombination;
			if (defaultCombination.IsValid) return defaultCombination;
			return null;
		}

		public bool Contains(InputCode code) => GetValidCombination().Contains(code);

		public bool AnyInput()
		{
			if (currentCombination.IsValid)
			{
				return currentCombination.AnyInput();
			}
			if (defaultCombination.IsValid)
			{
				return defaultCombination.AnyInput();
			}
			return false;
		}

		public float CombinationInput()
		{
			if (currentCombination.IsValid)
			{
				return currentCombination.CombinationInput();
			}
			if (defaultCombination.IsValid)
			{
				return defaultCombination.CombinationInput();
			}
			return 0f;
		}

		public bool CombinationInputDown()
		{
			if (currentCombination.IsValid)
			{
				return currentCombination.CombinationInputDown();
			}
			if (defaultCombination.IsValid)
			{
				return defaultCombination.CombinationInputDown();
			}
			return false;
		}

		public bool CombinationInputUp()
		{
			if (currentCombination.IsValid)
			{
				return currentCombination.CombinationInputUp();
			}
			if (defaultCombination.IsValid)
			{
				return defaultCombination.CombinationInputUp();
			}
			return false;
		}

		public void ResetToDefault() => currentCombination = new InputCombination();

		public int DefaultCombinationCount => defaultCombination.inputs.Count;

		public int CurrentCombinationCount => currentCombination.inputs.Count;

		public override string ToString()
		{
			return $"Action: {actionName}\n" +
				$"Default Combination: {defaultCombination}\n" +
				$"Current Combination: {currentCombination}";
		}
	}
}
