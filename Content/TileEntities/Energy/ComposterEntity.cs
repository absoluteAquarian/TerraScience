using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class ComposterEntity : PoweredMachineEntity{
		public override int MachineTile => ModContent.TileType<Composter>();

		public override int SlotsCount => 2;

		public override TerraFlux FluxCap => new TerraFlux(1500f);

		public override TerraFlux FluxUsage => new TerraFlux(50 / 60f);

		public static List<(int input, float resultStack)> ingredients;

		public float compostProgress;

		public override TagCompound ExtraSave(){
			var tag = base.ExtraSave();
			tag.Add("compost", compostProgress);
			return tag;
		}

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);
			compostProgress = tag.GetFloat("compost");
		}

		public override void ExtraNetSend(BinaryWriter writer){
			base.ExtraNetSend(writer);

			writer.Write(compostProgress);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			base.ExtraNetReceive(reader);

			compostProgress = reader.ReadSingle();
		}

		public override void PreUpdateReaction(){
			var input = this.RetrieveItem(0);
			var output = this.RetrieveItem(1);

			ReactionInProgress = CheckFluxRequirement(FluxUsage, use: false)  //Has enough power
				&& !input.IsAir && ValidType(input.type, out float result)  //Input is valid
				&& (result + compostProgress < 1 || output.IsAir || output.stack + (int)(compostProgress + result) <= output.maxStack);  //Output isn't full and wouldn't be full

			if(!ReactionInProgress)
				ReactionProgress = 0;
		}

		public override bool UpdateReaction(){
			//One "reaction" per 2 seconds
			ReactionProgress += 100f / (2f * 60f);

			return ReactionProgress >= 100;
		}

		public override void ReactionComplete(){
			//Remove an item from the input and update the compost progress
			//If the compost progress is complete, create a dirt block
			var input = this.RetrieveItem(0);
			var output = this.RetrieveItem(1);

			if(!ValidType(input.type, out float add)){
				ReactionInProgress = false;
				ReactionProgress = 0;
				return;
			}

			//It's valid
			input.stack--;
			if(input.stack <= 0){
				input.type = ItemID.None;
				input.netID = ItemID.None;
				input.stack = 0;
			}
			compostProgress += add;

			while(compostProgress >= 1 && output.stack < output.maxStack){
				compostProgress--;

				if(output.IsAir){
					output.SetDefaults(ItemID.DirtBlock);
					output.stack = 0;
				}

				output.stack++;
			}

			ReactionProgress = 0;
		}

		internal override int[] GetInputSlots() => new int[]{ 0 };

		internal override int[] GetOutputSlots() => new int[]{ 1 };

		internal override bool CanInputItem(int slot, Item item)
			=> slot == 0 && ValidType(item.type, out _);

		public static bool ValidType(int type, out float resultStack){
			foreach((int input, float stack) in ingredients){
				if(type == input){
					resultStack = stack;
					return true;
				}
			}

			resultStack = -1;
			return false;
		}
	}
}
