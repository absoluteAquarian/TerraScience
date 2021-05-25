using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class PulverizerEntity : PoweredMachineEntity{
		public static Dictionary<int, WeightedRandom<(int type, int stack)>> inputToOutputs;

		public override int MachineTile => ModContent.TileType<Pulverizer>();

		public override int SlotsCount => 13;

		public override TerraFlux FluxCap => new TerraFlux(3000f);

		public override TerraFlux FluxUsage => new TerraFlux(100f / 60f);

		public int updateCount;

		public int frameRand = -1;
		public int frameRand2 = -1;

		public override void PreUpdateReaction(){
			ReactionInProgress = !this.RetrieveItem(0).IsAir && CheckFluxRequirement(FluxUsage, use: false);

			this.StopReactionIfOutputSlotsAreFull(1, SlotsCount - 1);

			if(!ReactionInProgress)
				ReactionProgress = 0;

			if(frameRand == -1 || frameRand2 == -1){
				frameRand = Main.rand.Next(0, 3);
				frameRand2 = Main.rand.Next(0, 3);
			}
		}

		public override bool UpdateReaction(){
			CheckFluxRequirement(FluxUsage, use: true);

			updateCount++;

			//Machine speeds up as it works
			ReactionSpeed *= 1f + 0.086f / 60f;

			const float max = 3;
			if(ReactionSpeed > max)
				ReactionSpeed = max;

			//5s to crush one item
			ReactionProgress += 100f / 5f / 60f * ReactionSpeed;

			return ReactionProgress >= 100f;
		}

		public override void ReactionComplete(){
			ReactionProgress = 0;

			Item input = this.RetrieveItem(0);

			(int type, int stack) = inputToOutputs[input.type].Get();

			input.stack--;
			if(input.stack <= 0)
				input.TurnToAir();

			if(type > 0 && stack > 0){
				this.TryInsertOutput(1, SlotsCount - 1, type, stack);

				this.PlaySound(SoundID.Grab, TileUtils.TileEntityCenter(this, MachineTile));
			}

			frameRand = frameRand2;
			frameRand2 = Main.rand.Next(0, 3);
		}

		public override void PostReaction(){
			if(!ReactionInProgress && this.RetrieveItem(0).IsAir){
				ReactionSpeed *= 1f - 1.23f / 60f;

				if(ReactionSpeed < 1f)
					ReactionSpeed = 1f;

				ReactionProgress = 0;
			}
		}

		internal override int[] GetInputSlots() => new int[]{ 0 };

		internal override int[] GetOutputSlots() => new int[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

		internal override bool CanInputItem(int slot, Item item)
			=> slot == 0 && inputToOutputs.ContainsKey(item.type);
	}
}
