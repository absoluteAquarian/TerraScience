using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities{
	public class SaltExtractorEntity : MachineEntity, IFluidMachine{
		public override int SlotsCount => 1;

		public override void ExtraLoad(TagCompound tag){
			this.LoadFluids(tag);
		}

		public override TagCompound ExtraSave(){
			TagCompound tag = new TagCompound();

			this.SaveFluids(tag);

			return tag;
		}

		public override void ExtraNetSend(BinaryWriter writer){
			this.SendFluids(writer);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			this.ReceiveFluids(reader);
		}

		public override int MachineTile => ModContent.TileType<SaltExtractor>();

		public FluidEntry[] FluidEntries{ get; set; } = new FluidEntry[]{
			new FluidEntry(max: 10f, isInput: true, MachineFluidID.LiquidWater, MachineFluidID.LiquidSaltwater)
		};

		public int FluidPlaceDelay{ get; set; }

		public override void PreUpdateReaction(){
			Item salt = this.RetrieveItem(0);

			if(FluidEntries[0].current > 0 && salt.stack < 100)
				ReactionInProgress = true;
		}

		public override bool UpdateReaction(){
			float litersLostPerSecond = 0f;
			if(FluidEntries[0].id == MachineFluidID.LiquidWater)
				litersLostPerSecond = 0.05f;
			else if(FluidEntries[0].id == MachineFluidID.LiquidSaltwater)
				litersLostPerSecond = 0.075f;

			float reaction = ReactionSpeed * litersLostPerSecond / 60f;
			FluidEntries[0].current -= reaction;

			if(FluidEntries[0].id == MachineFluidID.LiquidWater)
				ReactionProgress += reaction * 0.5f * 100;
			else if(FluidEntries[0].id == MachineFluidID.LiquidSaltwater)
				ReactionProgress += reaction * 1.5f * 100;

			return true;
		}

		public override void ReactionComplete(){
			Item salt = this.RetrieveItem(0);

			if(salt.IsAir){
				salt.SetDefaults(ModContent.ItemType<Salt>());
				salt.stack = 0;
			}

			salt.stack++;

			// TODO: make method for setting volume to half
			this.PlaySound(SoundID.Grab, Position.ToWorldCoordinates());
		}

		public override void PostUpdateReaction(){
			//Reaction happens faster and faster as it "heats up", but cools down very quickly
			ReactionSpeed *= 1f + 0.0392f / 60f;

			//Reaction speed caps at 2x speed
			if(ReactionSpeed > 2f)
				ReactionSpeed = 2f;
		}

		public override void PostReaction(){
			Item salt = this.RetrieveItem(0);

			if(!ReactionInProgress){
				ReactionSpeed *= 1f - 0.0943f / 60f;

				if(ReactionSpeed < 1f)
					ReactionSpeed = 1f;
			}

			//If there isn't any water left, pause the reaction
			if(FluidEntries[0].current <= 0f && ReactionInProgress){
				ReactionInProgress = false;
				FluidEntries[0].current = 0f;
				FluidEntries[0].id = MachineFluidID.None;
			}

			//Stop the reaction if more than 99 salt items are stored
			if(salt.stack >= 100)
				ReactionInProgress = false;

			//Update the delay timer
			if(FluidPlaceDelay > 0)
				FluidPlaceDelay--;
		}

		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[]{ SlotsCount - 1 };

		internal override bool CanInputItem(int slot, Item item) => false;

		public void TryExportFluid(Point16 pumpPos){ }

		public void TryImportFluid(Point16 pipePos) => this.TryImportFluids(pipePos, 0);
	}
}
