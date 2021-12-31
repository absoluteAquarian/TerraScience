using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class LiquidDuplicatorEntity : PoweredMachineEntity, IFluidMachine{
		//Usage depends on what liquid is being duplicated
		public override TerraFlux FluxUsage => TerraFlux.Zero;

		public override TerraFlux FluxCap => new TerraFlux(2000f);

		public override int MachineTile => ModContent.TileType<LiquidDuplicator>();

		public override int SlotsCount => 1;

		public FluidEntry[] FluidEntries{ get; set; } = new FluidEntry[]{
			new FluidEntry(max: 10f, isInput: false, null)
		};

		public int FluidPlaceDelay{ get; set; }

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);

			this.LoadFluids(tag);
		}

		public override TagCompound ExtraSave(){
			var tag = base.ExtraSave();

			this.SaveFluids(tag);

			return tag;
		}

		public override void ExtraNetSend(BinaryWriter writer){
			base.ExtraNetSend(writer);

			this.SendFluids(writer);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			base.ExtraNetReceive(reader);

			this.ReceiveFluids(reader);
		}

		public override void PreUpdateReaction(){
			Item input = this.RetrieveItem(0);

			if((ParentState?.Active ?? false) && ParentState.GetSlot(0).ItemTypeChanged)
				ReactionProgress = 0;

			var entry = FluidEntries[0];

			ReactionInProgress = !input.IsAir
				&& (entry.id == MachineFluidID.None || entry.id == MiscUtils.GetFluidIDFromItem(input.type))
				&& entry.current + 1 <= entry.max;
		}

		public override bool UpdateReaction(){
			Item input = this.RetrieveItem(0);

			float flux;
			float time;
			switch(MiscUtils.GetFluidIDFromItem(input.type)){
				case MachineFluidID.LiquidWater:
					flux = 100f;
					time = 5f;
					break;
				case MachineFluidID.LiquidSaltwater:
					flux = 150f;
					time = 5f;
					break;
				case MachineFluidID.LiquidLava:
					flux = 300f;
					time = 10f;
					break;
				case MachineFluidID.LiquidHoney:
					flux = 200f;
					time = 15f;
					break;
				default:
					throw new Exception("TerraScience internal error -- unknown liquid ID");
			}

			if(!CheckFluxRequirement(new TerraFlux(flux / 60f), use: true))
				return false;

			ReactionProgress += time / 60f;

			return ReactionProgress >= 100f;
		}

		public override void ReactionComplete(){
			ReactionProgress = 0f;

			var entry = FluidEntries[0];

			MachineFluidID id = MiscUtils.GetFluidIDFromItem(this.RetrieveItem(0).type);

			if(entry.id == MachineFluidID.None)
				entry.id = id;

			entry.current++;

			this.PlaySound(SoundID.Splash, TileUtils.TileEntityCenter(this, MachineTile));
		}

		internal override bool CanInputItem(int slot, Item item) => false;

		internal override int[] GetInputSlots() => new int[0];

		internal override int[] GetOutputSlots() => new int[0];

		public void TryExportFluid(Point16 pumpPos)
			=> this.TryExportFluids(pumpPos, 0);

		public void TryImportFluid(Point16 pipePos){ }
	}
}
