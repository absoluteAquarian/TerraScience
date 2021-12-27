using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy{
	public class ElectrolyzerEntity : PoweredMachineEntity, IFluidMachine{
		public const int MaxPlaceDelay = 12;

		public float CurBatteryCharge = 0f;
		public const float BatteryMax = 9f;

		public override int MachineTile => ModContent.TileType<Electrolyzer>();

		public override TerraFlux FluxCap => new TerraFlux(5000f);

		public override TerraFlux FluxUsage => new TerraFlux(1000f);

		public override int SlotsCount => 5;
		
		public FluidEntry[] FluidEntries{ get; set; } = new FluidEntry[]{
			new FluidEntry(30f, isInput: true, MachineFluidID.LiquidWater, MachineFluidID.LiquidSaltwater),
			new FluidEntry(100f, isInput: false, MachineFluidID.HydrogenGas),
			new FluidEntry(100f, isInput: false, MachineFluidID.OxygenGas, MachineFluidID.ChlorineGas)
		};

		public int FluidPlaceDelay{ get; set; }

		public static Dictionary<MachineFluidID, (MachineFluidID, MachineFluidID)> liquidToGas;

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);

			CurBatteryCharge = tag.GetFloat(nameof(CurBatteryCharge));

			this.LoadFluids(tag);
		}

		public override TagCompound ExtraSave(){
			var baseTag = base.ExtraSave();
			baseTag.Add(nameof(CurBatteryCharge), CurBatteryCharge);

			this.SaveFluids(baseTag);

			return baseTag;
		}

		public override void ExtraNetSend(BinaryWriter writer){
			base.ExtraNetSend(writer);

			this.SendFluids(writer);

			writer.Write(CurBatteryCharge);
		}

		public override void ExtraNetReceive(BinaryReader reader){
			base.ExtraNetReceive(reader);

			this.ReceiveFluids(reader);

			CurBatteryCharge = reader.ReadSingle();
		}

		public override bool UpdateReaction(){
			FluidEntries[0].current -= 0.3333f / 60f;

			FluidEntries[0].current = Utils.Clamp(FluidEntries[0].current, 0f, FluidEntries[0].max);

			//Moderate chance to produce something every tick
			if(Main.rand.NextFloat() < 0.1f / 60f)
				ReactionProgress = 100;

			return ReactionProgress == 100;
		}

		public override void ReactionComplete(){
			//1000TF per operation
			if(CurBatteryCharge <= 0f || !CheckFluxRequirement(FluxUsage))
				return;

			(MachineFluidID gas, MachineFluidID gas2) = liquidToGas[FluidEntries[0].id];
			FluidEntries[1].id = gas;
			FluidEntries[2].id = gas2;

			FluidEntries[1].current++;
			FluidEntries[2].current++;

			if(FluidEntries[0].current == 0)
				FluidEntries[0].id = MachineFluidID.None;

			this.PlaySound(SoundID.Item85, TileUtils.TileEntityCenter(this, MachineTile));
		}

		public override void PreUpdateReaction(){
			//Battery lasts for 30s
			if((float)StoredFlux <= 0f)
				CurBatteryCharge -= BatteryMax / 30f / 60f;
			else if(CheckFluxRequirement(FluxUsage, use: false))
				CurBatteryCharge += BatteryMax / 30f / 60f;

			CurBatteryCharge = Utils.Clamp(CurBatteryCharge, 0f, BatteryMax);

			Item battery = this.RetrieveItem(0);

			if(!battery.IsAir && CurBatteryCharge <= 0){
				battery.stack--;

				if(battery.stack <= 0)
					battery.TurnToAir();

				CurBatteryCharge = BatteryMax;
			}

			Item gas1Out = this.RetrieveItem(2);
			Item gas2Out = this.RetrieveItem(4);

			bool canOutput1 = gas1Out.IsAir || FluidEntries[1].id == MachineFluidID.None || (FluidEntries[1].id != MachineFluidID.None && gas1Out.modItem is Capsule capsule && capsule.FluidType == FluidEntries[1].id);
			bool canOutput2 = gas2Out.IsAir || FluidEntries[2].id == MachineFluidID.None || (FluidEntries[2].id != MachineFluidID.None && gas2Out.modItem is Capsule capsule2 && capsule2.FluidType == FluidEntries[2].id);

			ReactionInProgress = CurBatteryCharge > 0
				&& FluidEntries[0].current > 0
				&& FluidEntries[1].current < FluidEntries[1].max
				&& FluidEntries[2].current < FluidEntries[2].max
				&& canOutput1
				&& canOutput2;
		}

		public override void PostReaction(){
			//Update the delay timer
			if(FluidPlaceDelay > 0)
				FluidPlaceDelay--;

			if(FluidEntries[0].current <= 0)
				FluidEntries[0].id = MachineFluidID.None;

			(MachineFluidID gas, MachineFluidID gas2) = FluidEntries[0].id != MachineFluidID.None ? liquidToGas[FluidEntries[0].id] : (MachineFluidID.None, MachineFluidID.None);

			//Check whether a capsule can be filled or not
			Item gas1Input = this.RetrieveItem(1);
			Item gas1Output = this.RetrieveItem(2);
			TileEntityUtils.UpdateOutputSlot(gas, gas1Input, gas1Output, ref FluidEntries[1].current);

			if(FluidEntries[1].current <= 0)
				FluidEntries[1].id = MachineFluidID.None;

			Item gas2Input = this.RetrieveItem(3);
			Item gas2Output = this.RetrieveItem(4);
			TileEntityUtils.UpdateOutputSlot(gas2, gas2Input, gas2Output, ref FluidEntries[2].current);

			if(FluidEntries[2].current <= 0)
				FluidEntries[2].id = MachineFluidID.None;
		}

		internal override int[] GetInputSlots() => new int[]{ 0, 1, 3 };

		internal override int[] GetOutputSlots() => new int[]{ 2, 4 };

		internal override bool CanInputItem(int slot, Item item)
			=> (slot == 0 && item.type == ModContent.ItemType<Battery9V>()) || ((slot == 1 || slot == 3) && item.modItem is Capsule capsule && capsule.FluidType == MachineFluidID.None);

		public void TryImportFluid(Point16 pipePos) => this.TryImportFluids(pipePos, 0);

		public void TryExportFluid(Point16 pumpPos){
			//Pump needs to be connected to one of the tanks
			Point16 orig = Position;

			if(pumpPos == orig + new Point16(0, -1) || pumpPos == orig + new Point16(-1, 0) || pumpPos == orig + new Point16(-1, 1))
				this.TryExportFluids(pumpPos, 1);

			(ModContent.GetModTile(MachineTile) as Machine).GetDefaultParams(out _, out uint width, out _, out _);

			int use = (int)width - 1;

			if(pumpPos == orig + new Point16(use, -1) || pumpPos == orig + new Point16(use + 1, 0) || pumpPos == orig + new Point16(use + 1, 1))
				this.TryExportFluids(pumpPos, 2);
		}
	}
}
