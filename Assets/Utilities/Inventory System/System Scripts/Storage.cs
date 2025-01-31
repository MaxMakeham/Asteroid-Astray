﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
	public class Storage : MonoBehaviour
	{
		//index, type, new amount
		public event Action<int, Item.Type, int> OnStackUpdated;
		//old size, new size
		public event Action<int, int> OnSizeChanged;

		[SerializeField] private string inventoryName;
		[SerializeField] private int size = 10;
		[SerializeField] private bool noLimit = false;
		[SerializeField] private List<ItemStack> itemStacks = new List<ItemStack>();

		private void Awake()
		{
			TrimPadStacks();
		}

		public string InventoryName => inventoryName;

		public int Size => size;

		public List<ItemStack> ItemStacks
		{
			get => itemStacks;
			private set => itemStacks = value;
		}

		public void SetData(InventoryData data)
		{
			if (data.stacks != null)
			{
				size = data.size;
				ItemStacks = data.stacks;
			}
			TrimPadStacks();
		}

		public int AmountOfItem(Item.Type type)
		{
			int amount = 0;
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				ItemStack stack = ItemStacks[i];
				if (stack.ItemType== type)
				{
					amount += stack.Amount;
				}
			}
			return amount;
		}

		private int EmptySlotCount
			=> ItemStack.GetEmptyCount(ItemStacks);

		public bool ContainsAnyItems => EmptySlotCount< size;

		public int Count(Item.Type? include = null, int minRarity = Item.MIN_RARITY,
			int maxRarity = Item.MAX_RARITY)
			=> ItemStack.Count(ItemStacks, include, minRarity, maxRarity);

		public int AddToStack(Item.Type itemType, int amount, int index)
		{
			if (index < 0 || index >= Size) return amount;
			ItemStack stack = ItemStacks[index];
			if (itemType != stack.ItemType) return amount;
			int leftOver = stack.AddAmount(amount);
			OnStackUpdated?.Invoke(index, itemType, stack.Amount);
			return leftOver;
		}

		/// <summary>
		/// Increments any non-maxed stacks of equivalent item type by amount given.
		/// If no stacks of equivalent type are found, the items are placed in blank slots.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="num"></param>
		/// <returns>Returns number of items left over, due to not having room for them.</returns>
		public int AddItem(Item.Type type, int num = 1)
			=> ItemStack.AddItem(ItemStacks, type, num, noLimit,
				GetStackUpdatedInvoker, GetSizeChangedInvoker);

		/// <summary>
		/// Increments any non-maxed stacks of equivalent item type by amount given.
		/// If no stacks of equivalent type are found, the items are placed in blank slots.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="num"></param>
		/// <returns>Returns number of items left over, due to not having room for them.</returns>
		public int AddItem(ItemStack stack)
			=> AddItem(stack.ItemType, stack.Amount);

		/// <summary>
		/// Adds item stacks to inventory.
		/// </summary>
		/// <param name="items"></param>
		/// <returns>Returns list of stacks that could not be added, due to not having room for them.</returns>
		public List<ItemStack> AddItems(List<ItemStack> items)
			=> ItemStack.AddItems(ItemStacks, items, noLimit,
				GetStackUpdatedInvoker, GetSizeChangedInvoker);

		public bool RemoveItem(Item.Type type, int num = 1)
			=> ItemStack.RemoveItem(ItemStacks, type, num, GetStackUpdatedInvoker);

		public bool RemoveItem(ItemStack stack)
			=> ItemStack.RemoveItem(ItemStacks, stack.ItemType,
				stack.Amount, GetStackUpdatedInvoker);

		public void RemoveItems(List<ItemStack> items)
			=> ItemStack.RemoveItems(ItemStacks, items, GetStackUpdatedInvoker);

		public List<ItemStack> CreateCopyOfStacks()
			=> ItemStack.CreateCopyOfStacks(ItemStacks);

		public int SpaceLeftForItemType(Item.Type type, bool includeEmptyStacks)
			=> ItemStack.SpaceLeftForItemType(ItemStacks, type, includeEmptyStacks);

		public bool CanFit(List<ItemStack> items)
		{
			if (noLimit) return true;
			return ItemStack.CanFit(ItemStacks, items);
		}

		public bool CanFit(ItemStack stack) => ItemStack.CanFit(ItemStacks, stack);

		public int NonEmptyCount => ItemStack.GetNonEmptyCount(ItemStacks);

		public bool Contains(Item.Type type, int amount)
			=> ItemStack.Count(ItemStacks, type) >= amount;

		public bool Contains(ItemStack stack)
			=> Contains(stack.ItemType, stack.Amount);

		public bool Contains(List<ItemStack> stacks)
		{
			for (int i = 0; i < stacks.Count; i++)
			{
				Item.Type type = stacks[i].ItemType;
				int expectedAmount = ItemStack.Count(stacks, type);
				if (!Contains(type, expectedAmount)) return false;
			}
			return true;
		}

		public bool SetStacks(List<ItemStack> newStacks)
		{
			if (newStacks.Count > size) return false;
			ItemStacks = newStacks;
			TrimPadStacks();
			return true;
		}

		private void TrimPadStacks()
		{
			if (ItemStacks == null)
			{
				ItemStacks = new List<ItemStack>();
			}

			while (ItemStacks.Count < size)
			{
				ItemStacks.Add(new ItemStack());
			}

			if (ItemStacks.Count > size)
			{
				ItemStacks.RemoveRange(size, ItemStacks.Count - size);
			}
		}

		public void ClearAll()
		{
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				ItemStacks[i].SetBlank();
			}
			TrimPadStacks();
		}

		public int[] CountRarities(Item.Type? exclude = null)
		{
			int[] counts = new int[Item.MAX_RARITY + 1];
			bool fltr = exclude != null;

			for (int i = 0; i < ItemStacks.Count; i++)
			{
				ItemStack stack = ItemStacks[i];
				if (fltr && stack.ItemType== exclude) continue;
				int rarity = Item.TypeRarity(stack.ItemType);
				counts[rarity] += stack.Amount;
			}

			return counts;
		}

		public void RemoveByRarity(int rarity, int amount, Item.Type? exclude = null)
		{
			for (int i = ItemStacks.Count - 1; i >= 0; i--)
			{
				Item.Type type = ItemStacks[i].ItemType;
				if (type == exclude) continue;

				if (Item.TypeRarity(type) == rarity)
				{
					int stackAmount = ItemStacks[i].Amount;
					if (stackAmount > 0)
					{
						ItemStacks[i].Amount = stackAmount - amount;
						amount -= stackAmount - ItemStacks[i].Amount;
					}
				}

				if (amount <= 0) return;
			}

			if (amount > 0)
			{
				Debug.Log(string.Format("Unable to remove {0} items with {1} rarity.", amount, rarity));
			}
		}

		public void Swap(int a, int b)
		{
			if (a < 0 || b < 0 || a >= ItemStacks.Count || b >= ItemStacks.Count || a == b) return;

			//Item.Type typeA = inventory[a].GetItemType();
			//int amountA = inventory[a].GetAmount();
			//Item.Type typeB = inventory[b].GetItemType();
			//int amountB = inventory[b].GetAmount();

			//inventory[a].SetItemType(typeB);
			//inventory[a].SetAmount(amountB);
			//inventory[b].SetItemType(typeA);
			//inventory[b].SetAmount(amountA);
			ItemStack temp = ItemStacks[a];
			ItemStacks[a] = ItemStacks[b];
			ItemStacks[b] = temp;
		}

		public bool Insert(Item.Type type, int amount, int place)
		{
			if (place < 0 || place >= ItemStacks.Count) return false;

			if (ItemStacks[place].ItemType== Item.Type.Blank)
			{
				ItemStacks[place].ItemType = type;
				ItemStacks[place].Amount = amount;
				return true;
			}
			else
			{
				bool forward = false;
				int i = place + 1;
				for (; i < ItemStacks.Count; i++)
				{
					if (ItemStacks[i].ItemType== Item.Type.Blank)
					{
						forward = true;
						break;
					}
				}

				bool backward = false;
				if (!forward)
				{
					i = place - 1;
					for (; i >= 0; i--)
					{
						if (ItemStacks[i].ItemType== Item.Type.Blank)
						{
							backward = true;
							break;
						}
					}
				}

				if (!forward && !backward)
				{
					return false;
				}
				else
				{
					for (; ; i += forward ? -1 : 1)
					{
						if (i == place)
						{
							ItemStacks[place].ItemType = type;
							ItemStacks[place].Amount = amount;
							break;
						}
						Swap(i, i + (forward ? -1 : 1));
					}
					return true;
				}
			}
		}

		public ItemStack Replace(ItemStack stack, int place)
		{
			ItemStack temp = ItemStacks[place];
			ItemStacks[place] = stack;
			OnStackUpdated?.Invoke(place, stack.ItemType, stack.Amount);
			return temp;
		}

		public int Value
		{
			get
			{
				int value = 0;
				for (int i = 0; i < ItemStacks.Count; i++)
				{
					value += ItemStacks[i].Value;
				}
				return value;
			}
		}

		public int FirstInstanceId(Item.Type type)
		{
			for (int i = 0; i < ItemStacks.Count; i++)
			{
				if (ItemStacks[i].ItemType== type) return i;
			}
			return -1;
		}

		public Action<int, Item.Type, int> GetStackUpdatedInvoker
			=> (int index, Item.Type type, int amount) => OnStackUpdated?.Invoke(index, type, amount);

		public Action<int, int> GetSizeChangedInvoker
			=> (int oldSize, int newSize) => OnSizeChanged?.Invoke(oldSize, newSize);

		public InventoryData GetInventoryData() => new InventoryData(ItemStacks, size);

		[Serializable]
		public struct InventoryData
		{
			public List<ItemStack> stacks;
			public int size;

			public InventoryData(List<ItemStack> stacks, int size)
			{
				this.stacks = stacks;
				this.size = size;
			}
		}
	}
}