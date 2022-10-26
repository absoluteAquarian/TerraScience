using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class BlastFurnaceEntity : MachineEntity{
		public override int MachineTile => ModContent.TileType<BlastFurnace>();

		public override int SlotsCount => 10;

		public static Dictionary<int, (int requireStack, int resultType, int resultStack)> ingredientToResult;

		public bool ForceNoReaction = false;

		//Used for sound stuff, although it doesn't work yet
		private SoundEffectInstance burning;

		public override void ExtraNetSend(BinaryWriter writer){
			writer.Write(ForceNoReaction);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			ForceNoReaction = reader.ReadBoolean();
		}

		public override void PreUpdateReaction(){
			Item input = ParentState?.GetSlot(0).StoredItem ?? GetItem(0);
			Item fuel = ParentState?.GetSlot(1).StoredItem ?? GetItem(1);

			if(ingredientToResult.TryGetValue(input.type, out (int requireStack, int resultType, int resultStack) tuple)){
				if(input.stack >= tuple.requireStack && ForceNoReaction)
					ForceNoReaction = false;
			}

			ReactionInProgress = !ForceNoReaction && !input.IsAir && !fuel.IsAir;

			if(!ReactionInProgress)
				ReactionProgress = 0;
		}

		public override bool UpdateReaction(){
			// TODO: Heater blocks that can be powered to speed up the reaction
			//100% progress per 5s
			ReactionProgress += 100f / 5f / 60f;

			return true;
		}

		public override void PostReaction(){
			if(ReactionInProgress && !ForceNoReaction){
				Vector2 center = TileUtils.TileEntityCenter(this, MachineTile);

				// TODO: somehow save the sound played to be stopped later
				PlayCustomSound(center, "CampfireBurning");
			}else
				burning?.Stop();
		}

		public override void ReactionComplete(){
			Item input, fuel;
			if(ParentState is null){
				input = GetItem(0);
				fuel = GetItem(1);
			}else{
				input = ParentState.GetSlot(0).StoredItem;
				fuel = ParentState.GetSlot(1).StoredItem;
			}

			//Failsafe
			if(!ingredientToResult.ContainsKey(input.type)){
				ReactionProgress = 0;
				ReactionInProgress = false;
				ForceNoReaction = true;
				return;
			}

			(int requireStack, int resultType, int resultStack) = ingredientToResult[input.type];

			if(input.stack < requireStack){
				ForceNoReaction = true;
				ReactionProgress = 0;
				return;
			}

			input.stack -= requireStack;
			if(input.stack <= 0)
				input.TurnToAir();

			fuel.stack--;
			if(fuel.stack <= 0)
				fuel.TurnToAir();
			
			ReactionProgress = 0;

			Vector2 center = TileUtils.TileEntityCenter(this, MachineTile);

			this.PlayCustomSound(center, "Flame Arrow");

			//Find the first available slot to put the item in
			for(int i = 2; i < SlotsCount; i++){
				Item slot = ParentState?.GetSlot(i).StoredItem ?? GetItem(i);

				if(slot.IsAir){
					slot.SetDefaults(resultType);
					slot.stack = resultStack;
					return;
				}else if(slot.type == resultType){
					if(slot.stack + resultStack > slot.maxStack){
						int old = slot.stack;
						slot.stack = slot.maxStack;
						resultStack -= slot.stack - old;

						if(resultStack <= 0)
							return;
					}else{
						slot.stack += resultStack;
						return;
					}
				}
			}

			//Ran out of space
			ForceNoReaction = true;
		}

		internal override int[] GetInputSlots() => new int[]{ 0, 1 };

		internal override int[] GetOutputSlots() => new int[]{ 2, 3, 4, 5, 6, 7, 8, 9 };

		internal override bool CanInputItem(int slot, Item item)
			=> (slot == 0 && ItemUtils.IsOre(item)) || (slot == 1 && item.type == ModContent.ItemType<Coal>());
	}
}
