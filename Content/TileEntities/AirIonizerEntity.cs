using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class AirIonizerEntity : MachineEntity{
		public override int MachineTile => ModContent.TileType<AirIonizer>();

		public static List<int> ResultTypes;

		public static List<double> ResultWeights;

		public static List<int> ResultStacks;

		public override void PreUpdateReaction(){
			//If 'slots' is empty here, then the tile was just placed (since saving/loading would set the size properly)
			ValidateSlots(10);

			//Always try to collect air, unless one of the slots would be full if another result stack was added to it
			for(int i = 0; i < 10; i++){
				Item item;

				if(ParentState?.Active ?? false)
					item = ParentState.GetSlot(i).StoredItem;
				else
					item = GetItem(i);

				if(item.IsAir)
					continue;

				for(int j = 0; j < ResultTypes.Count; j++){
					if(ResultTypes[j] == item.type && item.stack + ResultStacks[j] >= item.maxStack){
						ReactionInProgress = false;
						return;
					}
				}
			}
			//If we're here, then no slots were too full
			ReactionInProgress = true;
		}

		public override bool UpdateReaction(){
			//Small chance to get an item each tick
			if(Main.rand.NextFloat() < 0.23f / 60f)
				ReactionProgress = 100;
			return ReactionProgress == 100;
		}

		public override void ReactionComplete(){
			//Initialize the randomness
			TerraScience.wRand.Clear();
			for(int i = 0; i < ResultTypes.Count; i++){
				TerraScience.wRand.Add((ResultTypes[i], ResultStacks[i]), ResultWeights[i]);
			}

			//Do the randomness
			var result = TerraScience.wRand.Get();

			//Parse the result
			int type = result.Item1;
			int stack = result.Item2;

			//Zappy sound
			Main.PlaySound(SoundLoader.customSoundType, TileUtils.TileEntityCenter(this, TileUtils.Structures.AirIonizer), TerraScience.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/Zap"));

			//Then try and either add to an existing stack or insert it into the first available slot
			//Check for existing stacks first, then empty slots
			for(int i = 0; i < 10; i++){
				Item item;

				if(ParentState?.Active ?? false)
					item = ParentState.GetSlot(i).StoredItem;
				else
					item = GetItem(i);

				if(item.IsAir)
					continue;

				if(item.type == type){
					item.stack += stack;

					if(item.stack > item.maxStack)
						item.stack = item.maxStack;

					return;
				}
			}
			for(int i = 0; i < 10; i++){
				Item item;

				if(ParentState?.Active ?? false)
					item = ParentState.GetSlot(i).StoredItem;
				else
					item = GetItem(i);

				if(item.IsAir){
					item.SetDefaults(type);
					item.stack = stack;
					return;
				}
			}
		}
	}
}
