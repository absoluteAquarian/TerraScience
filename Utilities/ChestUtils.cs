using System;
using System.Collections.Generic;
using Terraria;

namespace TerraScience.Utilities{
	public static class ChestUtils{
		/// <summary>
		/// <seealso cref="ChestUtils.FindChestByGuessingImproved(int, int)"/> fails when <paramref name="x"/> and <paramref name="y"/> are to the right and/or down of the chest's X and Y position.
		/// This method does a more proper, check to account for all corners
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static int FindChestByGuessingImproved(int x, int y){
			for(int i = 0; i < Main.maxChests; i++){
				Chest chest = Main.chest[i];

				if(chest != null && chest.x <= x && chest.x + 2 > x && chest.y <= y && chest.y + 2 > y)
					return i;
			}

			return -1;
		}

		public static bool TryInsertItems(this Chest chest, Item data){
			int stack = data.stack;
			Item clone = data.Clone();
			for(int i = 0; i < chest.item.Length; i++){
				Item item = chest.item[i];

				if(item.IsAir || data.type == item.type){
					if(item.IsAir){
						chest.item[i] = clone;
						return true;
					}else if(item.stack + data.stack <= item.maxStack){
						item.stack += clone.stack;
						return true;
					}else{
						clone.stack -= item.maxStack - item.stack;
						item.stack = item.maxStack;
					}
				}
			}

			return data.stack < stack;
		}

		public static bool TryInsertItemsIntoMachineSimulation(this Chest chest, Item data, int[] inputSlots, Func<int, Item, bool> validItemFunc){
			int stack = data.stack;
			Item clone = data.Clone();
			for(int i = 0; i < chest.item.Length; i++){
				if(!validItemFunc(inputSlots[i], clone))
					continue;

				Item item = chest.item[i];

				if(item.IsAir || data.type == item.type){
					if(item.IsAir){
						chest.item[i] = clone;
						return true;
					}else if(item.stack + data.stack <= item.maxStack){
						item.stack += clone.stack;
						return true;
					}else{
						clone.stack -= item.maxStack - item.stack;
						item.stack = item.maxStack;
					}
				}
			}

			return data.stack < stack;
		}

		internal static bool FindViableItemExport(this Chest chest, int stackToExtract, List<Func<Item, bool>> funcs, out List<int> slots){
			slots = null;

			for(int i = 0; i < chest.item.Length; i++){
				Item slot = chest.item[i];

				if(slot.IsAir)
					continue;

				if(funcs.TrueForAny(f => f(slot))){
					if(slots is null)
						slots = new List<int>();

					if(slot.stack >= stackToExtract){
						slots.Add(i);
						return true;
					}else{
						stackToExtract -= slot.stack;
						slots.Add(i);
					}
				}
			}

			return slots is not null;
		}
	}
}
