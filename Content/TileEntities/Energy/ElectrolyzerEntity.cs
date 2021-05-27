using System.Collections.Generic;
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
	public class ElectrolyzerEntity : PoweredMachineEntity, ILiquidMachine, IGasMachine{
		public const int MaxPlaceDelay = 12;

		public float CurBatteryCharge = 0f;
		public const float BatteryMax = 9f;

		public override int MachineTile => ModContent.TileType<Electrolyzer>();

		public override TerraFlux FluxCap => new TerraFlux(5000f);

		public override TerraFlux FluxUsage => new TerraFlux(1000f);

		public override int SlotsCount => 5;
		
		public LiquidEntry[] LiquidEntries{ get; set; } = new LiquidEntry[]{
			new LiquidEntry(30f, isInput: true, MachineLiquidID.Water, MachineLiquidID.Saltwater)
		};

		public GasEntry[] GasEntries{ get; set; } = new GasEntry[]{
			new GasEntry(100f, isInput: false, MachineGasID.Hydrogen),
			new GasEntry(100f, isInput: false, MachineGasID.Oxygen, MachineGasID.Chlorine)
		};

		public int LiquidPlaceDelay{ get; set; }
		public int GasPlaceDelay{ get; set; }

		public static Dictionary<MachineLiquidID, (MachineGasID, MachineGasID)> liquidToGas;

		public override void ExtraLoad(TagCompound tag){
			base.ExtraLoad(tag);

			CurBatteryCharge = tag.GetFloat(nameof(CurBatteryCharge));

			this.LoadLiquids(tag);
			this.LoadGases(tag);
		}

		public override TagCompound ExtraSave(){
			var baseTag = base.ExtraSave();
			baseTag.Add(nameof(CurBatteryCharge), CurBatteryCharge);

			this.SaveLiquids(baseTag);
			this.SaveGases(baseTag);

			return baseTag;
		}

		public override bool UpdateReaction(){
			LiquidEntries[0].current -= 0.3333f / 60f;

			LiquidEntries[0].current = Utils.Clamp(LiquidEntries[0].current, 0f, LiquidEntries[0].max);

			//Moderate chance to produce something every tick
			if(Main.rand.NextFloat() < 0.1f / 60f)
				ReactionProgress = 100;

			return ReactionProgress == 100;
		}

		public override void ReactionComplete(){
			//1000TF per operation
			if(CurBatteryCharge <= 0f || !CheckFluxRequirement(FluxUsage))
				return;

			(MachineGasID gas, MachineGasID gas2) = liquidToGas[LiquidEntries[0].id];
			GasEntries[0].id = gas;
			GasEntries[1].id = gas2;

			GasEntries[0].current++;
			GasEntries[1].current++;

			if(LiquidEntries[0].current == 0)
				LiquidEntries[0].id = MachineLiquidID.None;

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

			bool canOutput1 = gas1Out.IsAir || GasEntries[0].id == MachineGasID.None || (GasEntries[0].id != MachineGasID.None && gas1Out.modItem is Capsule capsule && capsule.GasType == GasEntries[0].id);
			bool canOutput2 = gas2Out.IsAir || GasEntries[1].id == MachineGasID.None || (GasEntries[1].id != MachineGasID.None && gas2Out.modItem is Capsule capsule2 && capsule2.GasType == GasEntries[1].id);

			ReactionInProgress = CurBatteryCharge > 0
				&& LiquidEntries[0].current > 0
				&& GasEntries[0].current < GasEntries[0].max
				&& GasEntries[1].current < GasEntries[1].max
				&& canOutput1
				&& canOutput2;
		}

		public override void PostReaction(){
			//Update the delay timer
			if(LiquidPlaceDelay > 0)
				LiquidPlaceDelay--;

			if(LiquidEntries[0].current <= 0)
				LiquidEntries[0].id = MachineLiquidID.None;

			(MachineGasID gas, MachineGasID gas2) = LiquidEntries[0].id != MachineLiquidID.None ? liquidToGas[LiquidEntries[0].id] : (MachineGasID.None, MachineGasID.None);

			//Check whether a capsule can be filled or not
			Item gas1Input = this.RetrieveItem(1);
			Item gas1Output = this.RetrieveItem(2);
			TileEntityUtils.UpdateOutputSlot(gas, gas1Input, gas1Output, ref GasEntries[0].current);

			if(GasEntries[0].current <= 0)
				GasEntries[0].id = MachineGasID.None;

			Item gas2Input = this.RetrieveItem(3);
			Item gas2Output = this.RetrieveItem(4);
			TileEntityUtils.UpdateOutputSlot(gas2, gas2Input, gas2Output, ref GasEntries[1].current);

			if(GasEntries[1].current <= 0)
				GasEntries[1].id = MachineGasID.None;
		}

		internal override int[] GetInputSlots() => new int[]{ 0, 1, 3 };

		internal override int[] GetOutputSlots() => new int[]{ 2, 4 };

		internal override bool CanInputItem(int slot, Item item)
			=> (slot == 0 && item.type == ModContent.ItemType<Battery9V>()) || ((slot == 1 || slot == 3) && item.modItem is Capsule capsule && capsule.GasType == MachineGasID.None);

		public void TryExportLiquid(Point16 pumpPos){ }

		public void TryImportLiquid(Point16 pipePos) => this.TryImportLiquids(pipePos, 0);

		public void TryExportGas(Point16 pumpPos){
			//Pump needs to be connected to one of the tanks
			Point16 orig = Position;

			if(pumpPos == orig + new Point16(0, -1) || pumpPos == orig + new Point16(-1, 0) || pumpPos == orig + new Point16(-1, 1))
				this.TryExportGases(pumpPos, 0);

			(ModContent.GetModTile(MachineTile) as Machine).GetDefaultParams(out _, out uint width, out _, out _);

			int use = (int)width - 1;

			if(pumpPos == orig + new Point16(use, -1) || pumpPos == orig + new Point16(use + 1, 0) || pumpPos == orig + new Point16(use + 1, 1))
				this.TryExportGases(pumpPos, 1);
		}

		public void TryImportGas(Point16 pipePos){ }
	}
}
