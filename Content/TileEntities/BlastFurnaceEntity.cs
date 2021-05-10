using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.API.UI;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class BlastFurnaceEntity : MachineEntity{
		public override int MachineTile => ModContent.TileType<BlastFurnace>();

		public override int SlotsCount => 10;

		public static Dictionary<int, (int, int)> ingredientToResult;

		public bool ForceNoReaction = false;

		//Used for sound stuff
		private SoundEffectInstance burning;

		public override void PreUpdateReaction(){
			Item input = ParentState?.GetSlot(0).StoredItem ?? GetItem(0);
			Item fuel = ParentState?.GetSlot(1).StoredItem ?? GetItem(1);

			ReactionInProgress = !ForceNoReaction && !input.IsAir && !fuel.IsAir;

			if(!ReactionInProgress)
				ReactionProgress = 0;
		}

		public override bool UpdateReaction(){
			// TODO: Heater blocks that can be powered to speed up the reaction
			//100% progress per 30s
			ReactionProgress += 100f / 30f / 60f;

			return true;
		}

		public override void PostReaction(){
			if(ReactionInProgress && !ForceNoReaction){
				Vector2 center = TileUtils.TileEntityCenter(this, MachineTile);

				burning = Main.PlaySound(SoundLoader.customSoundType, (int)center.X, (int)center.Y, TerraScience.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/CampfireBurning"));
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

			(int, int) result = ingredientToResult[input.type];

			input.stack--;
			if(input.stack <= 0)
				input.TurnToAir();

			fuel.stack--;
			if(fuel.stack <= 0)
				fuel.TurnToAir();
			
			ReactionProgress = 0;

			Vector2 center = TileUtils.TileEntityCenter(this, MachineTile);

			Main.PlaySound(SoundLoader.customSoundType, center, TerraScience.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/Flame Arrow"));

			//Find the first available slot to put the item in
			for(int i = 2; i < SlotsCount; i++){
				Item slot = ParentState?.GetSlot(i).StoredItem ?? GetItem(i);

				if(slot.IsAir){
					slot.SetDefaults(result.Item1);
					slot.stack = result.Item2;
					return;
				}else if(slot.type == result.Item1){
					if(slot.stack + result.Item2 > slot.maxStack){
						int old = slot.stack;
						slot.stack = slot.maxStack;
						result.Item2 -= slot.stack - old;

						if(result.Item2 <= 0)
							return;
					}else{
						slot.stack += result.Item2;
						return;
					}
				}
			}

			//Ran out of space
			ForceNoReaction = true;
		}
	}
}
